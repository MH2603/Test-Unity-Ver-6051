using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using DUCK.Crypto;

namespace MH
{
    public class TestLoadDojFile : MonoBehaviour
    {
        #region Public Fields
        [Header("File Settings")]
        public string nameFile = "ear_doji.doj"; // Default to the file in StreamingAssets
        
        [Header("Decryption Settings")]
        public string decryptionPassword = ""; // Password for AES decryption (leave empty if not encrypted)
        
        [HideInInspector]
        public string dojFilePath; // This will be automatically set to StreamingAssets path
        #endregion

        #region Properties
        /// <summary>
        /// Gets the full path to the .doj file in StreamingAssets folder
        /// </summary>
        private string StreamingAssetsFilePath => Path.Combine(Application.streamingAssetsPath, nameFile);
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            
            // Update the dojFilePath to use StreamingAssets
            dojFilePath = StreamingAssetsFilePath;
            LoadDoJFile(dojFilePath, transform);
        }
        #endregion

        #region Public Methods
        public async Task LoadDoJFile(string path, Transform parentTransform) 
        {
            if (!File.Exists(path)) 
            {
                Debug.LogError($"File not found: {path}");
                return;
            }
            
            byte[] data = File.ReadAllBytes(path);
            byte[] processedData = ProcessFileData(data);
            
            if (processedData == null)
            {
                Debug.LogError("Failed to process file data");
                return;
            }
            
            Debug.Log($"Loading model from StreamingAssets: {path}");
            var gltf = new GLTFast.GltfImport();
            
            try
            {
                bool success = await gltf.Load(processedData);
                Debug.Log($"Model loaded with glTFast: {success}");
                
                if (!success) 
                {
                    // fallback - try loading the original file
                    Debug.LogWarning("Fallback: Trying to load original file...");
                    success = await gltf.Load(path);
                }
                
                if (success) 
                {
                    await gltf.InstantiateMainSceneAsync(parentTransform);
                    Debug.Log("Model loaded and instantiated successfully from StreamingAssets");
                } 
                else 
                {
                    Debug.LogError("Failed to load model with glTFast");
                }
            }
            catch (System.Exception e) 
            {
                Debug.LogError($"Failed to load model: {e.Message}");
                return;
            }
        }

        /// <summary>
        /// Load model using the current nameFile setting
        /// </summary>
        public void LoadModelFromStreamingAssets()
        {
            dojFilePath = StreamingAssetsFilePath;
            LoadDoJFile(dojFilePath, transform);
        }

        /// <summary>
        /// Load model with custom password for decryption
        /// </summary>
        /// <param name="password">Password for decryption</param>
        public void LoadModelFromStreamingAssets(string password)
        {
            string tempPassword = decryptionPassword;
            decryptionPassword = password;
            LoadModelFromStreamingAssets();
            decryptionPassword = tempPassword;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Process file data by decrypting if needed
        /// </summary>
        /// <param name="data">Raw file data</param>
        /// <returns>Processed data ready for model loading</returns>
        private byte[] ProcessFileData(byte[] data)
        {
            try
            {
                byte[] processedData = data;

                // Decrypt if password is provided
                if (!string.IsNullOrEmpty(decryptionPassword))
                {
                    Debug.Log("Decrypting data with AES...");
                    processedData = SimpleAESEncryption.Decode(processedData, decryptionPassword);
                    Debug.Log($"Decrypted data: {processedData.Length} bytes");
                }

                return processedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing file data: {ex.Message}");
                return null;
            }
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Load Model")]
        private void LoadModelContext()
        {
            if (Application.isPlaying)
            {
                LoadModelFromStreamingAssets();
            }
            else
            {
                Debug.LogWarning("Please enter play mode to load the model.");
            }
        }
#endif
    }
}