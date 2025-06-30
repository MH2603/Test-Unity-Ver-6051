# Animation Post-Processor Documentation

## Overview

The Animation Post-Processor is a Unity Editor tool that automatically processes and configures FBX animation files during import. It standardizes animation import settings by copying configurations from a reference FBX file, eliminating the need for manual setup of each animation file.

## Features

- **Automated Import Processing**: Automatically processes FBX files in specified directories
- **Reference-Based Configuration**: Copies settings from a reference FBX file to ensure consistency
- **Avatar Management**: Automatically assigns and configures humanoid avatars
- **Animation Clip Configuration**: Sets up loop time, naming conventions, and clip properties
- **Material & Texture Extraction**: Optionally extracts materials and textures during import
- **Batch Processing**: Handles multiple animation files with consistent settings

## Files Structure

The system consists of three main components:

1. **`AnimationPostprocessor.cs`** - The core post-processor that extends Unity's AssetPostprocessor
2. **`AnimationPostProcessorSettings.cs`** - ScriptableObject for configuration settings
3. **`AnimationPostProcessorSettings.asset`** - The settings asset file

## Setup Instructions

### Step 1: Create Settings Asset

1. In the Project window, right-click in the folder where you want to store the settings
2. Navigate to **Create > AnimationPostProcessor > Settings**
3. Name the asset (e.g., "AnimationPostProcessorSettings")
4. The system automatically finds this asset, so you only need one per project

### Step 2: Configure Reference FBX

1. Select your `AnimationPostProcessorSettings` asset
2. In the Inspector, assign the following required fields:
   - **Reference FBX**: A properly configured FBX file that serves as the template
   - **Reference Avatar**: The avatar from the reference FBX (usually auto-assigned)

### Step 3: Configure Target Folder

1. Set the **Target Folder** path where your animation FBX files will be placed
2. Default: `"Assets/_Project/Animations"`
3. Only FBX files in this folder (and subfolders) will be processed

### Step 4: Configure Processing Options

Adjust the following settings based on your project needs:

## Configuration Options

### Core Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| **Enabled** | bool | true | Master switch to enable/disable the post-processor |
| **Target Folder** | string | "Assets/_Project/Animations" | Folder path where FBX files will be automatically processed |
| **Reference FBX** | GameObject | null | Template FBX file with desired import settings |
| **Reference Avatar** | Avatar | null | Avatar to be used for humanoid animations |

### Import Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| **Animation Type** | ModelImporterAnimationType | Human | Type of animation rig (Human, Generic, Legacy) |
| **Enable Translation DoF** | bool | true | Enables translation degrees of freedom for humanoid rigs |
| **Loop Time** | bool | true | Sets animation clips to loop by default |
| **Rename Clips** | bool | true | Automatically renames clips based on FBX filename |
| **Force Editor Apply** | bool | true | Forces Unity to apply import settings (uses reflection) |
| **Extract Textures** | bool | true | Automatically extracts textures and materials |

## Usage Workflow

### Basic Usage

1. **Set up your reference FBX**:
   - Import and configure one FBX file manually with all desired settings
   - Set up the rig, animation clips, materials, etc. as needed
   - Assign this FBX as the Reference FBX in settings

2. **Configure the settings asset**:
   - Set the target folder path
   - Enable desired processing options
   - Save the settings

3. **Import animations**:
   - Place new FBX animation files in the target folder
   - Unity will automatically apply the reference settings during import
   - No manual configuration required for subsequent imports

### Advanced Workflow

#### Custom Naming Convention
When `renameClips` is enabled:
- Single animation clip: Named after the FBX filename
- Multiple clips: Named as "filename + original_clip_name"

#### Material and Texture Handling
When `extractTextures` is enabled:
- Textures are extracted to the same directory as the FBX
- Material location is set to External
- Materials are automatically linked to extracted textures

#### Avatar Configuration
The system automatically:
- Copies the human description from the reference avatar
- Sets up translation degrees of freedom if enabled
- Falls back to Generic animation type if avatar is invalid

## Processing Details

### What Gets Copied from Reference FBX

#### Model Tab Settings
- Global scale and file scale usage
- Mesh compression and readability
- Optimization settings (polygons, vertices)
- Blend shapes, quads, and index format
- Normal and tangent import settings
- UV generation and swapping

#### Rig Tab Settings
- Animation type (Human, Generic, Legacy)
- Avatar assignment and configuration
- GameObject optimization settings

#### Animation Tab Settings
- Loop time configuration
- Root motion settings (orientation, position)
- Additive reference pose settings
- Mirror and wrap mode settings
- Animation masking configuration

#### Materials Tab Settings
- Material import mode and location
- Material naming conventions

## Best Practices

### Reference FBX Setup
1. **Use a clean, well-configured reference**: Ensure your reference FBX has optimal settings
2. **Test with different animation types**: Verify settings work for various animation styles
3. **Configure avatar properly**: Ensure the humanoid avatar is correctly mapped

### Folder Organization
```
Assets/
├── _Project/
│   ├── Animations/
│   │   ├── Characters/
│   │   │   ├── Walk.fbx
│   │   │   ├── Run.fbx
│   │   │   └── Jump.fbx
│   │   └── Reference/
│   │       └── CharacterReference.fbx
│   └── Settings/
│       └── AnimationPostProcessorSettings.asset
```

### Performance Considerations
1. **Limit target folder scope**: Keep the target folder specific to avoid processing unnecessary files
2. **Disable when not needed**: Turn off the processor when importing non-animation assets
3. **Monitor import times**: Large batches may take time due to the comprehensive copying process

## Troubleshooting

### Common Issues

#### NullReferenceException Error
If you encounter: `"NullReferenceException: Object reference not set to an instance of an object AnimationPostprocessor.OnPreprocessModel()"`

**Cause**: The Reference FBX is not properly assigned in the settings.

**Solution**: 
1. Open your `AnimationPostProcessorSettings` asset
2. Assign a valid FBX file to the `Reference FBX` field
3. Ensure the FBX file exists and is properly imported
4. The system will now show clear error messages in the Console if references are missing

#### Post-processor not running
- **Check settings asset**: Ensure `AnimationPostProcessorSettings.asset` exists and is properly configured
- **Verify target folder**: Confirm FBX files are in the specified target folder path
- **Enable setting**: Make sure `enabled` is set to true in settings
- **Reference FBX missing**: Ensure `Reference FBX` field is assigned with a valid FBX file

#### Avatar issues
- **Reference avatar invalid**: Check that the reference avatar is properly configured
- **Falls back to Generic**: System automatically uses Generic type if humanoid avatar fails
- **Missing avatar assignment**: Ensure Reference Avatar field is populated

#### Animation clips not configured properly
- **Reference clips missing**: Ensure the reference FBX has at least one animation clip configured
- **Naming issues**: Check `renameClips` setting if clip names are not as expected
- **Loop settings**: Verify `loopTime` setting matches your requirements

#### Material/Texture extraction fails
- **Write permissions**: Ensure Unity has write access to the target directory
- **External materials**: Check if `extractTextures` is enabled when materials are expected

### Debug Information

The system includes built-in debugging through Unity's console. Check for:
- Import progress messages
- Error messages related to avatar or clip configuration
- Warnings about missing reference assets

## Code Extension

### Adding Custom Settings

To add new configuration options:

1. **Extend the settings class**:
```csharp
// In AnimationPostProcessorSettings.cs
public bool customSetting = false;
public float customValue = 1.0f;
```

2. **Use in the post-processor**:
```csharp
// In AnimationPostprocessor.cs OnPreprocessModel() or OnPreprocessAnimation()
if (settings.customSetting) {
    importer.someProperty = settings.customValue;
}
```

### Custom Processing Logic

You can extend the `CopyModelImporterSettings` method to include additional import settings:

```csharp
// Add custom processing
importer.customProperty = referenceImporter.customProperty;
```

## Version Compatibility

- **Unity Version**: Compatible with Unity 2019.3 and later
- **Dependencies**: Uses Unity's AssetPostprocessor system and SerializedObject API
- **Platform**: Editor-only functionality (uses #if UNITY_EDITOR where needed)

## Support

For issues or enhancements:
1. Check the Unity Console for error messages
2. Verify all reference assets are properly assigned
3. Ensure target folder paths are correct
4. Test with a simple reference FBX first

---

*This documentation covers the Animation Post-Processor system for automated FBX import configuration in Unity.* 