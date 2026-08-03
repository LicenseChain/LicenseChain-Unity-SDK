using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Advanced API v1 example for Unity.
    /// </summary>
    public class AdvancedLicenseChainExample : MonoBehaviour
    {
        [Header("Connection")]
        public string apiKey = "";
        public string appId = "";
        public string baseUrl = "https://api.licensechain.app/v1";

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

        private LicenseChainApiV1Client _client;
        private WebhookHandler _webhookHandler;
        private readonly List<string> _logMessages = new List<string>();

        private void Start()
        {
            var config = new LicenseChainConfig
            {
                ApiKey = apiKey,
                BaseUrl = baseUrl,
                Timeout = 30000,
                Retries = 3,
                EnableLogging = true
            };

            _client = new LicenseChainApiV1Client(config);
            _webhookHandler = new WebhookHandler("webhook-secret");

            if (createLicenseButton != null) createLicenseButton.onClick.AddListener(CreateLicense);
            if (validateLicenseButton != null) validateLicenseButton.onClick.AddListener(ValidateLicense);
            if (createUserButton != null) createUserButton.onClick.AddListener(CreateUser);
            if (webhookTestButton != null) webhookTestButton.onClick.AddListener(TestWebhook);

            LogMessage("Advanced API v1 example initialized");
        }

        private async void CreateLicense()
        {
            try
            {
                UpdateStatus("Creating license...");
                var email = string.IsNullOrWhiteSpace(userIdInput?.text) ? "unity@example.com" : userIdInput.text;
                var result = await _client.CreateLicenseAsync(appId, email, "Unity User");
                var key = result["licenseKey"]?.ToString() ?? result["key"]?.ToString() ?? string.Empty;
                if (licenseKeyInput != null) licenseKeyInput.text = key;
                LogMessage("License created");
                UpdateStatus("License created");
            }
            catch (Exception ex)
            {
                LogMessage($"Create license failed: {ex.Message}");
                UpdateStatus("Create license failed");
            }
        }

        private async void ValidateLicense()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseKeyInput?.text))
                {
                    LogMessage("Enter a license key first");
                    return;
                }

                UpdateStatus("Validating license...");
                var result = await _client.ValidateLicenseAsync(licenseKeyInput.text, appId);
                var valid = result["valid"]?.Value<bool>() ?? false;
                LogMessage(valid ? "License is valid" : "License is invalid");
                UpdateStatus(valid ? "License valid" : "License invalid");
            }
            catch (Exception ex)
            {
                LogMessage($"Validate failed: {ex.Message}");
                UpdateStatus("Validate failed");
            }
        }

        private async void CreateUser()
        {
            try
            {
                UpdateStatus("Registering user...");
                var email = string.IsNullOrWhiteSpace(userIdInput?.text) ? "unity@example.com" : userIdInput.text;
                JObject result = await _client.RegisterUserAsync(email, "unity-default-password", "Unity User");
                LogMessage($"User register response: {result.ToString()}");
                UpdateStatus("User registration done");
            }
            catch (Exception ex)
            {
                LogMessage($"Create user failed: {ex.Message}");
                UpdateStatus("Create user failed");
            }
        }

        private void TestWebhook()
        {
            var payload = "{\"event\":\"license.created\"}";
            var signature = "invalid-signature";
            var verified = _webhookHandler.VerifySignature(payload, signature);
            LogMessage($"Webhook verification result: {verified}");
            UpdateStatus("Webhook test done");
        }

        private void LogMessage(string message)
        {
            _logMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (_logMessages.Count > 50) _logMessages.RemoveAt(0);
            if (logText != null) logText.text = string.Join("\n", _logMessages);
            Debug.Log(message);
        }

        private void UpdateStatus(string status)
        {
            if (statusText != null) statusText.text = status;
        }

        private void OnDestroy()
        {
            _client?.Dispose();
        }
    }
}
