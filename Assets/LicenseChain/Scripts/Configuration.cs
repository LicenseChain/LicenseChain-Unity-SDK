using System;
using UnityEngine;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Configuration class for LicenseChain SDK
    /// </summary>
    [Serializable]
    public class LicenseChainConfig
    {
        [Header("API Configuration")]
        [Tooltip("Your LicenseChain API key")]
        public string ApiKey = "";

        [Tooltip("Base URL for the LicenseChain API")]
        public string BaseUrl = "https://api.licensechain.com";

        [Tooltip("Request timeout in milliseconds")]
        public int Timeout = 30000;

        [Tooltip("Number of retry attempts for failed requests")]
        public int Retries = 3;

        [Header("Logging Configuration")]
        [Tooltip("Enable debug logging")]
        public bool EnableLogging = true;

        [Tooltip("Log level (0=Debug, 1=Info, 2=Warning, 3=Error, 4=Fatal)")]
        public int LogLevel = 1;

        [Tooltip("Enable file logging")]
        public bool EnableFileLogging = false;

        [Tooltip("Custom log file path (leave empty for default)")]
        public string LogFilePath = "";

        [Header("Security Configuration")]
        [Tooltip("Enable SSL certificate validation")]
        public bool ValidateSSL = true;

        [Tooltip("Custom user agent string")]
        public string UserAgent = "LicenseChain-Unity-SDK/1.0.0";

        [Header("Performance Configuration")]
        [Tooltip("Enable request caching")]
        public bool EnableCaching = true;

        [Tooltip("Cache duration in seconds")]
        public int CacheDuration = 300;

        [Tooltip("Maximum concurrent requests")]
        public int MaxConcurrentRequests = 10;

        /// <summary>
        /// Validates the configuration
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                Debug.LogError("LicenseChain: API Key is required");
                return false;
            }

            if (string.IsNullOrEmpty(BaseUrl))
            {
                Debug.LogError("LicenseChain: Base URL is required");
                return false;
            }

            if (Timeout <= 0)
            {
                Debug.LogError("LicenseChain: Timeout must be greater than 0");
                return false;
            }

            if (Retries < 0)
            {
                Debug.LogError("LicenseChain: Retries must be 0 or greater");
                return false;
            }

            if (LogLevel < 0 || LogLevel > 4)
            {
                Debug.LogError("LicenseChain: Log level must be between 0 and 4");
                return false;
            }

            if (CacheDuration < 0)
            {
                Debug.LogError("LicenseChain: Cache duration must be 0 or greater");
                return false;
            }

            if (MaxConcurrentRequests <= 0)
            {
                Debug.LogError("LicenseChain: Max concurrent requests must be greater than 0");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the log level as enum
        /// </summary>
        /// <returns>Log level enum</returns>
        public Logger.LogLevel GetLogLevel()
        {
            return (Logger.LogLevel)Mathf.Clamp(LogLevel, 0, 4);
        }

        /// <summary>
        /// Creates a default configuration
        /// </summary>
        /// <returns>Default configuration</returns>
        public static LicenseChainConfig CreateDefault()
        {
            return new LicenseChainConfig
            {
                ApiKey = "",
                BaseUrl = "https://api.licensechain.com",
                Timeout = 30000,
                Retries = 3,
                EnableLogging = true,
                LogLevel = 1,
                EnableFileLogging = false,
                LogFilePath = "",
                ValidateSSL = true,
                UserAgent = "LicenseChain-Unity-SDK/1.0.0",
                EnableCaching = true,
                CacheDuration = 300,
                MaxConcurrentRequests = 10
            };
        }

        /// <summary>
        /// Creates a configuration from JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Configuration object</returns>
        public static LicenseChainConfig FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<LicenseChainConfig>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse configuration from JSON: {ex.Message}");
                return CreateDefault();
            }
        }

        /// <summary>
        /// Converts configuration to JSON
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            try
            {
                return JsonUtility.ToJson(this, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize configuration to JSON: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Loads configuration from PlayerPrefs
        /// </summary>
        /// <param name="key">PlayerPrefs key</param>
        /// <returns>Configuration object</returns>
        public static LicenseChainConfig LoadFromPlayerPrefs(string key = "LicenseChainConfig")
        {
            try
            {
                string json = PlayerPrefs.GetString(key, "");
                if (string.IsNullOrEmpty(json))
                {
                    return CreateDefault();
                }
                return FromJson(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load configuration from PlayerPrefs: {ex.Message}");
                return CreateDefault();
            }
        }

        /// <summary>
        /// Saves configuration to PlayerPrefs
        /// </summary>
        /// <param name="key">PlayerPrefs key</param>
        public void SaveToPlayerPrefs(string key = "LicenseChainConfig")
        {
            try
            {
                string json = ToJson();
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save configuration to PlayerPrefs: {ex.Message}");
            }
        }
    }
}
