using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Handles webhook verification and processing
    /// </summary>
    public class WebhookHandler
    {
        private readonly string _secretKey;

        public WebhookHandler(string secretKey)
        {
            _secretKey = secretKey;
        }

        /// <summary>
        /// Verifies webhook signature
        /// </summary>
        /// <param name="payload">Webhook payload</param>
        /// <param name="signature">Webhook signature</param>
        /// <returns>True if signature is valid</returns>
        public bool VerifySignature(string payload, string signature)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature))
                return false;

            try
            {
                string expectedSignature = GenerateSignature(payload);
                return string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to verify webhook signature: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generates signature for webhook payload
        /// </summary>
        /// <param name="payload">Payload to sign</param>
        /// <returns>Generated signature</returns>
        private string GenerateSignature(string payload)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] hashBytes = hmac.ComputeHash(payloadBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        /// <summary>
        /// Processes webhook event
        /// </summary>
        /// <param name="eventType">Type of webhook event</param>
        /// <param name="data">Event data</param>
        public void ProcessEvent(string eventType, object data)
        {
            try
            {
                switch (eventType.ToLower())
                {
                    case "license.created":
                        HandleLicenseCreated(data);
                        break;
                    case "license.updated":
                        HandleLicenseUpdated(data);
                        break;
                    case "license.revoked":
                        HandleLicenseRevoked(data);
                        break;
                    case "license.expired":
                        HandleLicenseExpired(data);
                        break;
                    case "user.registered":
                        HandleUserRegistered(data);
                        break;
                    case "user.updated":
                        HandleUserUpdated(data);
                        break;
                    default:
                        Debug.LogWarning($"Unknown webhook event type: {eventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing webhook event {eventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles license created event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleLicenseCreated(object data)
        {
            Debug.Log("License created event received");
            // Add your custom logic here
            // You can dispatch events, update UI, etc.
        }

        /// <summary>
        /// Handles license updated event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleLicenseUpdated(object data)
        {
            Debug.Log("License updated event received");
            // Add your custom logic here
        }

        /// <summary>
        /// Handles license revoked event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleLicenseRevoked(object data)
        {
            Debug.Log("License revoked event received");
            // Add your custom logic here
        }

        /// <summary>
        /// Handles license expired event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleLicenseExpired(object data)
        {
            Debug.Log("License expired event received");
            // Add your custom logic here
        }

        /// <summary>
        /// Handles user registered event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleUserRegistered(object data)
        {
            Debug.Log("User registered event received");
            // Add your custom logic here
        }

        /// <summary>
        /// Handles user updated event
        /// </summary>
        /// <param name="data">Event data</param>
        private void HandleUserUpdated(object data)
        {
            Debug.Log("User updated event received");
            // Add your custom logic here
        }
    }

    /// <summary>
    /// Webhook event data structure
    /// </summary>
    [Serializable]
    public class WebhookEvent
    {
        public string type;
        public object data;
        public long timestamp;
    }
}
