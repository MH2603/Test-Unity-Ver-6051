using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CloudAuth;

public class CloudAuthUIController : MonoBehaviour
{
    [Header("Authentication Service")]
    [SerializeField] private UnityCloudAuthService authService;

    [Header("Login/Signup Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signupButton;
    [SerializeField] private Button anonymousLoginButton;
    [SerializeField] private Button unityAccountButton;

    [Header("User Profile Panel")]
    [SerializeField] private GameObject userPanel;
    [SerializeField] private TMP_Text userIdText;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button updateNameButton;

    [Header("Change Password Panel")]
    [SerializeField] private GameObject changePasswordPanel;
    [SerializeField] private TMP_InputField currentPasswordField;
    [SerializeField] private TMP_InputField newPasswordField;
    [SerializeField] private TMP_InputField confirmPasswordField;
    [SerializeField] private Button confirmChangePasswordButton;
    [SerializeField] private Button cancelChangePasswordButton;

    [Header("Update Name Panel")]
    [SerializeField] private GameObject updateNamePanel;
    [SerializeField] private TMP_InputField newNameInputField;
    [SerializeField] private Button confirmUpdateNameButton;
    [SerializeField] private Button cancelUpdateNameButton;

    [Header("Status Display")]
    [SerializeField] private TMP_Text authStatusText;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Settings")]
    [SerializeField] private bool clearPasswordFields = true;
    [SerializeField] private float errorMessageDuration = 5f;

    private PlayerProfile currentPlayerProfile;

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (authService == null)
        {
            authService = FindObjectOfType<UnityCloudAuthService>();
        }

        if (authService != null)
        {
            SubscribeToAuthEvents();
        }

        SubscribeToUIEvents();
        InitializeUI();
    }

    private void OnDisable()
    {
        UnsubscribeFromAuthEvents();
        UnsubscribeFromUIEvents();
    }

    #endregion

    #region Event Subscriptions

    private void SubscribeToAuthEvents()
    {
        authService.OnAuthStatusChanged += OnAuthStatusChanged;
        authService.OnSignedIn += OnSignedIn;
        authService.OnSignedOut += OnSignedOut;
        authService.OnPasswordChanged += OnPasswordChanged;
        authService.OnError += OnError;
        authService.OnProfileUpdated += OnProfileUpdated;
    }

    private void UnsubscribeFromAuthEvents()
    {
        if (authService != null)
        {
            authService.OnAuthStatusChanged -= OnAuthStatusChanged;
            authService.OnSignedIn -= OnSignedIn;
            authService.OnSignedOut -= OnSignedOut;
            authService.OnPasswordChanged -= OnPasswordChanged;
            authService.OnError -= OnError;
            authService.OnProfileUpdated -= OnProfileUpdated;
        }
    }

    private void SubscribeToUIEvents()
    {
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginButtonClicked);
        if (signupButton != null) signupButton.onClick.AddListener(OnSignupButtonClicked);
        if (anonymousLoginButton != null) anonymousLoginButton.onClick.AddListener(OnAnonymousLoginClicked);
        if (unityAccountButton != null) unityAccountButton.onClick.AddListener(OnUnityAccountClicked);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        if (changePasswordButton != null) changePasswordButton.onClick.AddListener(OnChangePasswordButtonClicked);
        if (updateNameButton != null) updateNameButton.onClick.AddListener(OnUpdateNameButtonClicked);
        if (confirmChangePasswordButton != null) confirmChangePasswordButton.onClick.AddListener(OnConfirmChangePasswordClicked);
        if (cancelChangePasswordButton != null) cancelChangePasswordButton.onClick.AddListener(OnCancelChangePasswordClicked);
        if (confirmUpdateNameButton != null) confirmUpdateNameButton.onClick.AddListener(OnConfirmUpdateNameClicked);
        if (cancelUpdateNameButton != null) cancelUpdateNameButton.onClick.AddListener(OnCancelUpdateNameClicked);
    }

    private void UnsubscribeFromUIEvents()
    {
        if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        if (signupButton != null) signupButton.onClick.RemoveListener(OnSignupButtonClicked);
        if (anonymousLoginButton != null) anonymousLoginButton.onClick.RemoveListener(OnAnonymousLoginClicked);
        if (unityAccountButton != null) unityAccountButton.onClick.RemoveListener(OnUnityAccountClicked);
        if (logoutButton != null) logoutButton.onClick.RemoveListener(OnLogoutButtonClicked);
        if (changePasswordButton != null) changePasswordButton.onClick.RemoveListener(OnChangePasswordButtonClicked);
        if (updateNameButton != null) updateNameButton.onClick.RemoveListener(OnUpdateNameButtonClicked);
        if (confirmChangePasswordButton != null) confirmChangePasswordButton.onClick.RemoveListener(OnConfirmChangePasswordClicked);
        if (cancelChangePasswordButton != null) cancelChangePasswordButton.onClick.RemoveListener(OnCancelChangePasswordClicked);
        if (confirmUpdateNameButton != null) confirmUpdateNameButton.onClick.RemoveListener(OnConfirmUpdateNameClicked);
        if (cancelUpdateNameButton != null) cancelUpdateNameButton.onClick.RemoveListener(OnCancelUpdateNameClicked);
    }

    #endregion

    #region UI Event Handlers

    private async void OnLoginButtonClicked()
    {
        if (!ValidateLoginInput()) return;

        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text;

        bool success = await authService.SignInWithUsernamePasswordAsync(username, password);
        
        if (clearPasswordFields && success)
        {
            ClearPasswordFields();
        }
    }

    private async void OnSignupButtonClicked()
    {
        if (!ValidateLoginInput()) return;

        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text;

        bool success = await authService.SignUpWithUsernamePasswordAsync(username, password);
        
        if (clearPasswordFields && success)
        {
            ClearPasswordFields();
        }
    }

    private async void OnAnonymousLoginClicked()
    {
        await authService.SignInAnonymouslyAsync();
    }

    private async void OnUnityAccountClicked()
    {
        await authService.SignInWithUnityPlayerAccountAsync();
    }

    private void OnLogoutButtonClicked()
    {
        authService.SignOut();
    }

    private void OnChangePasswordButtonClicked()
    {
        ShowChangePasswordPanel();
    }

    private void OnUpdateNameButtonClicked()
    {
        ShowUpdateNamePanel();
    }

    private async void OnConfirmChangePasswordClicked()
    {
        if (!ValidatePasswordChangeInput()) return;

        string currentPassword = currentPasswordField.text;
        string newPassword = newPasswordField.text;

        bool success = await authService.ChangePasswordAsync(currentPassword, newPassword);
        
        if (success)
        {
            HideChangePasswordPanel();
            ClearPasswordChangeFields();
        }
    }

    private void OnCancelChangePasswordClicked()
    {
        HideChangePasswordPanel();
        ClearPasswordChangeFields();
    }

    private async void OnConfirmUpdateNameClicked()
    {
        if (!ValidateNameUpdateInput()) return;

        string newName = newNameInputField.text.Trim();
        bool success = await authService.UpdatePlayerNameAsync(newName);
        
        if (success)
        {
            HideUpdateNamePanel();
            newNameInputField.text = "";
        }
    }

    private void OnCancelUpdateNameClicked()
    {
        HideUpdateNamePanel();
        newNameInputField.text = "";
    }

    #endregion

    #region Auth Event Handlers

    private void OnAuthStatusChanged(AuthStatus status)
    {
        UpdateAuthStatusDisplay(status);
        UpdateUIForStatus(status);
    }

    private void OnSignedIn(PlayerProfile profile)
    {
        currentPlayerProfile = profile;
        ShowUserPanel();
        UpdateUserProfileDisplay(profile);
        ClearErrorMessage();
    }

    private void OnSignedOut()
    {
        currentPlayerProfile = new PlayerProfile();
        ShowLoginPanel();
        ClearUserProfileDisplay();
        ClearErrorMessage();
    }

    private void OnPasswordChanged(string message)
    {
        ShowMessage(message, false);
    }

    private void OnError(string errorMessage)
    {
        ShowMessage(errorMessage, true);
    }

    private void OnProfileUpdated(PlayerProfile profile)
    {
        currentPlayerProfile = profile;
        UpdateUserProfileDisplay(profile);
        ShowMessage("Profile updated successfully", false);
    }

    #endregion

    #region UI Management

    private void InitializeUI()
    {
        if (authService != null && authService.IsSignedIn)
        {
            ShowUserPanel();
            UpdateUserProfileDisplay(authService.CurrentProfile);
        }
        else
        {
            ShowLoginPanel();
        }

        HideChangePasswordPanel();
        HideUpdateNamePanel();
        ClearErrorMessage();
        
        if (authService != null)
        {
            UpdateAuthStatusDisplay(authService.CurrentStatus);
        }
    }

    private void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (userPanel != null) userPanel.SetActive(false);
    }

    private void ShowUserPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (userPanel != null) userPanel.SetActive(true);
    }

    private void ShowChangePasswordPanel()
    {
        if (changePasswordPanel != null) changePasswordPanel.SetActive(true);
    }

    private void HideChangePasswordPanel()
    {
        if (changePasswordPanel != null) changePasswordPanel.SetActive(false);
    }

    private void ShowUpdateNamePanel()
    {
        if (updateNamePanel != null) 
        {
            updateNamePanel.SetActive(true);
            if (newNameInputField != null && !string.IsNullOrEmpty(currentPlayerProfile.Name))
            {
                newNameInputField.text = currentPlayerProfile.Name;
            }
        }
    }

    private void HideUpdateNamePanel()
    {
        if (updateNamePanel != null) updateNamePanel.SetActive(false);
    }

    private void UpdateUIForStatus(AuthStatus status)
    {
        bool isLoading = status == AuthStatus.SigningIn || status == AuthStatus.SigningOut || status == AuthStatus.Initializing;
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(isLoading);

        // Disable interactive elements during loading
        SetInteractable(loginButton, !isLoading);
        SetInteractable(signupButton, !isLoading);
        SetInteractable(anonymousLoginButton, !isLoading);
        SetInteractable(unityAccountButton, !isLoading);
        SetInteractable(logoutButton, !isLoading && status == AuthStatus.SignedIn);
    }

    private void SetInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void UpdateAuthStatusDisplay(AuthStatus status)
    {
        if (authStatusText != null)
        {
            authStatusText.text = $"Status: {status}";
        }
    }

    private void UpdateUserProfileDisplay(PlayerProfile profile)
    {
        if (userIdText != null)
            userIdText.text = $"ID: {profile.playerInfo.Id}";
        
        if (userNameText != null)
            userNameText.text = $"Name: {profile.Name}";
        
        if (statusText != null && authService != null)
            statusText.text = authService.GetStatusInfo();
    }

    private void ClearUserProfileDisplay()
    {
        if (userIdText != null) userIdText.text = "ID: Not logged in";
        if (userNameText != null) userNameText.text = "Name: Not logged in";
        if (statusText != null) statusText.text = "";
    }

    #endregion

    #region Input Validation

    private bool ValidateLoginInput()
    {
        if (string.IsNullOrWhiteSpace(usernameInputField.text))
        {
            ShowMessage("Please enter a username", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordInputField.text))
        {
            ShowMessage("Please enter a password", true);
            return false;
        }

        return true;
    }

    private bool ValidatePasswordChangeInput()
    {
        if (string.IsNullOrWhiteSpace(currentPasswordField.text))
        {
            ShowMessage("Please enter your current password", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newPasswordField.text))
        {
            ShowMessage("Please enter a new password", true);
            return false;
        }

        if (newPasswordField.text != confirmPasswordField.text)
        {
            ShowMessage("New passwords do not match", true);
            return false;
        }

        if (newPasswordField.text.Length < 6)
        {
            ShowMessage("New password must be at least 6 characters", true);
            return false;
        }

        return true;
    }

    private bool ValidateNameUpdateInput()
    {
        if (string.IsNullOrWhiteSpace(newNameInputField.text))
        {
            ShowMessage("Please enter a new name", true);
            return false;
        }

        return true;
    }

    #endregion

    #region Utility Methods

    private void ClearPasswordFields()
    {
        if (passwordInputField != null) passwordInputField.text = "";
    }

    private void ClearPasswordChangeFields()
    {
        if (currentPasswordField != null) currentPasswordField.text = "";
        if (newPasswordField != null) newPasswordField.text = "";
        if (confirmPasswordField != null) confirmPasswordField.text = "";
    }

    private void ShowMessage(string message, bool isError)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = isError ? Color.red : Color.green;
            
            if (errorMessageDuration > 0)
            {
                Invoke(nameof(ClearErrorMessage), errorMessageDuration);
            }
        }
        
        Debug.Log($"[CloudAuthUI] {(isError ? "Error" : "Info")}: {message}");
    }

    private void ClearErrorMessage()
    {
        if (errorMessageText != null)
            errorMessageText.text = "";
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually refresh the UI display
    /// </summary>
    public void RefreshUI()
    {
        if (authService != null)
        {
            UpdateAuthStatusDisplay(authService.CurrentStatus);
            
            if (authService.IsSignedIn)
            {
                UpdateUserProfileDisplay(authService.CurrentProfile);
                ShowUserPanel();
            }
            else
            {
                ShowLoginPanel();
            }
        }
    }

    /// <summary>
    /// Set the authentication service reference
    /// </summary>
    public void SetAuthService(UnityCloudAuthService service)
    {
        if (authService != null)
        {
            UnsubscribeFromAuthEvents();
        }

        authService = service;
        
        if (authService != null)
        {
            SubscribeToAuthEvents();
            RefreshUI();
        }
    }

    #endregion
} 