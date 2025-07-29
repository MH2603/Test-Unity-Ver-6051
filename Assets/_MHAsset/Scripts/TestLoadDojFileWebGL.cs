using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using DUCK.Crypto;

namespace MH
{
    public class TestLoadDojFileWebGL : MonoBehaviour
    {
        #region Public Fields
        [Header("File Settings")]
        public string nameFile = "ear_doji.doj"; // Default to the file in StreamingAssets
        
        [Header("Decryption Settings")]
        public string decryptionPassword = ""; // Password for AES decryption (leave empty if not encrypted)
        
        [Header("Loading Settings")]
        public bool loadOnStart = true;
        public bool showLoadingProgress = true;
        #endregion

        #region Private Fields
        private bool _isLoading = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the URL/path to the .doj file in StreamingAssets folder (WebGL compatible)
        /// </summary>
        private string StreamingAssetsFilePath
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // On WebGL, StreamingAssets is accessed via URL
                return System.IO.Path.Combine(Application.streamingAssetsPath, nameFile);
#else
                // On other platforms, use normal path
                return System.IO.Path.Combine(Application.streamingAssetsPath, nameFile);
#endif
            }
        }

        /// <summary>
        /// Check if currently loading a model
        /// </summary>
        public bool IsLoading => _isLoading;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (loadOnStart)
            {
                LoadModelFromStreamingAssets();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load model using the current nameFile setting (WebGL compatible)
        /// </summary>
        public void LoadModelFromStreamingAssets()
        {
            if (_isLoading)
            {
                Debug.LogWarning("Already loading a model. Please wait...");
                return;
            }

            StartCoroutine(LoadDoJFileCoroutine(StreamingAssetsFilePath, transform));
        }

        /// <summary>
        /// Load model from StreamingAssets with custom parent transform
        /// </summary>
        /// <param name="parentTransform">Parent transform for the loaded model</param>
        public void LoadModelFromStreamingAssets(Transform parentTransform)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Already loading a model. Please wait...");
                return;
            }

            StartCoroutine(LoadDoJFileCoroutine(StreamingAssetsFilePath, parentTransform));
        }

        /// <summary>
        /// Load model with custom password for decryption
        /// </summary>
        /// <param name="password">Password for decryption</param>
        public void LoadModelFromStreamingAssets(string password)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Already loading a model. Please wait...");
                return;
            }

            string tempPassword = decryptionPassword;
            decryptionPassword = password;
            StartCoroutine(LoadDoJFileCoroutine(StreamingAssetsFilePath, transform));
            decryptionPassword = tempPassword;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Coroutine to load .doj file using UnityWebRequest (WebGL compatible)
        /// </summary>
        /// <param name="filePath">Path/URL to the .doj file</param>
        /// <param name="parentTransform">Parent transform for the loaded model</param>
        private IEnumerator LoadDoJFileCoroutine(string filePath, Transform parentTransform)
        {
            _isLoading = true;
            
            Debug.Log($"Loading model from StreamingAssets (WebGL): {filePath}");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
            {
                // Send the request
                var operation = webRequest.SendWebRequest();

                // Show loading progress if enabled
                if (showLoadingProgress)
                {
                    while (!operation.isDone)
                    {
                        Debug.Log($"Loading progress: {(operation.progress * 100):F1}%");
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    yield return operation;
                }

                // Check for errors
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load file from StreamingAssets: {webRequest.error}");
                    Debug.LogError($"File path: {filePath}");
                    _isLoading = false;
                    yield break;
                }

                // Get the downloaded data
                byte[] data = webRequest.downloadHandler.data;
                
                if (data == null || data.Length == 0)
                {
                    Debug.LogError("Downloaded data is null or empty");
                    _isLoading = false;
                    yield break;
                }

                Debug.Log($"Successfully downloaded {data.Length} bytes");

                // Process the data (decrypt if needed)
                byte[] processedData = ProcessDownloadedData(data);
                
                if (processedData == null)
                {
                    Debug.LogError("Failed to process downloaded data");
                    _isLoading = false;
                    yield break;
                }

                // Load the model using glTFast
                yield return StartCoroutine(LoadModelWithGltfFastCoroutine(processedData, parentTransform));
            }

            _isLoading = false;
        }

        /// <summary>
        /// Process downloaded data by decrypting if needed
        /// </summary>
        /// <param name="data">Raw downloaded data</param>
        /// <returns>Processed data ready for model loading</returns>
        private byte[] ProcessDownloadedData(byte[] data)
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
                Debug.LogError($"Error processing data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Coroutine to load model using glTFast library
        /// </summary>
        /// <param name="modelData">The model data bytes</param>
        /// <param name="parentTransform">Parent transform for the loaded model</param>
        private IEnumerator LoadModelWithGltfFastCoroutine(byte[] modelData, Transform parentTransform)
        {
            var gltf = new GLTFast.GltfImport();
            
            // Load the model data
            var loadTask = gltf.Load(modelData);
            
            // Wait for the task to complete
            while (!loadTask.IsCompleted)
            {
                yield return null;
            }

            // Check if the task completed successfully
            if (loadTask.IsFaulted || loadTask.IsCanceled)
            {
                Debug.LogError($"Failed to load model: {loadTask.Exception?.GetBaseException()?.Message}");
                yield break;
            }

            bool success = loadTask.Result;
            Debug.Log($"Model loaded with glTFast: {success}");

            if (success)
            {
                // Instantiate the model
                var instantiateTask = gltf.InstantiateMainSceneAsync(parentTransform);
                
                // Wait for instantiation to complete
                while (!instantiateTask.IsCompleted)
                {
                    yield return null;
                }

                // Check instantiation result
                if (instantiateTask.IsFaulted || instantiateTask.IsCanceled)
                {
                    Debug.LogError($"Failed to instantiate model: {instantiateTask.Exception?.GetBaseException()?.Message}");
                }
                else
                {
                    Debug.Log("Model loaded and instantiated successfully from StreamingAssets (WebGL)");
                }
            }
            else
            {
                Debug.LogError("Failed to load model with glTFast");
            }
        }
        #endregion

        #region Public Utility Methods
        /// <summary>
        /// Check if the specified file exists in StreamingAssets (WebGL compatible)
        /// </summary>
        /// <param name="fileName">Name of the file to check</param>
        /// <param name="callback">Callback with the result</param>
        public void CheckFileExists(string fileName, System.Action<bool> callback)
        {
            StartCoroutine(CheckFileExistsCoroutine(fileName, callback));
        }

        private IEnumerator CheckFileExistsCoroutine(string fileName, System.Action<bool> callback)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            
            using (UnityWebRequest webRequest = UnityWebRequest.Head(filePath))
            {
                yield return webRequest.SendWebRequest();
                callback?.Invoke(webRequest.result == UnityWebRequest.Result.Success);
            }
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Load Model (WebGL Compatible)")]
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

        [ContextMenu("Check File Exists")]
        private void CheckFileExistsContext()
        {
            if (Application.isPlaying)
            {
                CheckFileExists(nameFile, (exists) => 
                {
                    Debug.Log($"File '{nameFile}' exists in StreamingAssets: {exists}");
                });
            }
            else
            {
                Debug.LogWarning("Please enter play mode to check file existence.");
            }
        }
#endif
    }
} 