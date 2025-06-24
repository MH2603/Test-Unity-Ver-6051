# Unity Cloud Data Save Setup Guide

## Overview
This guide will help you implement the Unity Cloud Data Save feature that integrates with your existing Unity Cloud Authentication system. The system allows you to save and load player data as JSON strings to Unity Cloud Save.

## Prerequisites

### 1. Unity Cloud Services Setup
- Unity Services must be properly configured in the Unity Dashboard
- Authentication service must be enabled
- **Cloud Save service must be enabled** in your Unity Cloud Project
- Project must be linked to a Unity Cloud Project ID

### 2. Required Unity Packages
Install these packages via Package Manager:
```
Unity.Services.Core
Unity.Services.Authentication
Unity.Services.CloudSave
Newtonsoft.Json (for JSON serialization)
```

### 3. Existing Components
Ensure you have the following existing components:
- `UnityCloudAuthService.cs` (already implemented)
- `SimpleCloudAuthUI.cs` or `CloudAuthUIController.cs` (already implemented)

## Implementation Steps

### Step 1: Verify Unity Cloud Save Service

1. Open Unity Dashboard (dashboard.unity3d.com)
2. Navigate to your project
3. Go to **Cloud Build** → **Services**
4. Ensure **Cloud Save** is enabled
5. Note any usage limits or quotas

### Step 2: Set Up the Cloud Data Service

1. **Add the Data Service to your scene:**
   - Create an empty GameObject named "CloudDataService"
   - Add the `UnityCloudDataService` component
   - Configure the settings in the inspector:
     - `Auto Save Enabled`: true (recommended)
     - `Auto Save Interval`: 300 seconds (5 minutes)
     - `Debug Mode`: true (for development)
     - `Enable Backups`: true (recommended)

### Step 3: Set Up the Data UI (Optional)

1. **Create a new Canvas or use existing UI:**
   - Add a new GameObject with `CloudDataUI` component
   - Connect the UI elements in the inspector (see UI Setup section below)

### Step 4: Integration with Existing Authentication

The `UnityCloudDataService` automatically integrates with your existing `UnityCloudAuthService`. It will:
- Auto-detect the authentication service
- Subscribe to sign-in/sign-out events
- Load player data when user signs in
- Clear local data when user signs out

## UI Setup (CloudDataUI Component)

### Required UI Elements
Create these UI elements and assign them to the `CloudDataUI` component:

#### Data Display Panel
```
DataPanel (GameObject)
├── PlayerNameText (TextMeshPro)
├── LevelText (TextMeshPro)
├── ExperienceText (TextMeshPro)
├── CoinsText (TextMeshPro)
├── AchievementsText (TextMeshPro)
└── LastSaveTimeText (TextMeshPro)
```

#### Data Modification Inputs
```
InputPanel (GameObject)
├── PlayerNameInput (TMP_InputField)
├── LevelInput (TMP_InputField)
├── ExperienceInput (TMP_InputField)
├── CoinsInput (TMP_InputField)
└── AchievementInput (TMP_InputField)
```

#### Action Buttons
```
ButtonPanel (GameObject)
├── SaveDataButton (Button)
├── LoadDataButton (Button)
├── AddCoinsButton (Button)
├── AddExperienceButton (Button)
├── AddAchievementButton (Button)
├── UpdateNameButton (Button)
├── DeleteDataButton (Button)
└── RestoreBackupButton (Button)
```

#### Status and Settings
```
StatusPanel (GameObject)
├── DataStatusText (TextMeshPro)
├── ErrorText (TextMeshPro)
├── AutoSaveToggle (Toggle)
├── AutoSaveIntervalSlider (Slider)
├── AutoSaveIntervalText (TextMeshPro)
└── SaveStatusImage (Image)
```

## Code Integration Examples

### Basic Usage in Your Game Scripts

```csharp
public class GameManager : MonoBehaviour
{
    private UnityCloudDataService dataService;
    
    void Start()
    {
        dataService = FindObjectOfType<UnityCloudDataService>();
    }
    
    // When player earns coins
    public async void OnPlayerEarnCoins(int amount)
    {
        if (dataService != null && dataService.IsReady)
        {
            dataService.AddCoins(amount);
            await dataService.SavePlayerDataAsync();
        }
    }
    
    // When player gains experience
    public async void OnPlayerGainExperience(float xp)
    {
        if (dataService != null && dataService.IsReady)
        {
            var data = dataService.CurrentPlayerData;
            dataService.UpdatePlayerProgress(data.level, data.experience + xp);
            await dataService.SavePlayerDataAsync();
        }
    }
    
    // When player unlocks achievement
    public async void OnAchievementUnlocked(string achievementId)
    {
        if (dataService != null && dataService.IsReady)
        {
            dataService.UnlockAchievement(achievementId);
            await dataService.SavePlayerDataAsync();
        }
    }
}
```

### Saving Custom Data

```csharp
// Create custom data structure
[System.Serializable]
public class GameSettings
{
    public float musicVolume;
    public float soundVolume;
    public string difficulty;
    public bool tutorialCompleted;
}

// Save custom data
public async void SaveGameSettings()
{
    var settings = new GameSettings
    {
        musicVolume = 0.8f,
        soundVolume = 0.9f,
        difficulty = "Hard",
        tutorialCompleted = true
    };
    
    await dataService.SaveCustomDataAsync("GameSettings", settings);
}

// Load custom data
public async void LoadGameSettings()
{
    var settings = await dataService.LoadCustomDataAsync<GameSettings>("GameSettings");
    if (settings != null)
    {
        // Apply settings to your game
        AudioListener.volume = settings.musicVolume;
        // etc...
    }
}
```

## Architecture Overview

```
Game Logic
    ↓
UnityCloudDataService ←→ UnityCloudAuthService
    ↓                           ↓
Unity Cloud Save          Unity Authentication
    ↓                           ↓
Unity Cloud Services ←→ Unity Dashboard
```

## Features and Capabilities

### Core Features
- **Automatic Integration**: Works seamlessly with existing authentication
- **JSON Serialization**: All data saved as JSON strings
- **Auto-save**: Configurable automatic saving
- **Backup System**: Optional backup creation and restoration
- **Custom Data**: Save any serializable data with custom keys
- **Event System**: Rich event system for UI updates
- **Error Handling**: Comprehensive error handling and user feedback

### Player Data Structure
The default `PlayerData` includes:
- Player name
- Level and experience
- Coins/currency
- Achievements list
- Last save timestamp

### Data Operations
- `SavePlayerDataAsync()`: Save main player data
- `LoadPlayerDataAsync()`: Load main player data
- `SaveCustomDataAsync(key, data)`: Save custom data with specific key
- `LoadCustomDataAsync<T>(key)`: Load custom data by key
- `DeleteAllDataAsync()`: Delete all player data
- `RestoreFromBackupAsync()`: Restore from backup

## Testing and Debugging

### 1. Enable Debug Mode
Set `Debug Mode` to true in the `UnityCloudDataService` component to see detailed logs.

### 2. Use Context Menu Testing
Right-click on the `UnityCloudDataService` in the Inspector to access:
- Force Save Data
- Force Load Data
- Clear All Data

### 3. Use the Demo Script
Add the `CloudDataExample` component to test functionality:
- Enable "Run Demo On Start" for automatic testing
- Use context menu options for manual testing

### 4. Monitor Cloud Save Dashboard
Check the Unity Dashboard → Cloud Save section to monitor:
- Data usage
- API call counts
- Error logs

## Troubleshooting

### Common Issues

1. **"Service not ready" errors**
   - Ensure user is signed in to authentication
   - Check Unity Services initialization
   - Verify Cloud Save is enabled in dashboard

2. **JSON serialization errors**
   - Ensure all custom data classes are marked `[Serializable]`
   - Check for circular references in data structures
   - Verify Newtonsoft.Json package is installed

3. **Rate limiting errors**
   - Cloud Save has rate limits (check Unity documentation)
   - Implement exponential backoff for retries
   - Consider reducing auto-save frequency

4. **Data not persisting**
   - Check internet connectivity
   - Verify Cloud Save quotas in dashboard
   - Check for authentication session expiration

### Data Limits
- Maximum data size per key: 5MB (check current Unity limits)
- Maximum number of keys per player: 1000 (check current Unity limits)
- API rate limits apply (check Unity documentation)

## Security Considerations

- Data is automatically tied to authenticated user
- No additional encryption needed (handled by Unity Cloud)
- Validate data on the client before saving
- Consider sensitive data handling for your specific use case

## Performance Tips

1. **Batch Operations**: Group multiple data changes before saving
2. **Selective Loading**: Only load data you need using specific keys
3. **Auto-save Tuning**: Adjust auto-save interval based on your game's needs
4. **Error Recovery**: Implement retry logic for failed operations

## Next Steps

1. Test the system with your existing authentication setup
2. Customize the `PlayerData` structure for your game's needs
3. Implement game-specific data operations
4. Create custom UI that fits your game's design
5. Test with different network conditions
6. Monitor usage in the Unity Dashboard

## Support and Resources

- Unity Cloud Save Documentation: https://docs.unity.com/cloud-save/
- Unity Authentication Documentation: https://docs.unity.com/authentication/
- Unity Services Dashboard: https://dashboard.unity3d.com/

---

This system provides a robust foundation for cloud data persistence that grows with your game's needs while maintaining Unity best practices and performance standards. 