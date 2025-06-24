using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Newtonsoft.Json;

namespace CloudAuth
{
    public enum DataSaveStatus
    {
        NotInitialized,
        Ready,
        Saving,
        Loading,
        Error
    }

    [Serializable]
    public class PlayerData
    {
        public string playerName;
        public int level;
        public float experience;
        public int coins;
        public List<string> achievements;
        public DateTime lastSaveTime;

        public PlayerData()
        {
            playerName = "";
            level = 1;
            experience = 0f;
            coins = 0;
            achievements = new List<string>();
            lastSaveTime = DateTime.UtcNow;
        }
    }

    public class UnityCloudDataService : MonoBehaviour
    {
        #region Constants
        private const string PLAYER_DATA_KEY = "PlayerGameData";
        private const string PLAYER_SETTINGS_KEY = "PlayerSettings";
        private const string SAVE_BACKUP_KEY = "BackupData";
        #endregion

        #region Private Fields
        [Header("Data Service Settings")]
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool _debugMode = true;
        [SerializeField] private bool _enableBackups = true;
        [SerializeField] private bool _autoInitializeServices = true;

        private PlayerData _currentPlayerData;
        private Dictionary<string, object> _customData;
        private float _lastAutoSaveTime;
        #endregion

        #region Public Properties
        public DataSaveStatus CurrentStatus { get; private set; } = DataSaveStatus.NotInitialized;
        public PlayerData CurrentPlayerData => _currentPlayerData;
        public bool IsReady => CurrentStatus == DataSaveStatus.Ready && 
                              UnityServices.State == ServicesInitializationState.Initialized &&
                              AuthenticationService.Instance != null && 
                              AuthenticationService.Instance.IsSignedIn;
        public DateTime LastSaveTime { get; private set; }
        #endregion

        #region Events
        public event Action<DataSaveStatus> OnDataStatusChanged;
        public event Action<PlayerData> OnDataLoaded;
        public event Action<PlayerData> OnDataSaved;
        public event Action<string> OnDataError;
        public event Action OnAutoSaveTriggered;
        #endregion

        #region Unity Lifecycle
        private async void Awake()
        {
            InitializeDataService();
            
            if (_autoInitializeServices)
            {
                await InitializeUnityServicesAsync();
            }
        }

        private void Start()
        {
            SubscribeToAuthEvents();
            
            // If already signed in, load data immediately
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                OnUserSignedIn();
            }
        }

        private void Update()
        {
            HandleAutoSave();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAuthEvents();
        }
        #endregion

        #region Initialization
        private void InitializeDataService()
        {
            _customData = new Dictionary<string, object>();
            _currentPlayerData = new PlayerData();
            SetStatus(DataSaveStatus.NotInitialized);
            
            LogDebug($"Cloud Data Service initialized independently");
        }

        private async Task<bool> InitializeUnityServicesAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                LogDebug("Unity Services already initialized");
                SetStatus(DataSaveStatus.Ready);
                return true;
            }

            try
            {
                LogDebug("Initializing Unity Services...");
                await UnityServices.InitializeAsync();
                SetStatus(DataSaveStatus.Ready);
                LogDebug("Unity Services initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize Unity Services: {ex.Message}");
                SetStatus(DataSaveStatus.Error);
                return false;
            }
        }

        private void SubscribeToAuthEvents()
        {
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn += OnUserSignedIn;
                AuthenticationService.Instance.SignedOut += OnUserSignedOut;
                AuthenticationService.Instance.SignInFailed += OnAuthError;
                AuthenticationService.Instance.Expired += OnUserSignedOut;
            }
        }

        private void UnsubscribeFromAuthEvents()
        {
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= OnUserSignedIn;
                AuthenticationService.Instance.SignedOut -= OnUserSignedOut;
                AuthenticationService.Instance.SignInFailed -= OnAuthError;
                AuthenticationService.Instance.Expired -= OnUserSignedOut;
            }
        }
        #endregion

        #region Authentication Event Handlers
        private async void OnUserSignedIn()
        {
            LogDebug($"User signed in, initializing cloud data for: {GetPlayerName()}");
            
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                SetStatus(DataSaveStatus.Ready);
                // Auto-load player data when signed in
                await LoadPlayerDataAsync();
            }
            else
            {
                LogError("Unity Services not initialized, cannot load data");
            }
        }

        private void OnUserSignedOut()
        {
            LogDebug("User signed out, clearing local data");
            
            // Clear local data
            _currentPlayerData = new PlayerData();
            _customData.Clear();
            SetStatus(DataSaveStatus.NotInitialized);
        }

        private void OnAuthError(RequestFailedException error)
        {
            LogError($"Authentication error: {error.Message}");
            SetStatus(DataSaveStatus.Error);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Get player name directly from Unity Authentication
        /// </summary>
        private string GetPlayerName()
        {
            if (AuthenticationService.Instance?.IsSignedIn == true)
            {
                try
                {
                    return AuthenticationService.Instance.PlayerName ?? "Unknown Player";
                }
                catch
                {
                    return "Unknown Player";
                }
            }
            return "Not Signed In";
        }

        /// <summary>
        /// Get player ID directly from Unity Authentication
        /// </summary>
        private string GetPlayerId()
        {
            return AuthenticationService.Instance?.PlayerId ?? "";
        }

        /// <summary>
        /// Check authentication status directly
        /// </summary>
        public bool IsAuthenticated()
        {
            return AuthenticationService.Instance?.IsSignedIn == true;
        }
        #endregion

        #region Public Data Methods
        /// <summary>
        /// Save player data to Unity Cloud Save
        /// </summary>
        public async Task<bool> SavePlayerDataAsync()
        {
            if (!IsReady)
            {
                LogError("Cannot save data: Service not ready or user not signed in");
                return false;
            }

            try
            {
                SetStatus(DataSaveStatus.Saving);
                LogDebug("Saving player data to cloud...");

                // Update save time
                _currentPlayerData.lastSaveTime = DateTime.UtcNow;

                // Convert to JSON
                string jsonData = JsonConvert.SerializeObject(_currentPlayerData, Formatting.Indented);
                
                // Prepare data for cloud save
                var dataToSave = new Dictionary<string, object>
                {
                    { PLAYER_DATA_KEY, jsonData }
                };

                // Add custom data if any
                foreach (var kvp in _customData)
                {
                    dataToSave[kvp.Key] = kvp.Value;
                }

                // Save to cloud
                await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);

                // Create backup if enabled
                if (_enableBackups)
                {
                    await CreateBackupAsync(jsonData);
                }

                LastSaveTime = DateTime.UtcNow;
                SetStatus(DataSaveStatus.Ready);
                
                LogDebug("Player data saved successfully");
                OnDataSaved?.Invoke(_currentPlayerData);
                return true;
            }
            catch (CloudSaveValidationException ex)
            {
                LogError($"Cloud Save validation error: {ex.Message}");
                SetStatus(DataSaveStatus.Error);
                OnDataError?.Invoke(ex.Message);
                return false;
            }
            catch (CloudSaveRateLimitedException ex)
            {
                LogError($"Cloud Save rate limited: {ex.Message}");
                SetStatus(DataSaveStatus.Error);
                OnDataError?.Invoke("Save operation rate limited. Please try again later.");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save player data: {ex.Message}");
                SetStatus(DataSaveStatus.Error);
                OnDataError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Load player data from Unity Cloud Save
        /// </summary>
        public async Task<bool> LoadPlayerDataAsync()
        {
            if (!IsReady)
            {
                LogError("Cannot load data: Service not ready or user not signed in");
                return false;
            }

            try
            {
                SetStatus(DataSaveStatus.Loading);
                LogDebug("Loading player data from cloud...");

                // Get data from cloud
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { PLAYER_DATA_KEY }
                );

                if (savedData.TryGetValue(PLAYER_DATA_KEY, out var playerDataValue))
                {
                    string jsonData = playerDataValue.Value.GetAs<string>();
                    _currentPlayerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
                    
                    LogDebug("Player data loaded successfully");
                    OnDataLoaded?.Invoke(_currentPlayerData);
                }
                else
                {
                    LogDebug("No saved data found, using default player data");
                    _currentPlayerData = new PlayerData();
                    
                    // Set default name from Unity auth if available
                    string playerName = GetPlayerName();
                    if (!string.IsNullOrEmpty(playerName) && playerName != "Unknown Player")
                    {
                        _currentPlayerData.playerName = playerName;
                    }
                }

                SetStatus(DataSaveStatus.Ready);
                return true;
            }
            catch (CloudSaveException ex)
            {
                LogError($"Failed to load player data: {ex.Message}");
                SetStatus(DataSaveStatus.Error);
                OnDataError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Save custom data with a specified key
        /// </summary>
        public async Task<bool> SaveCustomDataAsync(string key, object data)
        {
            if (!IsReady)
            {
                LogError("Cannot save custom data: Service not ready");
                return false;
            }

            try
            {
                string jsonData = JsonConvert.SerializeObject(data);
                _customData[key] = jsonData;

                var dataToSave = new Dictionary<string, object>
                {
                    { key, jsonData }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
                
                LogDebug($"Custom data saved with key: {key}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save custom data: {ex.Message}");
                OnDataError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Load custom data with a specified key
        /// </summary>
        public async Task<T> LoadCustomDataAsync<T>(string key) where T : class
        {
            if (!IsReady)
            {
                LogError("Cannot load custom data: Service not ready");
                return null;
            }

            try
            {
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { key }
                );

                if (savedData.TryGetValue(key, out var dataValue))
                {
                    string jsonData = dataValue.Value.GetAs<string>();
                    return JsonConvert.DeserializeObject<T>(jsonData);
                }

                return null;
            }
            catch (Exception ex)
            {
                LogError($"Failed to load custom data: {ex.Message}");
                OnDataError?.Invoke(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete all player data from cloud
        /// </summary>
        public async Task<bool> DeleteAllDataAsync()
        {
            if (!IsReady)
            {
                LogError("Cannot delete data: Service not ready");
                return false;
            }

            try
            {
                LogDebug("Deleting all player data from cloud...");

                await CloudSaveService.Instance.Data.Player.DeleteAsync(PLAYER_DATA_KEY);
                
                // Clear local data
                _currentPlayerData = new PlayerData();
                _customData.Clear();

                LogDebug("All player data deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete player data: {ex.Message}");
                OnDataError?.Invoke(ex.Message);
                return false;
            }
        }
        #endregion

        #region Player Data Manipulation
        /// <summary>
        /// Update player level and experience
        /// </summary>
        public void UpdatePlayerProgress(int newLevel, float newExperience)
        {
            _currentPlayerData.level = newLevel;
            _currentPlayerData.experience = newExperience;
            LogDebug($"Player progress updated: Level {newLevel}, XP {newExperience}");
        }

        /// <summary>
        /// Add coins to player data
        /// </summary>
        public void AddCoins(int amount)
        {
            _currentPlayerData.coins += amount;
            LogDebug($"Added {amount} coins. Total: {_currentPlayerData.coins}");
        }

        /// <summary>
        /// Add achievement to player data
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!_currentPlayerData.achievements.Contains(achievementId))
            {
                _currentPlayerData.achievements.Add(achievementId);
                LogDebug($"Achievement unlocked: {achievementId}");
            }
        }

        /// <summary>
        /// Check if player has achievement
        /// </summary>
        public bool HasAchievement(string achievementId)
        {
            return _currentPlayerData.achievements.Contains(achievementId);
        }
        #endregion

        #region Auto Save
        private void HandleAutoSave()
        {
            if (!_autoSaveEnabled || !IsReady)
                return;

            if (Time.time - _lastAutoSaveTime >= _autoSaveInterval)
            {
                TriggerAutoSave();
            }
        }

        private async void TriggerAutoSave()
        {
            _lastAutoSaveTime = Time.time;
            OnAutoSaveTriggered?.Invoke();
            
            LogDebug("Auto-save triggered");
            await SavePlayerDataAsync();
        }

        public void SetAutoSaveEnabled(bool enabled)
        {
            _autoSaveEnabled = enabled;
            LogDebug($"Auto-save {(enabled ? "enabled" : "disabled")}");
        }

        public void SetAutoSaveInterval(float intervalSeconds)
        {
            _autoSaveInterval = Mathf.Max(60f, intervalSeconds); // Minimum 1 minute
            LogDebug($"Auto-save interval set to {_autoSaveInterval} seconds");
        }
        #endregion

        #region Backup System
        private async Task CreateBackupAsync(string jsonData)
        {
            try
            {
                var backupData = new Dictionary<string, object>
                {
                    { SAVE_BACKUP_KEY, jsonData },
                    { "backup_timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(backupData);
                LogDebug("Backup created successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to create backup: {ex.Message}");
            }
        }

        public async Task<bool> RestoreFromBackupAsync()
        {
            if (!IsReady)
                return false;

            try
            {
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { SAVE_BACKUP_KEY }
                );

                if (savedData.TryGetValue(SAVE_BACKUP_KEY, out var backupValue))
                {
                    string jsonData = backupValue.Value.GetAs<string>();
                    _currentPlayerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
                    
                    LogDebug("Data restored from backup");
                    OnDataLoaded?.Invoke(_currentPlayerData);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogError($"Failed to restore from backup: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Utility Methods
        private void SetStatus(DataSaveStatus status)
        {
            if (CurrentStatus != status)
            {
                CurrentStatus = status;
                OnDataStatusChanged?.Invoke(status);
                LogDebug($"Data service status changed to: {status}");
            }
        }

        private void LogDebug(string message)
        {
            if (_debugMode)
            {
                Debug.Log($"[CloudDataService] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[CloudDataService] {message}");
        }

        public string GetStatusInfo()
        {
            var status = $"Status: {CurrentStatus}\n";
            status += $"Unity Services: {UnityServices.State}\n";
            status += $"Authentication: {(IsAuthenticated() ? "Signed In" : "Not Signed In")}\n";
            status += $"Player: {GetPlayerName()} ({GetPlayerId()})\n";
            status += $"Last Save: {LastSaveTime:yyyy-MM-dd HH:mm:ss}";
            return status;
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Force Save Data")]
        private async void EditorForceSave()
        {
            await SavePlayerDataAsync();
        }

        [ContextMenu("Force Load Data")]
        private async void EditorForceLoad()
        {
            await LoadPlayerDataAsync();
        }

        [ContextMenu("Clear All Data")]
        private async void EditorClearData()
        {
            await DeleteAllDataAsync();
        }
#endif
    }
} 