using UnityEngine;
using UnityEditor;
using System.IO;

namespace LicenseChain.Unity.Editor
{
    /// <summary>
    /// Custom editor for LicenseChain configuration
    /// </summary>
    [CustomEditor(typeof(LicenseChainConfig))]
    public class LicenseChainConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _apiKey;
        private SerializedProperty _baseUrl;
        private SerializedProperty _timeout;
        private SerializedProperty _retries;
        private SerializedProperty _enableLogging;
        private SerializedProperty _logLevel;
        private SerializedProperty _enableFileLogging;
        private SerializedProperty _logFilePath;
        private SerializedProperty _validateSSL;
        private SerializedProperty _userAgent;
        private SerializedProperty _enableCaching;
        private SerializedProperty _cacheDuration;
        private SerializedProperty _maxConcurrentRequests;

        private void OnEnable()
        {
            _apiKey = serializedObject.FindProperty("ApiKey");
            _baseUrl = serializedObject.FindProperty("BaseUrl");
            _timeout = serializedObject.FindProperty("Timeout");
            _retries = serializedObject.FindProperty("Retries");
            _enableLogging = serializedObject.FindProperty("EnableLogging");
            _logLevel = serializedObject.FindProperty("LogLevel");
            _enableFileLogging = serializedObject.FindProperty("EnableFileLogging");
            _logFilePath = serializedObject.FindProperty("LogFilePath");
            _validateSSL = serializedObject.FindProperty("ValidateSSL");
            _userAgent = serializedObject.FindProperty("UserAgent");
            _enableCaching = serializedObject.FindProperty("EnableCaching");
            _cacheDuration = serializedObject.FindProperty("CacheDuration");
            _maxConcurrentRequests = serializedObject.FindProperty("MaxConcurrentRequests");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("LicenseChain SDK Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // API Configuration
            EditorGUILayout.LabelField("API Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_apiKey, new GUIContent("API Key", "Your LicenseChain API key"));
            EditorGUILayout.PropertyField(_baseUrl, new GUIContent("Base URL", "Base URL for the LicenseChain API"));
            EditorGUILayout.PropertyField(_timeout, new GUIContent("Timeout (ms)", "Request timeout in milliseconds"));
            EditorGUILayout.PropertyField(_retries, new GUIContent("Retries", "Number of retry attempts for failed requests"));

            EditorGUILayout.Space();

            // Logging Configuration
            EditorGUILayout.LabelField("Logging Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableLogging, new GUIContent("Enable Logging", "Enable debug logging"));
            
            if (_enableLogging.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_logLevel, new GUIContent("Log Level", "0=Debug, 1=Info, 2=Warning, 3=Error, 4=Fatal"));
                EditorGUILayout.PropertyField(_enableFileLogging, new GUIContent("Enable File Logging", "Enable logging to file"));
                
                if (_enableFileLogging.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_logFilePath, new GUIContent("Log File Path", "Custom log file path (leave empty for default)"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Security Configuration
            EditorGUILayout.LabelField("Security Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_validateSSL, new GUIContent("Validate SSL", "Enable SSL certificate validation"));
            EditorGUILayout.PropertyField(_userAgent, new GUIContent("User Agent", "Custom user agent string"));

            EditorGUILayout.Space();

            // Performance Configuration
            EditorGUILayout.LabelField("Performance Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableCaching, new GUIContent("Enable Caching", "Enable request caching"));
            
            if (_enableCaching.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_cacheDuration, new GUIContent("Cache Duration (s)", "Cache duration in seconds"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.PropertyField(_maxConcurrentRequests, new GUIContent("Max Concurrent Requests", "Maximum concurrent requests"));

            EditorGUILayout.Space();

            // Validation and Actions
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Validate Configuration"))
            {
                var config = (LicenseChainConfig)target;
                if (config.IsValid())
                {
                    EditorUtility.DisplayDialog("Configuration", "Configuration is valid!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Configuration", "Configuration has errors. Check the console for details.", "OK");
                }
            }

            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Configuration", "Are you sure you want to reset to default values?", "Yes", "No"))
                {
                    var config = LicenseChainConfig.CreateDefault();
                    EditorUtility.CopySerialized(config, target);
                }
            }

            if (GUILayout.Button("Save to PlayerPrefs"))
            {
                var config = (LicenseChainConfig)target;
                config.SaveToPlayerPrefs();
                EditorUtility.DisplayDialog("Configuration", "Configuration saved to PlayerPrefs!", "OK");
            }

            if (GUILayout.Button("Load from PlayerPrefs"))
            {
                var config = LicenseChainConfig.LoadFromPlayerPrefs();
                EditorUtility.CopySerialized(config, target);
                EditorUtility.DisplayDialog("Configuration", "Configuration loaded from PlayerPrefs!", "OK");
            }

            EditorGUILayout.Space();

            // Help
            EditorGUILayout.LabelField("Help", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "For more information about the LicenseChain SDK, visit our documentation or contact support.",
                MessageType.Info
            );

            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Menu items for LicenseChain SDK
    /// </summary>
    public static class LicenseChainMenu
    {
        [MenuItem("LicenseChain/Create Configuration")]
        public static void CreateConfiguration()
        {
            var config = ScriptableObject.CreateInstance<LicenseChainConfig>();
            config.ApiKey = "";
            config.BaseUrl = "https://api.licensechain.com";
            config.Timeout = 30000;
            config.Retries = 3;
            config.EnableLogging = true;
            config.LogLevel = 1;
            config.EnableFileLogging = false;
            config.LogFilePath = "";
            config.ValidateSSL = true;
            config.UserAgent = "LicenseChain-Unity-SDK/1.0.0";
            config.EnableCaching = true;
            config.CacheDuration = 300;
            config.MaxConcurrentRequests = 10;

            string path = EditorUtility.SaveFilePanelInProject(
                "Create LicenseChain Configuration",
                "LicenseChainConfig",
                "asset",
                "Choose where to save the configuration"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
            }
        }

        [MenuItem("LicenseChain/Open Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://docs.licensechain.com");
        }

        [MenuItem("LicenseChain/Open Support")]
        public static void OpenSupport()
        {
            Application.OpenURL("https://support.licensechain.com");
        }

        [MenuItem("LicenseChain/Clear Logs")]
        public static void ClearLogs()
        {
            if (EditorUtility.DisplayDialog("Clear Logs", "Are you sure you want to clear all LicenseChain logs?", "Yes", "No"))
            {
                Logger.ClearLogFile();
                Debug.Log("LicenseChain logs cleared");
            }
        }
    }
}
