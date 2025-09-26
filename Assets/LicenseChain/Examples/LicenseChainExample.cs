using UnityEngine;
using UnityEngine.UI;
using LicenseChain;

namespace LicenseChain.Examples
{
    /// <summary>
    /// Example script demonstrating LicenseChain integration in Unity
    /// Based on advanced Unity integration with enhanced functionality
    /// </summary>
    public class LicenseChainExample : MonoBehaviour
    {
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
        [SerializeField] private GameObject adminPanel;

        [Header("Game Objects")]
        [SerializeField] private GameObject[] premiumFeatures;
        [SerializeField] private GameObject[] adminFeatures;

        private LicenseChainManager licenseChainManager;

        private void Start()
        {
            // Get the LicenseChain manager
            licenseChainManager = FindObjectOfType<LicenseChainManager>();
            if (licenseChainManager == null)
            {
                Debug.LogError("LicenseChainManager not found! Please add it to the scene.");
                return;
            }

            // Subscribe to events
            LicenseChainManager.OnLoginStatusChanged += OnLoginStatusChanged;
            LicenseChainManager.OnStatusMessage += OnStatusMessage;
            LicenseChainManager.OnUserDataReceived += OnUserDataReceived;

            // Setup UI
            SetupUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            LicenseChainManager.OnLoginStatusChanged -= OnLoginStatusChanged;
            LicenseChainManager.OnStatusMessage -= OnStatusMessage;
            LicenseChainManager.OnUserDataReceived -= OnUserDataReceived;
        }

        private void SetupUI()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);

            if (licenseLoginButton != null)
                licenseLoginButton.onClick.AddListener(OnLicenseLoginClicked);

            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            // Initially hide main panel and admin panel
            if (mainPanel != null)
                mainPanel.SetActive(false);

            if (adminPanel != null)
                adminPanel.SetActive(false);
        }

        private void OnLoginClicked()
        {
            if (licenseChainManager == null) return;

            string username = usernameField != null ? usernameField.text : "";
            string password = passwordField != null ? passwordField.text : "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Please enter username and password");
                return;
            }

            licenseChainManager.Login();
        }

        private void OnLicenseLoginClicked()
        {
            if (licenseChainManager == null) return;

            string license = licenseField != null ? licenseField.text : "";

            if (string.IsNullOrEmpty(license))
            {
                ShowStatus("Please enter a license key");
                return;
            }

            licenseChainManager.LicenseLogin();
        }

        private void OnLogoutClicked()
        {
            if (licenseChainManager == null) return;

            licenseChainManager.Logout();
        }

        private void OnLoginStatusChanged(bool isLoggedIn)
        {
            if (loginPanel != null)
                loginPanel.SetActive(!isLoggedIn);

            if (mainPanel != null)
                mainPanel.SetActive(isLoggedIn);

            if (isLoggedIn)
            {
                ShowStatus("Login successful!");
                CheckUserPermissions();
            }
            else
            {
                ShowStatus("Logged out");
                HideAllFeatures();
            }
        }

        private void OnStatusMessage(string message)
        {
            ShowStatus(message);
        }

        private void OnUserDataReceived(LicenseChainUser userData)
        {
            UpdateUserInfo(userData);
        }

        private void ShowStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                Debug.Log($"LicenseChain: {message}");
            }
        }

        private void UpdateUserInfo(LicenseChainUser userData)
        {
            if (userInfoText != null)
            {
                userInfoText.text = $"Welcome, {userData.username}!\n" +
                                  $"Email: {userData.email}\n" +
                                  $"License: {userData.license}\n" +
                                  $"Expires: {userData.expiry}";
            }
        }

        private void CheckUserPermissions()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            var userData = licenseChainManager.UserData;
            if (userData == null) return;

            // Check if user has premium features
            bool hasPremium = CheckPremiumAccess(userData);
            SetPremiumFeatures(hasPremium);

            // Check if user is admin
            bool isAdmin = CheckAdminAccess(userData);
            SetAdminFeatures(isAdmin);

            // Set user variables
            SetUserVariables(userData);
        }

        private bool CheckPremiumAccess(LicenseChainUser userData)
        {
            // Check if user has premium subscription
            if (userData.subscriptions != null)
            {
                foreach (string subscription in userData.subscriptions)
                {
                    if (subscription.ToLower().Contains("premium") || 
                        subscription.ToLower().Contains("pro") ||
                        subscription.ToLower().Contains("vip"))
                    {
                        return true;
                    }
                }
            }

            // Check license type
            if (userData.license != null)
            {
                string license = userData.license.ToLower();
                return license.Contains("premium") || 
                       license.Contains("pro") || 
                       license.Contains("vip");
            }

            return false;
        }

        private bool CheckAdminAccess(LicenseChainUser userData)
        {
            // Check if user has admin subscription
            if (userData.subscriptions != null)
            {
                foreach (string subscription in userData.subscriptions)
                {
                    if (subscription.ToLower().Contains("admin") || 
                        subscription.ToLower().Contains("moderator"))
                    {
                        return true;
                    }
                }
            }

            // Check if user is in admin list
            return userData.username.ToLower().Contains("admin") ||
                   userData.email.ToLower().Contains("admin@");
        }

        private void SetPremiumFeatures(bool hasPremium)
        {
            if (premiumFeatures == null) return;

            foreach (GameObject feature in premiumFeatures)
            {
                if (feature != null)
                {
                    feature.SetActive(hasPremium);
                }
            }
        }

        private void SetAdminFeatures(bool isAdmin)
        {
            if (adminPanel != null)
                adminPanel.SetActive(isAdmin);

            if (adminFeatures == null) return;

            foreach (GameObject feature in adminFeatures)
            {
                if (feature != null)
                {
                    feature.SetActive(isAdmin);
                }
            }
        }

        private void SetUserVariables(LicenseChainUser userData)
        {
            if (userData.variables == null) return;

            // Set user level
            if (userData.variables.ContainsKey("level"))
            {
                int level = int.Parse(userData.variables["level"]);
                SetUserLevel(level);
            }

            // Set user score
            if (userData.variables.ContainsKey("score"))
            {
                int score = int.Parse(userData.variables["score"]);
                SetUserScore(score);
            }

            // Set user preferences
            if (userData.variables.ContainsKey("theme"))
            {
                string theme = userData.variables["theme"];
                SetUserTheme(theme);
            }
        }

        private void SetUserLevel(int level)
        {
            // Update UI based on user level
            Debug.Log($"User level: {level}");
        }

        private void SetUserScore(int score)
        {
            // Update UI based on user score
            Debug.Log($"User score: {score}");
        }

        private void SetUserTheme(string theme)
        {
            // Update UI theme
            Debug.Log($"User theme: {theme}");
        }

        private void HideAllFeatures()
        {
            SetPremiumFeatures(false);
            SetAdminFeatures(false);
        }

        // Public methods for UI buttons
        public void OnSetVariableClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            // Example: Set user level
            licenseChainManager.SetVariable("level", "10");
            ShowStatus("Variable set successfully");
        }

        public void OnGetVariableClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            // Example: Get user level
            licenseChainManager.GetVariable("level", (value) =>
            {
                if (value != null)
                {
                    ShowStatus($"User level: {value}");
                }
                else
                {
                    ShowStatus("Variable not found");
                }
            });
        }

        public void OnLogMessageClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            // Example: Log user action
            licenseChainManager.LogMessage("User clicked log message button");
            ShowStatus("Message logged successfully");
        }

        public void OnDownloadFileClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            // Example: Download file
            licenseChainManager.DownloadFile("file_123", (content) =>
            {
                if (content != null)
                {
                    ShowStatus($"File downloaded: {content.Length} bytes");
                }
                else
                {
                    ShowStatus("File download failed");
                }
            });
        }

        public void OnGetAppStatsClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsInitialized) return;

            licenseChainManager.GetAppStats((stats) =>
            {
                if (stats != null)
                {
                    ShowStatus($"Total users: {stats.totalUsers}, Online: {stats.onlineUsers}");
                }
                else
                {
                    ShowStatus("Failed to get app stats");
                }
            });
        }

        public void OnGetOnlineUsersClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            licenseChainManager.GetOnlineUsers((users) =>
            {
                if (users != null)
                {
                    ShowStatus($"Online users: {users.Count}");
                    foreach (var user in users)
                    {
                        Debug.Log($"Online user: {user.username}");
                    }
                }
                else
                {
                    ShowStatus("Failed to get online users");
                }
            });
        }

        public void OnSendChatMessageClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            licenseChainManager.SendChatMessage("Hello from Unity!", "general");
            ShowStatus("Chat message sent");
        }

        public void OnGetChatMessagesClicked()
        {
            if (licenseChainManager == null || !licenseChainManager.IsLoggedIn) return;

            licenseChainManager.GetChatMessages("general", (messages) =>
            {
                if (messages != null)
                {
                    ShowStatus($"Chat messages: {messages.Count}");
                    foreach (var message in messages)
                    {
                        Debug.Log($"{message.username}: {message.message}");
                    }
                }
                else
                {
                    ShowStatus("Failed to get chat messages");
                }
            });
        }
    }
}
