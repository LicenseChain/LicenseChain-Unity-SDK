using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Advanced example demonstrating comprehensive LicenseChain SDK usage
    /// </summary>
    public class AdvancedLicenseChainExample : MonoBehaviour
    {
        [Header("UI References")]
        public Text statusText;
        public Text logText;
        public Button createLicenseButton;
        public Button validateLicenseButton;
        public Button createUserButton;
        public Button webhookTestButton;
        public InputField licenseKeyInput;
        public InputField userIdInput;
        public InputField productIdInput;

        private LicenseChainManager _licenseManager;
        private WebhookHandler _webhookHandler;
        private List<string> _logMessages = new List<string>();

        private void Start()
        {
            InitializeSDK();
            SetupUI();
            LogMessage("Advanced LicenseChain Example Started");
        }

        private void InitializeSDK()
        {
            // Configure the SDK
            var config = new LicenseChainConfig
            {
                ApiKey = "your-api-key-here",
                BaseUrl = "https://api.licensechain.app",
                Timeout = 30000,
                Retries = 3,
                EnableLogging = true
            };

            _licenseManager = new LicenseChainManager(config);
            _webhookHandler = new WebhookHandler("your-webhook-secret");

            // Setup logging
            Logger.SetLogLevel(Logger.LogLevel.Debug);
            Logger.SetFileLogging(true);

            LogMessage("SDK Initialized Successfully");
        }

        private void SetupUI()
        {
            createLicenseButton.onClick.AddListener(CreateLicense);
            validateLicenseButton.onClick.AddListener(ValidateLicense);
            createUserButton.onClick.AddListener(CreateUser);
            webhookTestButton.onClick.AddListener(TestWebhook);

            // Set default values
            userIdInput.text = "unity_user_" + UnityEngine.Random.Range(1000, 9999);
            productIdInput.text = "unity_product_" + UnityEngine.Random.Range(100, 999);
        }

        private async void CreateLicense()
        {
            try
            {
                LogMessage("Creating license...");
                UpdateStatus("Creating License...");

                var userId = userIdInput.text;
                var productId = productIdInput.text;

                var metadata = new Dictionary<string, object>
                {
                    {"platform", "Unity"},
                    {"version", Application.version},
                    {"device", Utils.GetDeviceId()},
                    {"features", new string[] {"premium", "api_access", "support"}},
                    {"maxUsers", 50}
                };

                var license = await _licenseManager.CreateLicenseAsync(userId, productId, metadata);
                
                licenseKeyInput.text = license.LicenseKey;
                LogMessage($"License created successfully: {license.LicenseKey}");
                UpdateStatus("License Created Successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating license: {ex.Message}");
                UpdateStatus("Error Creating License");
            }
        }

        private async void ValidateLicense()
        {
            try
            {
                var licenseKey = licenseKeyInput.text;
                if (string.IsNullOrEmpty(licenseKey))
                {
                    LogMessage("Please enter a license key");
                    return;
                }

                LogMessage("Validating license...");
                UpdateStatus("Validating License...");

                var isValid = await _licenseManager.ValidateLicenseAsync(licenseKey);
                
                if (isValid)
                {
                    LogMessage("License is valid!");
                    UpdateStatus("License Valid");
                }
                else
                {
                    LogMessage("License is invalid or expired");
                    UpdateStatus("License Invalid");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error validating license: {ex.Message}");
                UpdateStatus("Error Validating License");
            }
        }

        private async void CreateUser()
        {
            try
            {
                LogMessage("Creating user...");
                UpdateStatus("Creating User...");

                var userRegistration = new UserRegistration
                {
                    Username = "unity_user_" + UnityEngine.Random.Range(1000, 9999),
                    Email = "unity@example.com",
                    Password = "secure_password_123",
                    Metadata = new Dictionary<string, object>
                    {
                        {"platform", "Unity"},
                        {"version", Application.version},
                        {"device", Utils.GetDeviceId()},
                        {"company", "Unity Game Studio"}
                    }
                };

                var user = await _licenseManager.CreateUserAsync(userRegistration);
                
                LogMessage($"User created successfully: {user.Username}");
                UpdateStatus("User Created Successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating user: {ex.Message}");
                UpdateStatus("Error Creating User");
            }
        }

        private void TestWebhook()
        {
            try
            {
                LogMessage("Testing webhook handling...");
                UpdateStatus("Testing Webhook...");

                // Simulate webhook events
                var webhookEvents = new List<WebhookEvent>
                {
                    new WebhookEvent
                    {
                        type = "license.created",
                        data = new { licenseKey = "TEST123", userId = "test_user" },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    },
                    new WebhookEvent
                    {
                        type = "user.registered",
                        data = new { username = "test_user", email = "test@example.com" },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                };

                foreach (var webhookEvent in webhookEvents)
                {
                    var payload = Utils.ToJson(webhookEvent.data);
                    var signature = "simulated_signature"; // In real usage, this would come from the webhook
                    
                    var isValidSignature = _webhookHandler.VerifySignature(payload, signature);
                    LogMessage($"Webhook {webhookEvent.type}: {(isValidSignature ? "Valid" : "Invalid")} signature");
                    
                    _webhookHandler.ProcessEvent(webhookEvent.type, webhookEvent.data);
                }

                LogMessage("Webhook testing completed");
                UpdateStatus("Webhook Test Completed");
            }
            catch (Exception ex)
            {
                LogMessage($"Error testing webhook: {ex.Message}");
                UpdateStatus("Error Testing Webhook");
            }
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";
            _logMessages.Add(logMessage);
            
            // Keep only last 50 messages
            if (_logMessages.Count > 50)
            {
                _logMessages.RemoveAt(0);
            }

            // Update UI
            if (logText != null)
            {
                logText.text = string.Join("\n", _logMessages);
            }

            // Also log to console
            Logger.Info(message, this);
        }

        private void UpdateStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            _licenseManager?.Dispose();
        }

        // Additional utility methods for demonstration
        private void Update()
        {
            // Rotate log file if it gets too large
            if (Time.frameCount % 1000 == 0) // Check every 1000 frames
            {
                Logger.RotateLogFile();
            }
        }

        // Public methods for external access
        public void ClearLogs()
        {
            _logMessages.Clear();
            if (logText != null)
            {
                logText.text = "";
            }
            Logger.ClearLogFile();
            LogMessage("Logs cleared");
        }

        public void ExportLogs()
        {
            var logFilePath = Logger.GetLogFilePath();
            LogMessage($"Logs exported to: {logFilePath}");
        }

        public void SetLogLevel(int level)
        {
            Logger.SetLogLevel((Logger.LogLevel)level);
            LogMessage($"Log level set to: {(Logger.LogLevel)level}");
        }
    }
}
