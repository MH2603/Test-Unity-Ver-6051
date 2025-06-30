# Unity Cloud System Setup Guide

## Overview
This comprehensive guide will help you implement the complete Unity Cloud System, including authentication and data persistence services. The system provides user authentication, cloud data storage, and seamless integration between services.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Unity Dashboard Configuration](#unity-dashboard-configuration)
3. [Package Installation](#package-installation)
4. [Service Setup](#service-setup)
5. [UI Implementation](#ui-implementation)
6. [Code Integration](#code-integration)
7. [Testing and Validation](#testing-and-validation)
8. [Troubleshooting](#troubleshooting)
9. [Advanced Configuration](#advanced-configuration)

## Prerequisites

### System Requirements
- **Unity Version**: 2022.3 LTS or newer
- **Target Platforms**: All platforms supported by Unity Cloud Services
- **Internet Connection**: Required for cloud operations
- **Unity Account**: Active Unity Developer account

### Unity Cloud Services Requirements
- Unity Cloud Project with services enabled
- Valid Unity Project ID linked to your project
- Authentication and Cloud Save services activated

## Unity Dashboard Configuration

### Step 1: Create Unity Cloud Project

1. **Access Unity Dashboard**:
   - Go to [dashboard.unity3d.com](https://dashboard.unity3d.com)
   - Sign in with your Unity account

2. **Create or Select Project**:
   - Click "New Project" or select existing project
   - Choose your organization
   - Set project name and description

3. **Link Unity Project**:
   - Open your Unity project
   - Go to **Window** → **General** → **Services**
   - Click "Select Organization" and choose your organization
   - Select your cloud project or create new one

### Step 2: Enable Required Services

1. **Enable Authentication Service**:
   - In Unity Dashboard, go to **Operate** → **Authentication**
   - Click "Set up Authentication"
   - Configure authentication methods:
     - ✅ **Anonymous**: Enable for guest users
     - ✅ **Username and Password**: Enable for registered users
     - ✅ **Unity Player Accounts**: Enable for Unity ecosystem integration

2. **Enable Cloud Save Service**:
   - Go to **Operate** → **Cloud Save**
   - Click "Set up Cloud Save"
   - Review usage limits and quotas
   - Configure data retention policies

3. **Configure Project Settings**:
   - Go to **Project Settings** → **Unity Cloud**
   - Note your **Project ID** (you'll need this)
   - Configure environment settings (Development/Production)

## Package Installation

### Step 1: Install Core Packages

Open **Window** → **Package Manager** and install:

```
Unity.Services.Core (2.0.0 or newer)
Unity.Services.Authentication (3.0.0 or newer)
Unity.Services.CloudSave (3.0.0 or newer)
```

### Step 2: Install Supporting Packages

```
Newtonsoft.Json (3.2.1 or newer) - For JSON serialization
TextMeshPro (3.0.6 or newer) - For UI text rendering
Unity UI (1.0.0 or newer) - For user interface components
```

### Step 3: Verify Installation

1. Check that packages appear in **Package Manager** → **In Project**
2. Verify no compilation errors in Console
3. Confirm Unity Services window shows connected status

## Service Setup

### Step 1: Authentication Service Setup

1. **Create Authentication Service GameObject**:
   ```
   GameObject Name: "CloudAuthService"
   Component: UnityCloudAuthService
   ```

2. **Configure Authentication Settings**:
   - ✅ **Auto Initialize**: true (recommended)
   - ✅ **Debug Mode**: true (for development)

3. **Verify Configuration**:
   ```csharp
   // The service will automatically initialize Unity Services
   // and be ready for authentication operations
   ```

### Step 2: Data Service Setup

1. **Create Data Service GameObject**:
   ```
   GameObject Name: "CloudDataService"
   Component: UnityCloudDataService
   ```

2. **Configure Data Service Settings**:
   - ✅ **Auto Save Enabled**: true
   - **Auto Save Interval**: 300 seconds (5 minutes)
   - ✅ **Debug Mode**: true (for development)
   - ✅ **Enable Backups**: true
   - ✅ **Auto Initialize Services**: true

### Step 3: Service Integration

The services can work in two integration patterns:

#### Pattern A: Integrated Services (Recommended)
```csharp
// Both services work together with event-driven communication
// UnityCloudDataService automatically detects UnityCloudAuthService
// Data loading happens automatically when user signs in
```

#### Pattern B: Independent Data Service
```csharp
// UnityCloudDataService works directly with Unity Authentication
// No dependency on UnityCloudAuthService wrapper
// Manual authentication status checking required
```

## UI Implementation

### Step 1: Authentication UI Setup

Create authentication interface with these components:

#### Login Panel Structure
```
AuthenticationCanvas
├── LoginPanel
│   ├── UsernameInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── LoginButton (Button)
│   ├── RegisterButton (Button)
│   ├── AnonymousButton (Button)
│   ├── UnityAccountButton (Button)
│   └── StatusText (TextMeshProUGUI)
└── UserPanel
    ├── WelcomeText (TextMeshProUGUI)
    ├── PlayerNameText (TextMeshProUGUI)
    ├── PlayerIDText (TextMeshProUGUI)
    ├── LogoutButton (Button)
    └── ChangePasswordButton (Button)
```

#### Authentication UI Component Setup
1. **Add SimpleCloudAuthUI Component**:
   - Attach to Canvas or UI Controller GameObject
   - Assign all UI elements in inspector
   - Configure panel references

2. **Configure UI Events**:
   ```csharp
   // UI automatically subscribes to authentication events
   // and updates based on authentication status
   ```

### Step 2: Data Management UI Setup

Create data management interface:

#### Data Panel Structure
```
DataCanvas
├── DataDisplayPanel
│   ├── PlayerNameDisplay (TextMeshProUGUI)
│   ├── LevelDisplay (TextMeshProUGUI)
│   ├── ExperienceDisplay (TextMeshProUGUI)
│   ├── CoinsDisplay (TextMeshProUGUI)
│   ├── AchievementsDisplay (TextMeshProUGUI)
│   └── LastSaveDisplay (TextMeshProUGUI)
├── DataInputPanel
│   ├── PlayerNameInput (TMP_InputField)
│   ├── LevelInput (TMP_InputField)
│   ├── ExperienceInput (TMP_InputField)
│   ├── CoinsInput (TMP_InputField)
│   └── AchievementInput (TMP_InputField)
├── ActionButtonPanel
│   ├── SaveDataButton (Button)
│   ├── LoadDataButton (Button)
│   ├── AddCoinsButton (Button)
│   ├── AddExperienceButton (Button)
│   ├── UnlockAchievementButton (Button)
│   ├── DeleteDataButton (Button)
│   └── RestoreBackupButton (Button)
└── StatusPanel
    ├── DataStatusText (TextMeshProUGUI)
    ├── ErrorDisplay (TextMeshProUGUI)
    ├── AutoSaveToggle (Toggle)
    ├── SaveIntervalSlider (Slider)
    └── SaveIndicator (Image)
```

#### Data UI Component Setup
1. **Add CloudDataUI Component**:
   - Attach to Canvas or UI Controller GameObject
   - Assign all UI elements in inspector
   - Configure status indicators

## Code Integration

### Step 1: Basic Service Integration

```csharp
using CloudAuth;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Cloud Services")]
    [SerializeField] private UnityCloudAuthService authService;
    [SerializeField] private UnityCloudDataService dataService;
    
    private void Start()
    {
        // Find services if not assigned
        if (authService == null)
            authService = FindObjectOfType<UnityCloudAuthService>();
        
        if (dataService == null)
            dataService = FindObjectOfType<UnityCloudDataService>();
        
        // Subscribe to events
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        // Authentication events
        if (authService != null)
        {
            authService.OnSignedIn += OnUserSignedIn;
            authService.OnSignedOut += OnUserSignedOut;
            authService.OnError += OnAuthError;
        }
        
        // Data service events
        if (dataService != null)
        {
            dataService.OnDataLoaded += OnDataLoaded;
            dataService.OnDataSaved += OnDataSaved;
            dataService.OnDataError += OnDataError;
            dataService.OnAutoSaveTriggered += OnAutoSave;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (authService != null)
        {
            authService.OnSignedIn -= OnUserSignedIn;
            authService.OnSignedOut -= OnUserSignedOut;
            authService.OnError -= OnAuthError;
        }
        
        if (dataService != null)
        {
            dataService.OnDataLoaded -= OnDataLoaded;
            dataService.OnDataSaved -= OnDataSaved;
            dataService.OnDataError -= OnDataError;
            dataService.OnAutoSaveTriggered -= OnAutoSave;
        }
    }
    
    #region Event Handlers
    private void OnUserSignedIn(PlayerProfile profile)
    {
        Debug.Log($"User signed in: {profile.Name}");
        // Data service automatically loads player data
    }
    
    private void OnUserSignedOut()
    {
        Debug.Log("User signed out");
        // Data service automatically clears local data
    }
    
    private void OnAuthError(string error)
    {
        Debug.LogError($"Authentication error: {error}");
        // Handle authentication errors
    }
    
    private void OnDataLoaded(PlayerData data)
    {
        Debug.Log($"Data loaded: Level {data.level}, Coins {data.coins}");
        // Update UI with loaded data
    }
    
    private void OnDataSaved(PlayerData data)
    {
        Debug.Log($"Data saved: Level {data.level}, Coins {data.coins}");
        // Show save confirmation
    }
    
    private void OnDataError(string error)
    {
        Debug.LogError($"Data error: {error}");
        // Handle data errors
    }
    
    private void OnAutoSave()
    {
        Debug.Log("Auto-save triggered");
        // Show auto-save indicator
    }
    #endregion
}
```

### Step 2: Game Progress Integration

```csharp
public class PlayerProgressManager : MonoBehaviour
{
    private UnityCloudDataService dataService;
    
    private void Start()
    {
        dataService = FindObjectOfType<UnityCloudDataService>();
    }
    
    // Called when player earns coins
    public async void OnCoinsEarned(int amount)
    {
        if (dataService?.IsReady == true)
        {
            dataService.AddCoins(amount);
            Debug.Log($"Added {amount} coins");
            
            // Auto-save will handle saving, or save manually:
            // await dataService.SavePlayerDataAsync();
        }
    }
    
    // Called when player gains experience
    public async void OnExperienceGained(float xp)
    {
        if (dataService?.IsReady == true)
        {
            var currentData = dataService.CurrentPlayerData;
            float newXp = currentData.experience + xp;
            int newLevel = CalculateLevel(newXp);
            
            dataService.UpdatePlayerProgress(newLevel, newXp);
            Debug.Log($"Gained {xp} XP, new level: {newLevel}");
            
            // Save immediately for important progress
            await dataService.SavePlayerDataAsync();
        }
    }
    
    // Called when achievement is unlocked
    public async void OnAchievementUnlocked(string achievementId)
    {
        if (dataService?.IsReady == true)
        {
            if (!dataService.HasAchievement(achievementId))
            {
                dataService.UnlockAchievement(achievementId);
                Debug.Log($"Achievement unlocked: {achievementId}");
                
                // Save achievement immediately
                await dataService.SavePlayerDataAsync();
            }
        }
    }
    
    private int CalculateLevel(float experience)
    {
        // Simple leveling formula
        return Mathf.FloorToInt(experience / 1000f) + 1;
    }
}
```

### Step 3: Custom Data Integration

```csharp
[System.Serializable]
public class GameSettings
{
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public string difficulty = "Normal";
    public bool tutorialCompleted = false;
    public List<string> unlockedLevels = new List<string>();
}

public class SettingsManager : MonoBehaviour
{
    private UnityCloudDataService dataService;
    private GameSettings currentSettings;
    
    private void Start()
    {
        dataService = FindObjectOfType<UnityCloudDataService>();
        LoadSettings();
    }
    
    public async void LoadSettings()
    {
        if (dataService?.IsReady == true)
        {
            currentSettings = await dataService.LoadCustomDataAsync<GameSettings>("GameSettings");
            
            if (currentSettings == null)
            {
                currentSettings = new GameSettings();
                Debug.Log("Created default settings");
            }
            
            ApplySettings();
        }
    }
    
    public async void SaveSettings()
    {
        if (dataService?.IsReady == true && currentSettings != null)
        {
            await dataService.SaveCustomDataAsync("GameSettings", currentSettings);
            Debug.Log("Settings saved");
        }
    }
    
    private void ApplySettings()
    {
        // Apply settings to game systems
        AudioListener.volume = currentSettings.musicVolume;
        // Apply other settings...
    }
    
    // Public methods to modify settings
    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = volume;
        AudioListener.volume = volume;
    }
    
    public void SetDifficulty(string difficulty)
    {
        currentSettings.difficulty = difficulty;
    }
    
    public void MarkTutorialComplete()
    {
        currentSettings.tutorialCompleted = true;
        SaveSettings(); // Save immediately
    }
}
```

## Testing and Validation

### Step 1: Basic Functionality Testing

1. **Test Authentication**:
   - Username/Password registration and login
   - Anonymous login
   - Unity Player Account login (if available)
   - Logout functionality

2. **Test Data Operations**:
   - Save player data
   - Load player data
   - Auto-save functionality
   - Backup creation and restoration
   - Custom data save/load

### Step 2: Integration Testing

1. **Authentication + Data Flow**:
   - Sign in → Automatic data load
   - Sign out → Local data clearing
   - Data operations only when authenticated

2. **Error Handling**:
   - Network disconnection scenarios
   - Invalid authentication attempts
   - Rate limiting behavior
   - Service unavailability

### Step 3: UI Testing

1. **Authentication UI**:
   - Form validation
   - Status updates
   - Error messages
   - Panel switching

2. **Data UI**:
   - Data display updates
   - Input validation
   - Button state management
   - Status indicators

## Troubleshooting

### Common Setup Issues

#### Issue: "Unity Services not initialized"
**Solution**:
```csharp
// Ensure Unity Services are initialized before use
if (UnityServices.State != ServicesInitializationState.Initialized)
{
    await UnityServices.InitializeAsync();
}
```

#### Issue: "Authentication required"
**Solution**:
```csharp
// Check authentication status before data operations
if (!AuthenticationService.Instance.IsSignedIn)
{
    Debug.LogError("User must be signed in for data operations");
    return;
}
```

#### Issue: "Cloud Save service not enabled"
**Solution**:
1. Check Unity Dashboard → Cloud Save service
2. Ensure service is enabled for your project
3. Verify project ID configuration

### Authentication Issues

#### Issue: "Invalid credentials" 
**Solution**:
- Verify username/password format requirements
- Check network connectivity
- Ensure authentication service is properly configured

#### Issue: "Anonymous sign-in failed"
**Solution**:
- Verify anonymous authentication is enabled in Unity Dashboard
- Check Unity Services initialization status

### Data Service Issues

#### Issue: "Rate limited" errors
**Solution**:
- Implement exponential backoff retry logic
- Reduce save frequency
- Review Unity Cloud Save usage limits

#### Issue: "Data not loading"
**Solution**:
```csharp
// Verify authentication and service status
Debug.Log($"Auth Status: {AuthenticationService.Instance.IsSignedIn}");
Debug.Log($"Data Service Status: {dataService.CurrentStatus}");
Debug.Log($"Unity Services: {UnityServices.State}");
```

## Advanced Configuration

### Environment-Specific Configuration

```csharp
public class CloudServiceConfig : MonoBehaviour
{
    [Header("Environment Settings")]
    public bool isDevelopment = true;
    public float developmentAutoSaveInterval = 60f; // 1 minute for testing
    public float productionAutoSaveInterval = 300f; // 5 minutes for production
    
    private void Start()
    {
        ConfigureServices();
    }
    
    private void ConfigureServices()
    {
        var dataService = FindObjectOfType<UnityCloudDataService>();
        if (dataService != null)
        {
            float interval = isDevelopment ? developmentAutoSaveInterval : productionAutoSaveInterval;
            dataService.SetAutoSaveInterval(interval);
        }
    }
}
```

## Security Best Practices

### Data Validation
```csharp
public bool ValidatePlayerData(PlayerData data)
{
    // Validate data before saving
    if (string.IsNullOrEmpty(data.playerName) || data.playerName.Length > 50)
        return false;
    
    if (data.level < 1 || data.level > 1000)
        return false;
    
    if (data.experience < 0)
        return false;
    
    if (data.coins < 0)
        return false;
    
    return true;
}
```

## Deployment Checklist

### Pre-Deployment
- [ ] Disable debug mode in production
- [ ] Configure production auto-save intervals
- [ ] Test all authentication methods
- [ ] Verify data persistence across sessions
- [ ] Test error handling and recovery
- [ ] Validate UI responsiveness
- [ ] Check memory usage and cleanup

### Production Configuration
- [ ] Set appropriate auto-save intervals
- [ ] Configure backup retention policies
- [ ] Set up monitoring and analytics
- [ ] Implement proper error reporting
- [ ] Configure rate limiting awareness
- [ ] Set up user feedback systems

This comprehensive setup guide should help you successfully implement the Unity Cloud System in your project! 🚀 