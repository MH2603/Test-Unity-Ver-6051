using System.Collections;
using UnityEngine;
using CloudAuth;

/// <summary>
/// Example script demonstrating how to use the Unity Cloud Data Service
/// This script shows practical usage patterns for saving and loading player data
/// </summary>
public class CloudDataExample : MonoBehaviour
{
    #region Private Fields
    [Header("Services")]
    [SerializeField] private UnityCloudAuthService _authService;
    [SerializeField] private UnityCloudDataService _dataService;

    [Header("Demo Settings")]
    [SerializeField] private bool _runDemoOnStart = false;
    [SerializeField] private float _demoStepDelay = 2f;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        FindServices();
        
        if (_runDemoOnStart && _authService != null && _authService.IsSignedIn)
        {
            StartCoroutine(RunDemoSequence());
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    #endregion

    #region Initialization
    private void FindServices()
    {
        if (_authService == null)
            _authService = FindObjectOfType<UnityCloudAuthService>();

        if (_dataService == null)
            _dataService = FindObjectOfType<UnityCloudDataService>();
    }

    private void SubscribeToEvents()
    {
        if (_authService != null)
        {
            _authService.OnSignedIn += OnUserSignedIn;
        }

        if (_dataService != null)
        {
            _dataService.OnDataLoaded += OnDataLoaded;
            _dataService.OnDataSaved += OnDataSaved;
            _dataService.OnDataError += OnDataError;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_authService != null)
        {
            _authService.OnSignedIn -= OnUserSignedIn;
        }

        if (_dataService != null)
        {
            _dataService.OnDataLoaded -= OnDataLoaded;
            _dataService.OnDataSaved -= OnDataSaved;
            _dataService.OnDataError -= OnDataError;
        }
    }
    #endregion

    #region Event Handlers
    private void OnUserSignedIn(PlayerProfile profile)
    {
        Debug.Log($"[CloudDataExample] User signed in: {profile.Name}");
        
        if (_runDemoOnStart)
        {
            StartCoroutine(RunDemoSequence());
        }
    }

    private void OnDataLoaded(PlayerData data)
    {
        Debug.Log($"[CloudDataExample] Data loaded - Level: {data.level}, Coins: {data.coins}, XP: {data.experience}");
    }

    private void OnDataSaved(PlayerData data)
    {
        Debug.Log($"[CloudDataExample] Data saved - Level: {data.level}, Coins: {data.coins}, XP: {data.experience}");
    }

    private void OnDataError(string error)
    {
        Debug.LogError($"[CloudDataExample] Data error: {error}");
    }
    #endregion

    #region Demo Methods
    /// <summary>
    /// Runs a complete demo sequence showing various data operations
    /// </summary>
    private IEnumerator RunDemoSequence()
    {
        if (_dataService == null || !_dataService.IsReady)
        {
            Debug.LogWarning("[CloudDataExample] Data service not ready for demo");
            yield break;
        }

        Debug.Log("[CloudDataExample] Starting demo sequence...");

        // Step 1: Load existing data
        yield return StartCoroutine(DemoLoadData());
        yield return new WaitForSeconds(_demoStepDelay);

        // Step 2: Modify player data
        yield return StartCoroutine(DemoModifyPlayerData());
        yield return new WaitForSeconds(_demoStepDelay);

        // Step 3: Save the modified data
        yield return StartCoroutine(DemoSaveData());
        yield return new WaitForSeconds(_demoStepDelay);

        // Step 4: Demonstrate custom data saving
        yield return StartCoroutine(DemoCustomData());
        yield return new WaitForSeconds(_demoStepDelay);

        Debug.Log("[CloudDataExample] Demo sequence completed!");
    }

    private IEnumerator DemoLoadData()
    {
        Debug.Log("[CloudDataExample] Demo: Loading player data...");
        
        // Start the async operation
        var loadTask = _dataService.LoadPlayerDataAsync();
        
        // Wait until the task is completed
        yield return new WaitUntil(() => loadTask.IsCompleted);
        
        if (loadTask.Result)
        {
            var data = _dataService.CurrentPlayerData;
            Debug.Log($"[CloudDataExample] Loaded data: {data.playerName}, Level {data.level}");
        }
        else
        {
            Debug.LogError("[CloudDataExample] Failed to load data");
        }
    }

    private IEnumerator DemoModifyPlayerData()
    {
        Debug.Log("[CloudDataExample] Demo: Modifying player data...");

        // Add some coins
        _dataService.AddCoins(500);
        Debug.Log("[CloudDataExample] Added 500 coins");

        // Add experience and potentially level up
        var currentData = _dataService.CurrentPlayerData;
        float newXp = currentData.experience + 1000f;
        int newLevel = Mathf.FloorToInt(newXp / 1000f) + 1; // Simple leveling system
        
        _dataService.UpdatePlayerProgress(newLevel, newXp);
        Debug.Log($"[CloudDataExample] Updated progress: Level {newLevel}, XP {newXp}");

        // Unlock some achievements
        _dataService.UnlockAchievement("Demo Player");
        _dataService.UnlockAchievement("Cloud Save User");
        Debug.Log("[CloudDataExample] Unlocked achievements");

        yield return null;
    }

    private IEnumerator DemoSaveData()
    {
        Debug.Log("[CloudDataExample] Demo: Saving player data...");
        
        // Start the async operation
        var saveTask = _dataService.SavePlayerDataAsync();
        
        // Wait until the task is completed
        yield return new WaitUntil(() => saveTask.IsCompleted);
        
        if (saveTask.Result)
        {
            Debug.Log("[CloudDataExample] Data saved successfully");
        }
        else
        {
            Debug.LogError("[CloudDataExample] Failed to save data");
        }
    }

    private IEnumerator DemoCustomData()
    {
        Debug.Log("[CloudDataExample] Demo: Working with custom data...");

        // Create some custom game settings
        var gameSettings = new GameSettings
        {
            musicVolume = 0.8f,
            soundVolume = 0.9f,
            difficulty = "Normal",
            lastPlayedLevel = "Level_05"
        };

        // Save custom data
        var saveTask = _dataService.SaveCustomDataAsync("GameSettings", gameSettings);
        
        // Wait until the save task is completed
        yield return new WaitUntil(() => saveTask.IsCompleted);
        
        if (saveTask.Result)
        {
            Debug.Log("[CloudDataExample] Custom settings saved");
            
            // Load it back
            var loadTask = _dataService.LoadCustomDataAsync<GameSettings>("GameSettings");
            
            // Wait until the load task is completed
            yield return new WaitUntil(() => loadTask.IsCompleted);
            
            if (loadTask.Result != null)
            {
                Debug.Log($"[CloudDataExample] Loaded settings: Volume {loadTask.Result.musicVolume}, Difficulty {loadTask.Result.difficulty}");
            }
        }
        else
        {
            Debug.LogError("[CloudDataExample] Failed to save custom data");
        }
    }
    #endregion

    #region Public Methods for Testing
    /// <summary>
    /// Simulates earning coins (useful for testing from other scripts)
    /// </summary>
    public async void SimulateEarnCoins(int amount)
    {
        if (_dataService == null || !_dataService.IsReady) return;

        _dataService.AddCoins(amount);
        Debug.Log($"[CloudDataExample] Earned {amount} coins!");
        
        // Auto-save after earning coins
        await _dataService.SavePlayerDataAsync();
    }

    /// <summary>
    /// Simulates gaining experience
    /// </summary>
    public async void SimulateGainExperience(float amount)
    {
        if (_dataService == null || !_dataService.IsReady) return;

        var currentData = _dataService.CurrentPlayerData;
        float newXp = currentData.experience + amount;
        
        // Simple leveling: every 1000 XP = 1 level
        int newLevel = Mathf.FloorToInt(newXp / 1000f) + 1;
        
        _dataService.UpdatePlayerProgress(newLevel, newXp);
        Debug.Log($"[CloudDataExample] Gained {amount} XP! Now Level {newLevel} with {newXp} XP");
        
        // Auto-save after gaining XP
        await _dataService.SavePlayerDataAsync();
    }

    /// <summary>
    /// Unlocks a specific achievement
    /// </summary>
    public async void UnlockAchievement(string achievementId)
    {
        if (_dataService == null || !_dataService.IsReady) return;

        if (!_dataService.HasAchievement(achievementId))
        {
            _dataService.UnlockAchievement(achievementId);
            Debug.Log($"[CloudDataExample] Achievement unlocked: {achievementId}");
            
            // Auto-save after unlocking achievement
            await _dataService.SavePlayerDataAsync();
        }
        else
        {
            Debug.Log($"[CloudDataExample] Achievement already unlocked: {achievementId}");
        }
    }

    /// <summary>
    /// Prints current player data to console
    /// </summary>
    public void PrintPlayerData()
    {
        if (_dataService == null || !_dataService.IsReady)
        {
            Debug.Log("[CloudDataExample] Data service not ready");
            return;
        }

        var data = _dataService.CurrentPlayerData;
        Debug.Log($"[CloudDataExample] Player Data:\n" +
                  $"Name: {data.playerName}\n" +
                  $"Level: {data.level}\n" +
                  $"Experience: {data.experience}\n" +
                  $"Coins: {data.coins}\n" +
                  $"Achievements: {string.Join(", ", data.achievements)}\n" +
                  $"Last Save: {data.lastSaveTime}");
    }
    #endregion

    #region Context Menu Methods (Editor Only)
#if UNITY_EDITOR
    [ContextMenu("Run Demo Sequence")]
    private void EditorRunDemo()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(RunDemoSequence());
        }
        else
        {
            Debug.LogWarning("Demo can only run in Play Mode");
        }
    }

    [ContextMenu("Print Current Data")]
    private void EditorPrintData()
    {
        if (Application.isPlaying)
        {
            PrintPlayerData();
        }
        else
        {
            Debug.LogWarning("Data can only be accessed in Play Mode");
        }
    }

    [ContextMenu("Add Test Coins")]
    private void EditorAddCoins()
    {
        if (Application.isPlaying)
        {
            SimulateEarnCoins(100);
        }
        else
        {
            Debug.LogWarning("Can only add coins in Play Mode");
        }
    }
#endif
    #endregion
}

/// <summary>
/// Example custom data structure for game settings
/// </summary>
[System.Serializable]
public class GameSettings
{
    public float musicVolume;
    public float soundVolume;
    public string difficulty;
    public string lastPlayedLevel;
    public bool tutorialCompleted;
    public System.DateTime lastPlayTime;

    public GameSettings()
    {
        musicVolume = 1f;
        soundVolume = 1f;
        difficulty = "Normal";
        lastPlayedLevel = "Level_01";
        tutorialCompleted = false;
        lastPlayTime = System.DateTime.UtcNow;
    }
} 