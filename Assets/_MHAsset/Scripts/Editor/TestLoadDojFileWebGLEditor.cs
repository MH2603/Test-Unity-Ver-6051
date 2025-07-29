using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace MH.Editor
{
    [CustomEditor(typeof(TestLoadDojFileWebGL))]
    public class TestLoadDojFileWebGLEditor : UnityEditor.Editor
    {
        #region Private Fields
        private string[] _availableDojFiles;
        private string[] _availableDojFileNames;
        private bool _showAdvancedOptions = false;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            RefreshAvailableFiles();
        }
        #endregion

        #region Editor GUI
        public override void OnInspectorGUI()
        {
            TestLoadDojFileWebGL testLoader = (TestLoadDojFileWebGL)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("WebGL DOJ File Loader", EditorStyles.boldLabel);
            
            // WebGL compatibility info
            EditorGUILayout.HelpBox(
                "🌐 WebGL Compatible Version\n" +
                "Uses UnityWebRequest for file loading - works in WebGL builds!", 
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Refresh files button
            if (GUILayout.Button("Refresh Available Files"))
            {
                RefreshAvailableFiles();
            }

            EditorGUILayout.Space(10);

            // File selection dropdown
            if (_availableDojFileNames != null && _availableDojFileNames.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Available DOJ Files:", GUILayout.Width(120));
                
                // Find current file index
                int currentIndex = System.Array.FindIndex(_availableDojFileNames, name => name == testLoader.nameFile);
                if (currentIndex == -1) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup(currentIndex, _availableDojFileNames);
                if (newIndex != currentIndex && newIndex >= 0 && newIndex < _availableDojFileNames.Length)
                {
                    testLoader.nameFile = _availableDojFileNames[newIndex];
                    EditorUtility.SetDirty(testLoader);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No .doj files found in StreamingAssets folder.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // Manual file name input
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Name:", GUILayout.Width(120));
            string newFileName = EditorGUILayout.TextField(testLoader.nameFile);
            if (newFileName != testLoader.nameFile)
            {
                testLoader.nameFile = newFileName;
                EditorUtility.SetDirty(testLoader);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Decryption password field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Decryption Password:", GUILayout.Width(120));
            string newPassword = EditorGUILayout.PasswordField(testLoader.decryptionPassword);
            if (newPassword != testLoader.decryptionPassword)
            {
                testLoader.decryptionPassword = newPassword;
                EditorUtility.SetDirty(testLoader);
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(testLoader.decryptionPassword))
            {
                EditorGUILayout.HelpBox("🔒 File will be decrypted using AES encryption.", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            // Show WebGL URL path
            string webglPath = System.IO.Path.Combine(Application.streamingAssetsPath, testLoader.nameFile);
            EditorGUILayout.LabelField("WebGL URL Path:", EditorStyles.miniLabel);
            EditorGUILayout.SelectableLabel(webglPath, EditorStyles.textField, GUILayout.Height(16));

            // File exists check (only works in editor)
            string editorPath = Path.Combine(Application.dataPath, "StreamingAssets", testLoader.nameFile);
            bool fileExists = File.Exists(editorPath);
            if (fileExists)
            {
                EditorGUILayout.HelpBox("✓ File exists in StreamingAssets", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ File not found in StreamingAssets folder", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // Loading settings
            EditorGUILayout.LabelField("Loading Settings", EditorStyles.boldLabel);
            testLoader.loadOnStart = EditorGUILayout.Toggle("Load on Start", testLoader.loadOnStart);
            testLoader.showLoadingProgress = EditorGUILayout.Toggle("Show Loading Progress", testLoader.showLoadingProgress);

            EditorGUILayout.Space(10);

            // Loading status
            if (Application.isPlaying)
            {
                if (testLoader.IsLoading)
                {
                    EditorGUILayout.HelpBox("🔄 Loading model...", MessageType.Info);
                    GUI.enabled = false;
                }
                else
                {
                    EditorGUILayout.HelpBox("✅ Ready to load", MessageType.None);
                }
            }

            // Load button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Model", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    testLoader.LoadModelFromStreamingAssets();
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required", 
                        "Please enter play mode to load the model.\n\nWebGL file loading requires runtime environment.", 
                        "OK");
                }
            }

            // Test file exists button (WebGL compatible)
            if (Application.isPlaying && GUILayout.Button("Test File Exists", GUILayout.Width(120)))
            {
                testLoader.CheckFileExists(testLoader.nameFile, (exists) => 
                {
                    string message = exists ? 
                        $"✅ File '{testLoader.nameFile}' is accessible via WebGL" : 
                        $"❌ File '{testLoader.nameFile}' is NOT accessible via WebGL";
                    Debug.Log(message);
                    
                    EditorUtility.DisplayDialog("WebGL File Check", message, "OK");
                });
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // Advanced options
            _showAdvancedOptions = EditorGUILayout.Foldout(_showAdvancedOptions, "Advanced Options");
            if (_showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Platform Information", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Current Platform: {Application.platform}");
                EditorGUILayout.LabelField($"StreamingAssets Path: {Application.streamingAssetsPath}");
                
                EditorGUILayout.Space(5);
                
                // WebGL specific info
                EditorGUILayout.LabelField("WebGL Compatibility", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "• Uses UnityWebRequest for file loading\n" +
                    "• Supports asynchronous loading with progress\n" +
                    "• Works with CORS-enabled servers\n" +
                    "• Supports AES decryption", 
                    MessageType.Info);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // WebGL build info
            EditorGUILayout.HelpBox(
                "📦 WebGL Build Notes:\n" +
                "• Files in StreamingAssets are automatically included\n" +
                "• Ensure your web server supports the .doj file type\n" +
                "• Loading is asynchronous and non-blocking\n" +
                "• AES encryption/decryption works in WebGL builds", 
                MessageType.Info);

            // Quick access buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open StreamingAssets"))
            {
                string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
                if (!Directory.Exists(streamingAssetsPath))
                {
                    Directory.CreateDirectory(streamingAssetsPath);
                }
                EditorUtility.RevealInFinder(streamingAssetsPath);
            }

            if (GUILayout.Button("WebGL Build Settings"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Private Methods
        private void RefreshAvailableFiles()
        {
            string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
            
            if (Directory.Exists(streamingAssetsPath))
            {
                _availableDojFiles = Directory.GetFiles(streamingAssetsPath, "*.doj", SearchOption.TopDirectoryOnly);
                _availableDojFileNames = _availableDojFiles.Select(Path.GetFileName).ToArray();
            }
            else
            {
                _availableDojFiles = new string[0];
                _availableDojFileNames = new string[0];
            }
        }
        #endregion
    }
} 