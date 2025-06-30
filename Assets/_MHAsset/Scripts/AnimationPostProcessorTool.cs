using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationPostProcessorTool : EditorWindow
{
    #region Menu Item
    [MenuItem("Tool/Config_Animation_Auto")]
    public static void ConfigAnimationAuto()
    {
        ProcessAnimationsInTargetFolder();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Manually processes all FBX files in the target folder using the Animation Post-Processor settings
    /// </summary>
    public static void ProcessAnimationsInTargetFolder()
    {
        // Load settings
        AnimationPostProcessorSettings settings = LoadSettings();
        if (settings == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "AnimationPostProcessorSettings asset not found!\n\nPlease create the settings asset first:\nCreate > AnimationPostProcessor > Settings", 
                "OK");
            return;
        }

        if (!settings.enabled)
        {
            EditorUtility.DisplayDialog("Warning", 
                "Animation Post-Processor is currently disabled in settings.\n\nPlease enable it in the AnimationPostProcessorSettings asset.", 
                "OK");
            return;
        }

        if (string.IsNullOrEmpty(settings.targetFolder))
        {
            EditorUtility.DisplayDialog("Error", 
                "Target folder is not set in AnimationPostProcessorSettings!\n\nPlease specify a target folder path.", 
                "OK");
            return;
        }

        if (settings.referenceFBX == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Reference FBX is not assigned in AnimationPostProcessorSettings!\n\nPlease assign a properly configured FBX file as the reference.", 
                "OK");
            return;
        }

        if (!Directory.Exists(settings.targetFolder))
        {
            EditorUtility.DisplayDialog("Error", 
                $"Target folder does not exist:\n{settings.targetFolder}\n\nPlease check the path in AnimationPostProcessorSettings.", 
                "OK");
            return;
        }

        // Find all FBX files in target folder
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { settings.targetFolder });
        
        if (fbxGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", 
                $"No FBX files found in target folder:\n{settings.targetFolder}", 
                "OK");
            return;
        }

        // Process each FBX file
        int processedCount = 0;
        int totalCount = fbxGuids.Length;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < fbxGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fbxGuids[i]);
                
                // Show progress bar
                float progress = (float)i / totalCount;
                bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                    "Processing Animations", 
                    $"Processing: {Path.GetFileName(assetPath)} ({i + 1}/{totalCount})", 
                    progress);

                if (cancelled)
                {
                    break;
                }

                // Only process FBX files
                if (assetPath.ToLower().EndsWith(".fbx"))
                {
                    Debug.Log($"[AnimationPostProcessor] Manual processing: {assetPath}");
                    
                    // Force reimport to trigger the post-processor
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    processedCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        // Show completion dialog
        string message = $"Animation processing completed!\n\n" +
                        $"Processed: {processedCount} FBX files\n" +
                        $"Total found: {totalCount} files\n" +
                        $"Target folder: {settings.targetFolder}";
        
        EditorUtility.DisplayDialog("Complete", message, "OK");
        
        Debug.Log($"[AnimationPostProcessor] Manual processing completed. Processed {processedCount}/{totalCount} FBX files.");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads the AnimationPostProcessorSettings asset
    /// </summary>
    /// <returns>Settings asset or null if not found</returns>
    private static AnimationPostProcessorSettings LoadSettings()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationPostProcessorSettings");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AnimationPostProcessorSettings>(path);
        }
        return null;
    }
    #endregion

    #region Editor Window (Optional)
    /// <summary>
    /// Opens a dedicated window for animation processing (optional feature)
    /// </summary>
    [MenuItem("Tool/Config_Animation_Auto_Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimationPostProcessorTool>("Animation Processor");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Animation Post-Processor Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool manually triggers the Animation Post-Processor for all FBX files in the target folder.\n\n" +
            "Use this to:\n" +
            "• Process existing FBX files\n" +
            "• Re-process files after changing settings\n" +
            "• Batch process multiple animations", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        // Load and display current settings
        AnimationPostProcessorSettings settings = LoadSettings();
        if (settings != null)
        {
            EditorGUILayout.LabelField("Current Settings:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Enabled: {settings.enabled}");
            EditorGUILayout.LabelField($"Target Folder: {settings.targetFolder}");
            EditorGUILayout.LabelField($"Reference FBX: {(settings.referenceFBX != null ? settings.referenceFBX.name : "None")}");
            EditorGUILayout.LabelField($"Animation Type: {settings.animationType}");
            
            GUILayout.Space(10);
            
            // Count FBX files in target folder
            if (!string.IsNullOrEmpty(settings.targetFolder) && Directory.Exists(settings.targetFolder))
            {
                string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { settings.targetFolder });
                EditorGUILayout.LabelField($"FBX Files Found: {fbxGuids.Length}");
            }
            else
            {
                EditorGUILayout.LabelField("FBX Files Found: 0 (invalid folder path)");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("AnimationPostProcessorSettings asset not found!", MessageType.Warning);
        }
        
        GUILayout.Space(20);
        
        // Process button
        GUI.enabled = settings != null && settings.enabled && !string.IsNullOrEmpty(settings.targetFolder) && settings.referenceFBX != null;
        if (GUILayout.Button("Process All Animations in Target Folder", GUILayout.Height(30)))
        {
            ProcessAnimationsInTargetFolder();
        }
        GUI.enabled = true;
        
        // Show warning if Reference FBX is missing
        if (settings != null && settings.referenceFBX == null)
        {
            EditorGUILayout.HelpBox("Reference FBX is not assigned! Please assign a Reference FBX in the settings to enable processing.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        // Settings button
        if (settings != null)
        {
            if (GUILayout.Button("Select Settings Asset"))
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
        }
        else
        {
            if (GUILayout.Button("Create Settings Asset"))
            {
                // Create settings asset
                var newSettings = CreateInstance<AnimationPostProcessorSettings>();
                string path = "Assets/AnimationPostProcessorSettings.asset";
                AssetDatabase.CreateAsset(newSettings, AssetDatabase.GenerateUniqueAssetPath(path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Selection.activeObject = newSettings;
                EditorGUIUtility.PingObject(newSettings);
            }
        }
    }
    #endregion
} 