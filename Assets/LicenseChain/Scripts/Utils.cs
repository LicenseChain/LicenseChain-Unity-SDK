using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Utility functions for the LicenseChain SDK
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Validates if a string is a valid email address
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <returns>True if valid email, False otherwise</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a string is a valid license key format
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <returns>True if valid format, False otherwise</returns>
        public static bool IsValidLicenseKey(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey))
                return false;

            // License key should be 32 characters long and contain only alphanumeric characters
            if (licenseKey.Length != 32)
                return false;

            var keyRegex = new Regex(@"^[a-zA-Z0-9]+$");
            return keyRegex.IsMatch(licenseKey);
        }

        /// <summary>
        /// Generates a random license key
        /// </summary>
        /// <returns>Random 32-character license key</returns>
        public static string GenerateLicenseKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            var result = new StringBuilder(32);

            for (int i = 0; i < 32; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates MD5 hash of a string
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>MD5 hash as hexadecimal string</returns>
        public static string GenerateMD5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Generates SHA256 hash of a string
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>SHA256 hash as hexadecimal string</returns>
        public static string GenerateSHA256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Sanitizes input string to prevent injection attacks
        /// </summary>
        /// <param name="input">Input string to sanitize</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("'", "''")
                       .Replace("\"", "\"\"")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;");
        }

        /// <summary>
        /// Formats a date to ISO 8601 format
        /// </summary>
        /// <param name="date">Date to format</param>
        /// <returns>ISO 8601 formatted date string</returns>
        public static string FormatISODate(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses ISO 8601 date string to DateTime
        /// </summary>
        /// <param name="isoDate">ISO 8601 date string</param>
        /// <returns>Parsed DateTime or null if invalid</returns>
        public static DateTime? ParseISODate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate))
                return null;

            try
            {
                return DateTime.ParseExact(isoDate, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return DateTime.Parse(isoDate);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>JSON string representation</returns>
        public static string ToJson(object obj)
        {
            if (obj == null)
                return "null";

            try
            {
                return JsonUtility.ToJson(obj);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize object to JSON: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Converts JSON string to object
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Deserialized object or default value if invalid</returns>
        public static T FromJson<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize JSON to {typeof(T).Name}: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Sleeps for specified milliseconds
        /// </summary>
        /// <param name="milliseconds">Milliseconds to sleep</param>
        public static void Sleep(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Sleeps asynchronously for specified milliseconds
        /// </summary>
        /// <param name="milliseconds">Milliseconds to sleep</param>
        /// <returns>Task</returns>
        public static async System.Threading.Tasks.Task SleepAsync(int milliseconds)
        {
            await System.Threading.Tasks.Task.Delay(milliseconds);
        }

        /// <summary>
        /// Retries an operation with exponential backoff
        /// </summary>
        /// <param name="operation">Operation to retry</param>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="baseDelay">Base delay in milliseconds</param>
        /// <returns>Task</returns>
        public static async System.Threading.Tasks.Task RetryAsync(
            Func<System.Threading.Tasks.Task> operation, 
            int maxRetries, 
            int baseDelay)
        {
            int retryCount = 0;
            int delay = baseDelay;

            while (retryCount < maxRetries)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw;
                    }

                    Debug.LogWarning($"Operation failed, retrying in {delay}ms (attempt {retryCount}/{maxRetries}): {ex.Message}");
                    await SleepAsync(delay);
                    delay *= 2; // Exponential backoff
                }
            }
        }

        /// <summary>
        /// Gets the device's unique identifier
        /// </summary>
        /// <returns>Device unique identifier</returns>
        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// Gets the device's platform
        /// </summary>
        /// <returns>Device platform</returns>
        public static string GetPlatform()
        {
            return Application.platform.ToString();
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        /// <returns>Application version</returns>
        public static string GetAppVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// Checks if the application is running in editor
        /// </summary>
        /// <returns>True if running in editor</returns>
        public static bool IsEditor()
        {
            return Application.isEditor;
        }

        /// <summary>
        /// Checks if the application is running in debug mode
        /// </summary>
        /// <returns>True if running in debug mode</returns>
        public static bool IsDebugBuild()
        {
            return Debug.isDebugBuild;
        }
    }
}
