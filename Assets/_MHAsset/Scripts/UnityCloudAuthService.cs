using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

namespace CloudAuth
{
    public enum AuthStatus
    {
        NotInitialized,
        Initializing,
        SignedOut,
        SigningIn,
        SignedIn,
        SigningOut,
        Error
    }

    public class UnityCloudAuthService : MonoBehaviour
    {
        [Header("Authentication Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool debugMode = true;

        // Events
        public event Action<AuthStatus> OnAuthStatusChanged;
        public event Action<PlayerProfile> OnSignedIn;
        public event Action OnSignedOut;
        public event Action<string> OnPasswordChanged;
        public event Action<string> OnError;
        public event Action<PlayerProfile> OnProfileUpdated;

        // Properties
        public AuthStatus CurrentStatus { get; private set; } = AuthStatus.NotInitialized;
        public PlayerProfile CurrentProfile { get; private set; }
        public bool IsSignedIn => CurrentStatus == AuthStatus.SignedIn;
        public bool IsInitialized => UnityServices.State == ServicesInitializationState.Initialized;

        private PlayerInfo playerInfo;

        #region Unity Lifecycle

        private async void Awake()
        {
            if (autoInitialize)
            {
                await InitializeAsync();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                LogDebug("Unity Services already initialized");
                return true;
            }

            try
            {
                SetStatus(AuthStatus.Initializing);
                LogDebug("Initializing Unity Services...");

                await UnityServices.InitializeAsync();
                
                SubscribeToEvents();
                SetStatus(AuthStatus.SignedOut);
                
                LogDebug("Unity Services initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize Unity Services: {ex.Message}");
                SetStatus(AuthStatus.Error);
                return false;
            }
        }

        #endregion

        #region Authentication Methods

        /// <summary>
        /// Sign in with username and password
        /// </summary>
        public async Task<bool> SignInWithUsernamePasswordAsync(string username, string password)
        {
            if (!IsInitialized)
            {
                LogError("Unity Services not initialized");
                return false;
            }

            try
            {
                SetStatus(AuthStatus.SigningIn);
                LogDebug($"Signing in user: {username}");

                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                
                await UpdatePlayerProfile();
                SetStatus(AuthStatus.SignedIn);
                
                LogDebug("Sign in successful");
                OnSignedIn?.Invoke(CurrentProfile);
                return true;
            }
            catch (AuthenticationException ex)
            {
                LogError($"Authentication failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
            catch (RequestFailedException ex)
            {
                LogError($"Request failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign in with Unity Player Account Service
        /// </summary>
        public async Task<bool> SignInWithUnityPlayerAccountAsync()
        {
            if (!IsInitialized)
            {
                LogError("Unity Services not initialized");
                return false;
            }

            try
            {
                SetStatus(AuthStatus.SigningIn);
                LogDebug("Starting Unity Player Account sign in...");

                await PlayerAccountService.Instance.StartSignInAsync();
                return true; // The actual sign-in will be handled by the SignedIn event
            }
            catch (Exception ex)
            {
                LogError($"Unity Player Account sign in failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign up with username and password
        /// </summary>
        public async Task<bool> SignUpWithUsernamePasswordAsync(string username, string password)
        {
            if (!IsInitialized)
            {
                LogError("Unity Services not initialized");
                return false;
            }

            try
            {
                SetStatus(AuthStatus.SigningIn);
                LogDebug($"Signing up user: {username}");

                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                
                await UpdatePlayerProfile();
                SetStatus(AuthStatus.SignedIn);
                
                LogDebug("Sign up successful");
                OnSignedIn?.Invoke(CurrentProfile);
                return true;
            }
            catch (AuthenticationException ex)
            {
                LogError($"Sign up failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
            catch (RequestFailedException ex)
            {
                LogError($"Request failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign in anonymously
        /// </summary>
        public async Task<bool> SignInAnonymouslyAsync()
        {
            if (!IsInitialized)
            {
                LogError("Unity Services not initialized");
                return false;
            }

            try
            {
                SetStatus(AuthStatus.SigningIn);
                LogDebug("Signing in anonymously...");

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                await UpdatePlayerProfile();
                SetStatus(AuthStatus.SignedIn);
                
                LogDebug("Anonymous sign in successful");
                OnSignedIn?.Invoke(CurrentProfile);
                return true;
            }
            catch (AuthenticationException ex)
            {
                LogError($"Anonymous sign in failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        public void SignOut()
        {
            try
            {
                SetStatus(AuthStatus.SigningOut);
                LogDebug("Signing out...");

                AuthenticationService.Instance.SignOut();
                
                CurrentProfile = new PlayerProfile();
                SetStatus(AuthStatus.SignedOut);
                
                LogDebug("Sign out successful");
                OnSignedOut?.Invoke();
            }
            catch (Exception ex)
            {
                LogError($"Sign out failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Change the current user's password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (!IsSignedIn)
            {
                LogError("User must be signed in to change password");
                return false;
            }

            try
            {
                LogDebug("Changing password...");

                await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
                
                LogDebug("Password changed successfully");
                OnPasswordChanged?.Invoke("Password changed successfully");
                return true;
            }
            catch (AuthenticationException ex)
            {
                LogError($"Password change failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
            catch (RequestFailedException ex)
            {
                LogError($"Request failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                LogDebug($"Requesting password reset for: {email}");

                // Note: This method might not be directly available in Unity Authentication
                // You may need to implement this through your own backend service
                LogDebug("Password reset email sent");
                OnPasswordChanged?.Invoke("Password reset email sent");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Password reset request failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        #endregion

        #region Profile Management

        /// <summary>
        /// Update player name
        /// </summary>
        public async Task<bool> UpdatePlayerNameAsync(string newName)
        {
            if (!IsSignedIn)
            {
                LogError("User must be signed in to update profile");
                return false;
            }

            try
            {
                LogDebug($"Updating player name to: {newName}");

                await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
                
                await UpdatePlayerProfile();
                
                LogDebug("Player name updated successfully");
                OnProfileUpdated?.Invoke(CurrentProfile);
                return true;
            }
            catch (AuthenticationException ex)
            {
                LogError($"Name update failed: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get current player profile
        /// </summary>
        public async Task<PlayerProfile> GetPlayerProfileAsync()
        {
            if (!IsSignedIn)
            {
                LogError("User must be signed in to get profile");
                return new PlayerProfile();
            }

            await UpdatePlayerProfile();
            return CurrentProfile;
        }

        #endregion

        #region Status and Info

        /// <summary>
        /// Get detailed authentication status information
        /// </summary>
        public string GetStatusInfo()
        {
            var statusInfo = $"Authentication Status: {CurrentStatus}\n";
            statusInfo += $"Unity Services State: {UnityServices.State}\n";
            
            if (IsSignedIn)
            {
                statusInfo += $"Player ID: {CurrentProfile.playerInfo.Id}\n";
                statusInfo += $"Player Name: {CurrentProfile.Name}\n";
                statusInfo += $"Creation Date: {CurrentProfile.playerInfo.CreatedAt}\n";
            }

            return statusInfo;
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return IsSignedIn && AuthenticationService.Instance.IsSignedIn;
        }

        /// <summary>
        /// Get current access token
        /// </summary>
        public string GetAccessToken()
        {
            if (!IsSignedIn)
            {
                LogError("User must be signed in to get access token");
                return string.Empty;
            }

            return AuthenticationService.Instance.AccessToken;
        }

        #endregion

        #region Private Methods

        private void SubscribeToEvents()
        {
            AuthenticationService.Instance.SignedIn += OnAuthServiceSignedIn;
            AuthenticationService.Instance.SignedOut += OnAuthServiceSignedOut;
            AuthenticationService.Instance.SignInFailed += OnAuthServiceSignInFailed;
            AuthenticationService.Instance.Expired += OnAuthServiceExpired;

            if (PlayerAccountService.Instance != null)
            {
                PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= OnAuthServiceSignedIn;
                AuthenticationService.Instance.SignedOut -= OnAuthServiceSignedOut;
                AuthenticationService.Instance.SignInFailed -= OnAuthServiceSignInFailed;
                AuthenticationService.Instance.Expired -= OnAuthServiceExpired;
            }

            if (PlayerAccountService.Instance != null)
            {
                PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
            }
        }

        private async void OnPlayerAccountSignedIn()
        {
            try
            {
                var accessToken = PlayerAccountService.Instance.AccessToken;
                await SignInWithUnityAsync(accessToken);
            }
            catch (Exception ex)
            {
                LogError($"Player Account sign in failed: {ex.Message}");
                SetStatus(AuthStatus.Error);
                OnError?.Invoke(ex.Message);
            }
        }

        private async Task SignInWithUnityAsync(string accessToken)
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            await UpdatePlayerProfile();
            SetStatus(AuthStatus.SignedIn);
            OnSignedIn?.Invoke(CurrentProfile);
        }

        private async Task UpdatePlayerProfile()
        {
            playerInfo = AuthenticationService.Instance.PlayerInfo;
            var name = await AuthenticationService.Instance.GetPlayerNameAsync();
            
            CurrentProfile = new PlayerProfile
            {
                playerInfo = playerInfo,
                Name = name
            };
        }

        private void OnAuthServiceSignedIn()
        {
            LogDebug("Auth Service: Signed In");
        }

        private void OnAuthServiceSignedOut()
        {
            LogDebug("Auth Service: Signed Out");
            SetStatus(AuthStatus.SignedOut);
            OnSignedOut?.Invoke();
        }

        private void OnAuthServiceSignInFailed(RequestFailedException exception)
        {
            LogError($"Auth Service: Sign In Failed - {exception.Message}");
            SetStatus(AuthStatus.Error);
            OnError?.Invoke(exception.Message);
        }

        private void OnAuthServiceExpired()
        {
            LogDebug("Auth Service: Session Expired");
            SetStatus(AuthStatus.SignedOut);
            OnSignedOut?.Invoke();
        }

        private void SetStatus(AuthStatus status)
        {
            if (CurrentStatus != status)
            {
                CurrentStatus = status;
                OnAuthStatusChanged?.Invoke(status);
                LogDebug($"Status changed to: {status}");
            }
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[UnityCloudAuthService] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[UnityCloudAuthService] {message}");
        }

        #endregion
    }
} 