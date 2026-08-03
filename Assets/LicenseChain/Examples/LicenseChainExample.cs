using LicenseChain.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace LicenseChain.Examples
{
    /// <summary>
    /// Basic API v1 example script for Unity integration.
    /// </summary>
    public class LicenseChainExample : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private string appId = "";
        [SerializeField] private string baseUrl = "https://api.licensechain.app/v1";

        [Header("UI References")]
        [SerializeField] private InputField usernameField;
        [SerializeField] private InputField passwordField;
        [SerializeField] private InputField licenseField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button licenseLoginButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text userInfoText;
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject mainPanel;

        private LicenseChainApiV1Client _client;

        private void Start()
        {
            var config = new LicenseChainConfig
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl
            };
            _client = new LicenseChainApiV1Client(config);
            SetupUI();
        }

        private void OnDestroy()
        {
            _client?.Dispose();
        }

        private void SetupUI()
        {
            if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
            if (licenseLoginButton != null) licenseLoginButton.onClick.AddListener(OnLicenseLoginClicked);
            if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);
            if (mainPanel != null) mainPanel.SetActive(false);
        }

        private async void OnLoginClicked()
        {
            var email = usernameField != null ? usernameField.text : "";
            var password = passwordField != null ? passwordField.text : "";
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Enter email and password");
                return;
            }

            try
            {
                await _client.RegisterUserAsync(email, password, "Unity User");
                var me = await _client.GetCurrentUserAsync();
                userInfoText.text = me.ToString();
                SetLoggedIn(true);
                ShowStatus("API v1 login flow completed");
            }
            catch (System.Exception ex)
            {
                ShowStatus($"Login failed: {ex.Message}");
            }
        }

        private async void OnLicenseLoginClicked()
        {
            var key = licenseField != null ? licenseField.text : "";
            if (string.IsNullOrWhiteSpace(key))
            {
                ShowStatus("Enter license key");
                return;
            }

            try
            {
                var result = await _client.ValidateLicenseAsync(key, appId);
                var valid = result["valid"]?.Value<bool>() ?? false;
                SetLoggedIn(valid);
                ShowStatus(valid ? "License is valid" : "License is invalid");
            }
            catch (System.Exception ex)
            {
                ShowStatus($"Validation failed: {ex.Message}");
            }
        }

        private void OnLogoutClicked()
        {
            SetLoggedIn(false);
            ShowStatus("Logged out");
        }

        private void SetLoggedIn(bool isLoggedIn)
        {
            if (loginPanel != null) loginPanel.SetActive(!isLoggedIn);
            if (mainPanel != null) mainPanel.SetActive(isLoggedIn);
        }

        private void ShowStatus(string message)
        {
            if (statusText != null) statusText.text = message;
            Debug.Log($"LicenseChain: {message}");
        }
    }
}
