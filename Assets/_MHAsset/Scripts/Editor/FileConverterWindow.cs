using UnityEngine;
using UnityEditor;
using System.IO;

namespace MH.Editor
{
    public class FileConverterWindow : EditorWindow
    {
        private string inputPath = "";
        private string outputPath = "";
        private string password = "";
        private Vector2 scrollPosition;

        [MenuItem("Tools/File Converter")]
        public static void ShowWindow()
        {
            GetWindow<FileConverterWindow>("File Converter");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("GLB to DOJ Converter", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            inputPath = EditorGUILayout.TextField("Input Path (.glb)", inputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                inputPath = EditorUtility.OpenFilePanel("Select .glb file", "", "glb");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path (.doj)", outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                outputPath = EditorUtility.SaveFilePanel("Save .doj file", "", "output.doj", "doj");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            password = EditorGUILayout.PasswordField("Encryption Password (optional)", password);
            
            if (!string.IsNullOrEmpty(password))
            {
                EditorGUILayout.HelpBox("File will be AES encrypted with the provided password.", MessageType.Info);
            }

            EditorGUILayout.Space(20);

            GUI.enabled = !string.IsNullOrEmpty(inputPath) && !string.IsNullOrEmpty(outputPath);
            if (GUILayout.Button("Convert"))
            {
                try
                {
                    var converter = new FileConverter(inputPath, outputPath);
                    
                    // Choose conversion method based on settings
                    if (!string.IsNullOrEmpty(password))
                    {
                        // Use encryption
                        converter.ConvertGlbToDoj_EnCode(password);
                        EditorUtility.DisplayDialog("Success", "File converted and encrypted successfully!", "OK");
                    }
                    else
                    {
                        // Standard conversion
                        converter.ConvertGlbToDoj();
                        EditorUtility.DisplayDialog("Success", "File converted successfully!", "OK");
                    }
                }
                catch (IOException ex)
                {
                    EditorUtility.DisplayDialog("Error", ex.Message, "OK");
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }
    }
}