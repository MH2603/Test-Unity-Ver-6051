using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace MH.Editor
{
    [CustomEditor(typeof(TestLoadDojFile))]
    public class TestLoadDojFileEditor : UnityEditor.Editor
    {
        #region Private Fields
        private string[] _availableDojFiles;
        private string[] _availableDojFileNames;
        private int _selectedFileIndex = 0;
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
            TestLoadDojFile testLoader = (TestLoadDojFile)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("StreamingAssets DOJ File Loader", EditorStyles.boldLabel);
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
                    testLoader.dojFilePath = Path.Combine(Application.streamingAssetsPath, testLoader.nameFile);
                    EditorUtility.SetDirty(testLoader);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No .doj files found in StreamingAssets folder.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // Manual file name input (for files not yet in StreamingAssets)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Name:", GUILayout.Width(120));
            string newFileName = EditorGUILayout.TextField(testLoader.nameFile);
            if (newFileName != testLoader.nameFile)
            {
                testLoader.nameFile = newFileName;
                testLoader.dojFilePath = Path.Combine(Application.streamingAssetsPath, testLoader.nameFile);
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

            // Show full path
            string fullPath = Path.Combine(Application.streamingAssetsPath, testLoader.nameFile);
            EditorGUILayout.LabelField("Full Path:", EditorStyles.miniLabel);
            EditorGUILayout.SelectableLabel(fullPath, EditorStyles.textField, GUILayout.Height(16));

            // File exists check
            bool fileExists = File.Exists(fullPath);
            if (fileExists)
            {
                EditorGUILayout.HelpBox("✓ File exists in StreamingAssets", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ File not found in StreamingAssets folder", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // Load button
            GUI.enabled = fileExists;
            if (GUILayout.Button("Load Model", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    testLoader.LoadModelFromStreamingAssets();
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required", 
                        "Please enter play mode to load the model.\n\nStreamingAssets files can only be accessed at runtime.", 
                        "OK");
                }
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // Additional info
            EditorGUILayout.HelpBox(
                "Files should be placed in Assets/StreamingAssets/ folder.\n" +
                "StreamingAssets files are included in builds and accessible at runtime.", 
                MessageType.Info);

            // Open StreamingAssets folder button
            if (GUILayout.Button("Open StreamingAssets Folder"))
            {
                string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
                if (!Directory.Exists(streamingAssetsPath))
                {
                    Directory.CreateDirectory(streamingAssetsPath);
                }
                EditorUtility.RevealInFinder(streamingAssetsPath);
            }
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