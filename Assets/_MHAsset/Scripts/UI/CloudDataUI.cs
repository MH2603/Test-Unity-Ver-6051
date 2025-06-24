using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CloudAuth;

public class CloudDataUI : MonoBehaviour
{
    #region Private Fields
    [Header("Services")]
    [SerializeField] private UnityCloudAuthService _authService;
    [SerializeField] private UnityCloudDataService _dataService;

    [Header("Data Display Panel")]
    [SerializeField] private GameObject _dataPanel;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _experienceText;
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private TMP_Text _achievementsText;
    [SerializeField] private TMP_Text _lastSaveTimeText;

    [Header("Data Modification")]
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private TMP_InputField _levelInput;
    [SerializeField] private TMP_InputField _experienceInput;
    [SerializeField] private TMP_InputField _coinsInput;
    [SerializeField] private TMP_InputField _achievementInput;

    [Header("Action Buttons")]
    [SerializeField] private Button _saveDataButton;
    [SerializeField] private Button _loadDataButton;
    [SerializeField] private Button _addCoinsButton;
    [SerializeField] private Button _addExperienceButton;
    [SerializeField] private Button _addAchievementButton;
    [SerializeField] private Button _updateNameButton;
    [SerializeField] private Button _deleteDataButton;
    [SerializeField] private Button _restoreBackupButton;

    [Header("Status & Settings")]
    [SerializeField] private TMP_Text _dataStatusText;
    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private Toggle _autoSaveToggle;
    [SerializeField] private Slider _autoSaveIntervalSlider;
    [SerializeField] private TMP_Text _autoSaveIntervalText;

    [Header("Auto Save Indicator")]
    [SerializeField] private GameObject _autoSaveIndicator;
    [SerializeField] private Image _saveStatusImage;
    [SerializeField] private Color _savingColor = Color.yellow;
    [SerializeField] private Color _savedColor = Color.green;
    [SerializeField] private Color _errorColor = Color.red;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        FindServices();
        SetupUI();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        SetupButtonEvents();
        UpdateUI();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        RemoveButtonEvents();
    }
    #endregion

    #region Initialization
    private void FindServices()
    {
        if (_authService == null)
            _authService = FindObjectOfType<UnityCloudAuthService>();

        if (_dataService == null)
            _dataService = FindObjectOfType<UnityCloudDataService>();

        if (_authService == null)
            LogError("UnityCloudAuthService not found!");

        if (_dataService == null)
            LogError("UnityCloudDataService not found!");
    }

    private void SetupUI()
    {
        // Initialize auto-save settings
        if (_autoSaveToggle != null)
        {
            _autoSaveToggle.isOn = true; // Default enabled
            _autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);
        }

        if (_autoSaveIntervalSlider != null)
        {
            _autoSaveIntervalSlider.minValue = 60f; // 1 minute minimum
            _autoSaveIntervalSlider.maxValue = 1800f; // 30 minutes maximum
            _autoSaveIntervalSlider.value = 300f; // 5 minutes default
            _autoSaveIntervalSlider.onValueChanged.AddListener(OnAutoSaveIntervalChanged);
        }

        UpdateAutoSaveIntervalText();
    }

    private void SubscribeToEvents()
    {
        if (_authService != null)
        {
            _authService.OnSignedIn += OnUserSignedIn;
            _authService.OnSignedOut += OnUserSignedOut;
        }

        if (_dataService != null)
        {
            _dataService.OnDataStatusChanged += OnDataStatusChanged;
            _dataService.OnDataLoaded += OnDataLoaded;
            _dataService.OnDataSaved += OnDataSaved;
            _dataService.OnDataError += OnDataError;
            _dataService.OnAutoSaveTriggered += OnAutoSaveTriggered;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_authService != null)
        {
            _authService.OnSignedIn -= OnUserSignedIn;
            _authService.OnSignedOut -= OnUserSignedOut;
        }

        if (_dataService != null)
        {
            _dataService.OnDataStatusChanged -= OnDataStatusChanged;
            _dataService.OnDataLoaded -= OnDataLoaded;
            _dataService.OnDataSaved -= OnDataSaved;
            _dataService.OnDataError -= OnDataError;
            _dataService.OnAutoSaveTriggered -= OnAutoSaveTriggered;
        }
    }

    private void SetupButtonEvents()
    {
        if (_saveDataButton != null) _saveDataButton.onClick.AddListener(OnSaveDataClicked);
        if (_loadDataButton != null) _loadDataButton.onClick.AddListener(OnLoadDataClicked);
        if (_addCoinsButton != null) _addCoinsButton.onClick.AddListener(OnAddCoinsClicked);
        if (_addExperienceButton != null) _addExperienceButton.onClick.AddListener(OnAddExperienceClicked);
        if (_addAchievementButton != null) _addAchievementButton.onClick.AddListener(OnAddAchievementClicked);
        if (_updateNameButton != null) _updateNameButton.onClick.AddListener(OnUpdateNameClicked);
        if (_deleteDataButton != null) _deleteDataButton.onClick.AddListener(OnDeleteDataClicked);
        if (_restoreBackupButton != null) _restoreBackupButton.onClick.AddListener(OnRestoreBackupClicked);
    }

    private void RemoveButtonEvents()
    {
        if (_saveDataButton != null) _saveDataButton.onClick.RemoveListener(OnSaveDataClicked);
        if (_loadDataButton != null) _loadDataButton.onClick.RemoveListener(OnLoadDataClicked);
        if (_addCoinsButton != null) _addCoinsButton.onClick.RemoveListener(OnAddCoinsClicked);
        if (_addExperienceButton != null) _addExperienceButton.onClick.RemoveListener(OnAddExperienceClicked);
        if (_addAchievementButton != null) _addAchievementButton.onClick.RemoveListener(OnAddAchievementClicked);
        if (_updateNameButton != null) _updateNameButton.onClick.RemoveListener(OnUpdateNameClicked);
        if (_deleteDataButton != null) _deleteDataButton.onClick.RemoveListener(OnDeleteDataClicked);
        if (_restoreBackupButton != null) _restoreBackupButton.onClick.RemoveListener(OnRestoreBackupClicked);
    }
    #endregion

    #region Event Handlers
    private void OnUserSignedIn(PlayerProfile profile)
    {
        ShowDataPanel();
        ClearError();
        UpdateUI();
    }

    private void OnUserSignedOut()
    {
        HideDataPanel();
        ClearError();
    }

    private void OnDataStatusChanged(DataSaveStatus status)
    {
        UpdateDataStatus(status);
        UpdateButtonStates(status);
        UpdateSaveStatusIndicator(status);
    }

    private void OnDataLoaded(PlayerData data)
    {
        UpdateDataDisplay(data);
        ClearError();
        ShowMessage("Data loaded successfully!");
    }

    private void OnDataSaved(PlayerData data)
    {
        UpdateDataDisplay(data);
        ClearError();
        ShowMessage("Data saved successfully!");
    }

    private void OnDataError(string error)
    {
        ShowError(error);
    }

    private void OnAutoSaveTriggered()
    {
        ShowMessage("Auto-save triggered...");
    }
    #endregion

    #region Button Click Handlers
    private async void OnSaveDataClicked()
    {
        if (_dataService == null) return;

        bool success = await _dataService.SavePlayerDataAsync();
        if (!success)
        {
            ShowError("Failed to save data");
        }
    }

    private async void OnLoadDataClicked()
    {
        if (_dataService == null) return;

        bool success = await _dataService.LoadPlayerDataAsync();
        if (!success)
        {
            ShowError("Failed to load data");
        }
    }

    private void OnAddCoinsClicked()
    {
        if (_dataService == null || _coinsInput == null) return;

        if (int.TryParse(_coinsInput.text, out int coins))
        {
            _dataService.AddCoins(coins);
            _coinsInput.text = "";
            UpdateDataDisplay(_dataService.CurrentPlayerData);
            ShowMessage($"Added {coins} coins!");
        }
        else
        {
            ShowError("Invalid coin amount");
        }
    }

    private void OnAddExperienceClicked()
    {
        if (_dataService == null || _experienceInput == null) return;

        if (float.TryParse(_experienceInput.text, out float experience))
        {
            var currentData = _dataService.CurrentPlayerData;
            _dataService.UpdatePlayerProgress(currentData.level, currentData.experience + experience);
            _experienceInput.text = "";
            UpdateDataDisplay(_dataService.CurrentPlayerData);
            ShowMessage($"Added {experience} experience!");
        }
        else
        {
            ShowError("Invalid experience amount");
        }
    }

    private void OnAddAchievementClicked()
    {
        if (_dataService == null || _achievementInput == null) return;

        string achievement = _achievementInput.text.Trim();
        if (!string.IsNullOrEmpty(achievement))
        {
            _dataService.UnlockAchievement(achievement);
            _achievementInput.text = "";
            UpdateDataDisplay(_dataService.CurrentPlayerData);
            ShowMessage($"Achievement unlocked: {achievement}");
        }
        else
        {
            ShowError("Achievement name cannot be empty");
        }
    }

    private void OnUpdateNameClicked()
    {
        if (_dataService == null || _playerNameInput == null) return;

        string newName = _playerNameInput.text.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            _dataService.CurrentPlayerData.playerName = newName;
            _playerNameInput.text = "";
            UpdateDataDisplay(_dataService.CurrentPlayerData);
            ShowMessage($"Player name updated to: {newName}");
        }
        else
        {
            ShowError("Player name cannot be empty");
        }
    }

    private async void OnDeleteDataClicked()
    {
        if (_dataService == null) return;

        // Show confirmation dialog (you can implement a more sophisticated confirmation system)
        if (Application.isEditor || 
            UnityEngine.Device.Application.platform == RuntimePlatform.WindowsPlayer ||
            UnityEngine.Device.Application.platform == RuntimePlatform.OSXPlayer ||
            UnityEngine.Device.Application.platform == RuntimePlatform.LinuxPlayer)
        {
            bool success = await _dataService.DeleteAllDataAsync();
            if (success)
            {
                ShowMessage("All data deleted successfully");
            }
            else
            {
                ShowError("Failed to delete data");
            }
        }
    }

    private async void OnRestoreBackupClicked()
    {
        if (_dataService == null) return;

        bool success = await _dataService.RestoreFromBackupAsync();
        if (success)
        {
            ShowMessage("Data restored from backup");
        }
        else
        {
            ShowError("Failed to restore from backup or no backup found");
        }
    }

    private void OnAutoSaveToggleChanged(bool enabled)
    {
        if (_dataService != null)
        {
            _dataService.SetAutoSaveEnabled(enabled);
            ShowMessage($"Auto-save {(enabled ? "enabled" : "disabled")}");
        }
    }

    private void OnAutoSaveIntervalChanged(float interval)
    {
        if (_dataService != null)
        {
            _dataService.SetAutoSaveInterval(interval);
            UpdateAutoSaveIntervalText();
        }
    }
    #endregion

    #region UI Updates
    private void UpdateUI()
    {
        bool isSignedIn = _authService != null && _authService.IsSignedIn;
        
        // hard coded for now, as we don't have a real data service state
        bool isDataReady = _dataService != null && _dataService.IsReady;
        isDataReady = true;
        
        if (isSignedIn && isDataReady)
        {
            ShowDataPanel();
            UpdateDataDisplay(_dataService.CurrentPlayerData);
        }
        else
        {
            HideDataPanel();
        }

        UpdateDataStatus(_dataService?.CurrentStatus ?? DataSaveStatus.NotInitialized);
    }

    private void ShowDataPanel()
    {
        if (_dataPanel != null)
            _dataPanel.SetActive(true);
    }

    private void HideDataPanel()
    {
        if (_dataPanel != null)
            _dataPanel.SetActive(false);
    }

    private void UpdateDataDisplay(PlayerData data)
    {
        if (data == null) return;

        if (_playerNameText != null)
            _playerNameText.text = $"Name: {data.playerName}";

        if (_levelText != null)
            _levelText.text = $"Level: {data.level}";

        if (_experienceText != null)
            _experienceText.text = $"Experience: {data.experience:F1}";

        if (_coinsText != null)
            _coinsText.text = $"Coins: {data.coins}";

        if (_achievementsText != null)
        {
            string achievementsStr = data.achievements.Count > 0 
                ? string.Join(", ", data.achievements)
                : "None";
            _achievementsText.text = $"Achievements: {achievementsStr}";
        }

        if (_lastSaveTimeText != null)
            _lastSaveTimeText.text = $"Last Save: {data.lastSaveTime:yyyy-MM-dd HH:mm:ss}";
    }

    private void UpdateDataStatus(DataSaveStatus status)
    {
        if (_dataStatusText != null)
            _dataStatusText.text = $"Data Status: {status}";
    }

    private void UpdateButtonStates(DataSaveStatus status)
    {
        bool isProcessing = status == DataSaveStatus.Saving || status == DataSaveStatus.Loading;
        bool isReady = status == DataSaveStatus.Ready;

        SetButtonInteractable(_saveDataButton, isReady);
        SetButtonInteractable(_loadDataButton, isReady);
        SetButtonInteractable(_addCoinsButton, isReady);
        SetButtonInteractable(_addExperienceButton, isReady);
        SetButtonInteractable(_addAchievementButton, isReady);
        SetButtonInteractable(_updateNameButton, isReady);
        SetButtonInteractable(_deleteDataButton, isReady);
        SetButtonInteractable(_restoreBackupButton, isReady);
    }

    private void UpdateSaveStatusIndicator(DataSaveStatus status)
    {
        if (_saveStatusImage == null) return;

        switch (status)
        {
            case DataSaveStatus.Saving:
                _saveStatusImage.color = _savingColor;
                break;
            case DataSaveStatus.Ready:
                _saveStatusImage.color = _savedColor;
                break;
            case DataSaveStatus.Error:
                _saveStatusImage.color = _errorColor;
                break;
            default:
                _saveStatusImage.color = Color.white;
                break;
        }
    }

    private void UpdateAutoSaveIntervalText()
    {
        if (_autoSaveIntervalText != null && _autoSaveIntervalSlider != null)
        {
            int minutes = Mathf.FloorToInt(_autoSaveIntervalSlider.value / 60f);
            int seconds = Mathf.FloorToInt(_autoSaveIntervalSlider.value % 60f);
            _autoSaveIntervalText.text = $"Auto-save: {minutes}m {seconds}s";
        }
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void ShowMessage(string message)
    {
        Debug.Log($"[CloudDataUI] {message}");
        
        // You can implement a proper message display system here
        if (_errorText != null)
        {
            _errorText.text = message;
            _errorText.color = Color.green;
        }
    }

    private void ShowError(string error)
    {
        Debug.LogError($"[CloudDataUI] {error}");
        
        if (_errorText != null)
        {
            _errorText.text = error;
            _errorText.color = Color.red;
        }
    }

    private void ClearError()
    {
        if (_errorText != null)
            _errorText.text = "";
    }

    private void LogError(string message)
    {
        Debug.LogError($"[CloudDataUI] {message}");
    }
    #endregion

#if UNITY_EDITOR
    [ContextMenu("Test Add Coins")]
    private void TestAddCoins()
    {
        if (_dataService != null)
        {
            _dataService.AddCoins(100);
            UpdateDataDisplay(_dataService.CurrentPlayerData);
        }
    }

    [ContextMenu("Test Add Experience")]
    private void TestAddExperience()
    {
        if (_dataService != null)
        {
            var currentData = _dataService.CurrentPlayerData;
            _dataService.UpdatePlayerProgress(currentData.level, currentData.experience + 50f);
            UpdateDataDisplay(_dataService.CurrentPlayerData);
        }
    }

    [ContextMenu("Test Add Achievement")]
    private void TestAddAchievement()
    {
        if (_dataService != null)
        {
            _dataService.UnlockAchievement("Test Achievement");
            UpdateDataDisplay(_dataService.CurrentPlayerData);
        }
    }
#endif
} 