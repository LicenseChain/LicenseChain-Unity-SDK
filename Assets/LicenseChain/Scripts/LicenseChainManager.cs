using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace LicenseChain
{
    /// <summary>
    /// Main LicenseChain manager for Unity applications
    /// Reverse-engineered from advanced Unity patterns with enhanced functionality
    /// </summary>
    public class LicenseChainManager : MonoBehaviour
    {
        [Header("LicenseChain Configuration")]
        [SerializeField] private string appName = "";
        [SerializeField] private string ownerId = "";
        [SerializeField] private string appSecret = "";
        [SerializeField] private string baseUrl = "https://api.licensechain.app";
        [SerializeField] private float timeout = 30f;
        [SerializeField] private int retries = 3;

        [Header("UI References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private UnityEngine.UI.InputField usernameField;
        [SerializeField] private UnityEngine.UI.InputField passwordField;
        [SerializeField] private UnityEngine.UI.InputField licenseField;
        [SerializeField] private UnityEngine.UI.Button loginButton;
        [SerializeField] private UnityEngine.UI.Button licenseLoginButton;
        [SerializeField] private UnityEngine.UI.Text statusText;
        [SerializeField] private UnityEngine.UI.Text userInfoText;

        // Events
        public static event Action<bool> OnLoginStatusChanged;
        public static event Action<string> OnStatusMessage;
        public static event Action<LicenseChainUser> OnUserDataReceived;

        // Private fields
        private string sessionId;
        private LicenseChainUser userData;
        private bool isInitialized = false;
        private bool isLoggedIn = false;
        private Coroutine currentRequest;

        // Properties
        public bool IsInitialized => isInitialized;
        public bool IsLoggedIn => isLoggedIn;
        public LicenseChainUser UserData => userData;
        public string SessionId => sessionId;

        private void Start()
        {
            InitializeLicenseChain();
            SetupUI();
        }

        /// <summary>
        /// Initialize the LicenseChain client
        /// </summary>
        public void InitializeLicenseChain()
        {
            if (string.IsNullOrEmpty(appName) || string.IsNullOrEmpty(ownerId) || string.IsNullOrEmpty(appSecret))
            {
                Debug.LogError("LicenseChain: App name, owner ID, and app secret must be set!");
                OnStatusMessage?.Invoke("Configuration error: Missing required fields");
                return;
            }

            StartCoroutine(InitializeCoroutine());
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        public void Login()
        {
            if (string.IsNullOrEmpty(usernameField.text) || string.IsNullOrEmpty(passwordField.text))
            {
                OnStatusMessage?.Invoke("Please enter username and password");
                return;
            }

            StartCoroutine(LoginCoroutine(usernameField.text, passwordField.text));
        }

        /// <summary>
        /// Login with license key only
        /// </summary>
        public void LicenseLogin()
        {
            if (string.IsNullOrEmpty(licenseField.text))
            {
                OnStatusMessage?.Invoke("Please enter a license key");
                return;
            }

            StartCoroutine(LicenseLoginCoroutine(licenseField.text));
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public void Logout()
        {
            StartCoroutine(LogoutCoroutine());
        }

        /// <summary>
        /// Set user variable
        /// </summary>
        public void SetVariable(string var, string data)
        {
            if (!isLoggedIn) return;
            StartCoroutine(SetVariableCoroutine(var, data));
        }

        /// <summary>
        /// Get user variable
        /// </summary>
        public void GetVariable(string var, System.Action<string> callback)
        {
            if (!isLoggedIn) return;
            StartCoroutine(GetVariableCoroutine(var, callback));
        }

        /// <summary>
        /// Log message to LicenseChain
        /// </summary>
        public void LogMessage(string message)
        {
            if (!isLoggedIn) return;
            StartCoroutine(LogMessageCoroutine(message));
        }

        /// <summary>
        /// Download file from LicenseChain
        /// </summary>
        public void DownloadFile(string fileId, System.Action<string> callback)
        {
            if (!isLoggedIn) return;
            StartCoroutine(DownloadFileCoroutine(fileId, callback));
        }

        /// <summary>
        /// Get application statistics
        /// </summary>
        public void GetAppStats(System.Action<LicenseChainAppStats> callback)
        {
            if (!isInitialized) return;
            StartCoroutine(GetAppStatsCoroutine(callback));
        }

        /// <summary>
        /// Get online users
        /// </summary>
        public void GetOnlineUsers(System.Action<List<LicenseChainUser>> callback)
        {
            if (!isLoggedIn) return;
            StartCoroutine(GetOnlineUsersCoroutine(callback));
        }

        /// <summary>
        /// Send chat message
        /// </summary>
        public void SendChatMessage(string message, string channel = "general")
        {
            if (!isLoggedIn) return;
            StartCoroutine(SendChatMessageCoroutine(message, channel));
        }

        /// <summary>
        /// Get chat messages
        /// </summary>
        public void GetChatMessages(string channel = "general", System.Action<List<LicenseChainChatMessage>> callback = null)
        {
            if (!isLoggedIn) return;
            StartCoroutine(GetChatMessagesCoroutine(channel, callback));
        }

        private void SetupUI()
        {
            if (loginButton != null)
                loginButton.onClick.AddListener(Login);

            if (licenseLoginButton != null)
                licenseLoginButton.onClick.AddListener(LicenseLogin);
        }

        private IEnumerator InitializeCoroutine()
        {
            var requestData = new
            {
                type = "init",
                ver = "1.0",
                hash = GenerateHash(),
                enckey = GenerateEncryptionKey(),
                name = appName,
                ownerid = ownerId
            };

            yield return StartCoroutine(MakeRequest("init", requestData, (response) =>
            {
                if (response.success)
                {
                    sessionId = response.sessionid;
                    isInitialized = true;
                    OnStatusMessage?.Invoke("LicenseChain initialized successfully");
                    Debug.Log("LicenseChain: Initialized successfully");
                }
                else
                {
                    OnStatusMessage?.Invoke($"Initialization failed: {response.message}");
                    Debug.LogError($"LicenseChain: Initialization failed - {response.message}");
                }
            }));
        }

        private IEnumerator LoginCoroutine(string username, string password)
        {
            var requestData = new
            {
                type = "login",
                username = username,
                pass = password,
                hwid = GetHardwareId()
            };

            yield return StartCoroutine(MakeRequest("login", requestData, (response) =>
            {
                if (response.success)
                {
                    userData = JsonConvert.DeserializeObject<LicenseChainUser>(JsonConvert.SerializeObject(response.info));
                    isLoggedIn = true;
                    OnLoginStatusChanged?.Invoke(true);
                    OnUserDataReceived?.Invoke(userData);
                    OnStatusMessage?.Invoke("Login successful");
                    UpdateUI();
                    Debug.Log("LicenseChain: Login successful");
                }
                else
                {
                    OnStatusMessage?.Invoke($"Login failed: {response.message}");
                    Debug.LogError($"LicenseChain: Login failed - {response.message}");
                }
            }));
        }

        private IEnumerator LicenseLoginCoroutine(string license)
        {
            var requestData = new
            {
                type = "license",
                key = license,
                hwid = GetHardwareId()
            };

            yield return StartCoroutine(MakeRequest("license", requestData, (response) =>
            {
                if (response.success)
                {
                    userData = JsonConvert.DeserializeObject<LicenseChainUser>(JsonConvert.SerializeObject(response.info));
                    isLoggedIn = true;
                    OnLoginStatusChanged?.Invoke(true);
                    OnUserDataReceived?.Invoke(userData);
                    OnStatusMessage?.Invoke("License login successful");
                    UpdateUI();
                    Debug.Log("LicenseChain: License login successful");
                }
                else
                {
                    OnStatusMessage?.Invoke($"License login failed: {response.message}");
                    Debug.LogError($"LicenseChain: License login failed - {response.message}");
                }
            }));
        }

        private IEnumerator LogoutCoroutine()
        {
            var requestData = new
            {
                type = "logout",
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("logout", requestData, (response) =>
            {
                if (response.success)
                {
                    userData = null;
                    isLoggedIn = false;
                    OnLoginStatusChanged?.Invoke(false);
                    OnStatusMessage?.Invoke("Logged out successfully");
                    UpdateUI();
                    Debug.Log("LicenseChain: Logged out successfully");
                }
                else
                {
                    OnStatusMessage?.Invoke($"Logout failed: {response.message}");
                    Debug.LogError($"LicenseChain: Logout failed - {response.message}");
                }
            }));
        }

        private IEnumerator SetVariableCoroutine(string var, string data)
        {
            var requestData = new
            {
                type = "setvar",
                var = var,
                data = data,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("setvar", requestData, (response) =>
            {
                if (response.success)
                {
                    Debug.Log($"LicenseChain: Variable {var} set successfully");
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to set variable {var} - {response.message}");
                }
            }));
        }

        private IEnumerator GetVariableCoroutine(string var, System.Action<string> callback)
        {
            var requestData = new
            {
                type = "getvar",
                var = var,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("getvar", requestData, (response) =>
            {
                if (response.success)
                {
                    callback?.Invoke(response.data);
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to get variable {var} - {response.message}");
                    callback?.Invoke(null);
                }
            }));
        }

        private IEnumerator LogMessageCoroutine(string message)
        {
            var requestData = new
            {
                type = "log",
                pcuser = GetPCUser(),
                message = message,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("log", requestData, (response) =>
            {
                if (response.success)
                {
                    Debug.Log($"LicenseChain: Message logged successfully");
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to log message - {response.message}");
                }
            }));
        }

        private IEnumerator DownloadFileCoroutine(string fileId, System.Action<string> callback)
        {
            var requestData = new
            {
                type = "file",
                fileid = fileId,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("file", requestData, (response) =>
            {
                if (response.success)
                {
                    callback?.Invoke(response.contents);
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to download file {fileId} - {response.message}");
                    callback?.Invoke(null);
                }
            }));
        }

        private IEnumerator GetAppStatsCoroutine(System.Action<LicenseChainAppStats> callback)
        {
            var requestData = new
            {
                type = "app",
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("app", requestData, (response) =>
            {
                if (response.success)
                {
                    var stats = JsonConvert.DeserializeObject<LicenseChainAppStats>(JsonConvert.SerializeObject(response));
                    callback?.Invoke(stats);
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to get app stats - {response.message}");
                    callback?.Invoke(null);
                }
            }));
        }

        private IEnumerator GetOnlineUsersCoroutine(System.Action<List<LicenseChainUser>> callback)
        {
            var requestData = new
            {
                type = "online",
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("online", requestData, (response) =>
            {
                if (response.success)
                {
                    var users = JsonConvert.DeserializeObject<List<LicenseChainUser>>(JsonConvert.SerializeObject(response.users));
                    callback?.Invoke(users);
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to get online users - {response.message}");
                    callback?.Invoke(null);
                }
            }));
        }

        private IEnumerator SendChatMessageCoroutine(string message, string channel)
        {
            var requestData = new
            {
                type = "chatsend",
                message = message,
                channel = channel,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("chatsend", requestData, (response) =>
            {
                if (response.success)
                {
                    Debug.Log($"LicenseChain: Chat message sent successfully");
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to send chat message - {response.message}");
                }
            }));
        }

        private IEnumerator GetChatMessagesCoroutine(string channel, System.Action<List<LicenseChainChatMessage>> callback)
        {
            var requestData = new
            {
                type = "chatget",
                channel = channel,
                sessionid = sessionId
            };

            yield return StartCoroutine(MakeRequest("chatget", requestData, (response) =>
            {
                if (response.success)
                {
                    var messages = JsonConvert.DeserializeObject<List<LicenseChainChatMessage>>(JsonConvert.SerializeObject(response.messages));
                    callback?.Invoke(messages);
                }
                else
                {
                    Debug.LogError($"LicenseChain: Failed to get chat messages - {response.message}");
                    callback?.Invoke(null);
                }
            }));
        }

        private IEnumerator MakeRequest(string endpoint, object data, System.Action<LicenseChainResponse> callback)
        {
            string url = $"{baseUrl}/{endpoint}";
            string jsonData = JsonConvert.SerializeObject(data);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("User-Agent", "LicenseChain-Unity-SDK/1.0.0");
                request.timeout = (int)timeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<LicenseChainResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"LicenseChain: Failed to parse response - {e.Message}");
                        callback?.Invoke(new LicenseChainResponse { success = false, message = "Failed to parse response" });
                    }
                }
                else
                {
                    Debug.LogError($"LicenseChain: Request failed - {request.error}");
                    callback?.Invoke(new LicenseChainResponse { success = false, message = request.error });
                }
            }
        }

        private void UpdateUI()
        {
            if (loginPanel != null)
                loginPanel.SetActive(!isLoggedIn);

            if (mainPanel != null)
                mainPanel.SetActive(isLoggedIn);

            if (userInfoText != null && isLoggedIn && userData != null)
            {
                userInfoText.text = $"Welcome, {userData.username}!\n" +
                                  $"Email: {userData.email}\n" +
                                  $"License: {userData.license}\n" +
                                  $"Expires: {userData.expiry}";
            }
        }

        private string GenerateHash()
        {
            string data = $"{appName}{ownerId}{appSecret}";
            return HashString(data);
        }

        private string GenerateEncryptionKey()
        {
            return System.Guid.NewGuid().ToString("N").Substring(0, 32);
        }

        private string GetHardwareId()
        {
            try
            {
                return SystemInfo.deviceUniqueIdentifier;
            }
            catch
            {
                return "unknown-hwid-" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            }
        }

        private string GetPCUser()
        {
            return Environment.UserName ?? "unknown";
        }

        private string HashString(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
