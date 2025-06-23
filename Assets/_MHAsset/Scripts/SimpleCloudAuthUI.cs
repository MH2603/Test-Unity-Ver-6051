using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CloudAuth;

public class SimpleCloudAuthUI : MonoBehaviour
{
    [Header("Authentication Service")]
    [SerializeField] private UnityCloudAuthService authService;

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    [Header("User Panel")]
    [SerializeField] private GameObject userPanel;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text userIdText;
    [SerializeField] private Button logoutButton;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text errorText;

    private void OnEnable()
    {
        // Find auth service if not assigned
        if (authService == null)
            authService = FindObjectOfType<UnityCloudAuthService>();

        // Subscribe to events
        if (authService != null)
        {
            authService.OnSignedIn += OnUserSignedIn;
            authService.OnSignedOut += OnUserSignedOut;
            authService.OnError += OnAuthError;
            authService.OnAuthStatusChanged += OnStatusChanged;
        }

        // Subscribe to button events
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);

        // Initialize UI
        UpdateUI();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (authService != null)
        {
            authService.OnSignedIn -= OnUserSignedIn;
            authService.OnSignedOut -= OnUserSignedOut;
            authService.OnError -= OnAuthError;
            authService.OnAuthStatusChanged -= OnStatusChanged;
        }

        // Unsubscribe from button events
        if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginClicked);
        if (registerButton != null) registerButton.onClick.RemoveListener(OnRegisterClicked);
        if (logoutButton != null) logoutButton.onClick.RemoveListener(OnLogoutClicked);
    }

    #region Button Events

    private async void OnLoginClicked()
    {
        if (!ValidateInput()) return;

        string username = usernameField.text.Trim();
        string password = passwordField.text;

        bool success = await authService.SignInWithUsernamePasswordAsync(username, password);
        
        if (success)
        {
            ClearPasswordField();
        }
    }

    private async void OnRegisterClicked()
    {
        if (!ValidateInput()) return;

        string username = usernameField.text.Trim();
        string password = passwordField.text;

        bool success = await authService.SignUpWithUsernamePasswordAsync(username, password);
        
        if (success)
        {
            ClearPasswordField();
        }
    }

    private void OnLogoutClicked()
    {
        authService.SignOut();
    }

    #endregion

    #region Auth Events

    private void OnUserSignedIn(PlayerProfile profile)
    {
        ShowUserPanel();
        UpdateUserInfo(profile);
        ClearError();
    }

    private void OnUserSignedOut()
    {
        ShowLoginPanel();
        ClearUserInfo();
        ClearError();
    }

    private void OnAuthError(string error)
    {
        ShowError(error);
    }

    private void OnStatusChanged(AuthStatus status)
    {
        UpdateStatus(status);
        UpdateButtonStates(status);
    }

    #endregion

    #region UI Updates

    private void UpdateUI()
    {
        if (authService != null && authService.IsSignedIn)
        {
            ShowUserPanel();
            UpdateUserInfo(authService.CurrentProfile);
        }
        else
        {
            ShowLoginPanel();
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

    private void UpdateUserInfo(PlayerProfile profile)
    {
        if (userNameText != null)
            userNameText.text = $"Welcome, {profile.Name}";
        
        if (userIdText != null)
            userIdText.text = $"ID: {profile.playerInfo.Id}";
    }

    private void ClearUserInfo()
    {
        if (userNameText != null) userNameText.text = "";
        if (userIdText != null) userIdText.text = "";
    }

    private void UpdateStatus(AuthStatus status)
    {
        if (statusText != null)
            statusText.text = $"Status: {status}";
    }

    private void UpdateButtonStates(AuthStatus status)
    {
        bool isProcessing = status == AuthStatus.SigningIn || status == AuthStatus.Initializing;
        
        if (loginButton != null) loginButton.interactable = !isProcessing;
        if (registerButton != null) registerButton.interactable = !isProcessing;
        if (logoutButton != null) logoutButton.interactable = !isProcessing;
    }

    #endregion

    #region Validation & Utility

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(usernameField.text))
        {
            ShowError("Please enter a username");
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordField.text))
        {
            ShowError("Please enter a password");
            return false;
        }

        if (passwordField.text.Length < 6)
        {
            ShowError("Password must be at least 6 characters");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
        
        Debug.LogError($"[SimpleCloudAuthUI] {message}");
    }

    private void ClearError()
    {
        if (errorText != null)
            errorText.text = "";
    }

    private void ClearPasswordField()
    {
        if (passwordField != null)
            passwordField.text = "";
    }

    #endregion
} 