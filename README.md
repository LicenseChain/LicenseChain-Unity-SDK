# LicenseChain Unity SDK

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3+-blue.svg)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-8.0+-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Package Manager](https://img.shields.io/badge/Package%20Manager-1.0+-green.svg)](https://docs.unity3d.com/Manual/Packages.html)

Official Unity SDK for LicenseChain - Secure license management for Unity games and applications.

## 🚀 Features

- **🔐 Secure Authentication** - User registration, login, and session management
- **📜 License Management** - Create, validate, update, and revoke licenses
- **🛡️ Hardware ID Validation** - Prevent license sharing and unauthorized access
- **🔔 Webhook Support** - Real-time license events and notifications
- **📊 Analytics Integration** - Track license usage and performance metrics
- **⚡ High Performance** - Optimized for Unity's runtime
- **🔄 Async Operations** - Non-blocking HTTP requests and data processing
- **🛠️ Easy Integration** - Simple API with comprehensive documentation

## 📦 Installation

### Method 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager
2. Click the "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/LicenseChain/LicenseChain-Unity-SDK.git`

### Method 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/LicenseChain/LicenseChain-Unity-SDK/releases)
2. Extract the `.unitypackage` file
3. Import the package into your Unity project
4. Place the `LicenseChain` folder in your `Assets` directory

### Method 3: Git Submodule

```bash
# Add as submodule
git submodule add https://github.com/LicenseChain/LicenseChain-Unity-SDK.git Assets/LicenseChain

# Update submodule
git submodule update --init --recursive
```

## 🚀 Quick Start

### Basic Setup

```csharp
using LicenseChain.Unity;

public class LicenseManager : MonoBehaviour
{
    private LicenseChainClient client;
    
    void Start()
    {
        // Initialize the client
        var config = new LicenseChainConfig
        {
            ApiKey = "your-api-key",
            AppName = "your-app-name",
            Version = "1.0.0"
        };
        
        client = new LicenseChainClient(config);
        
        // Connect to LicenseChain
        StartCoroutine(ConnectToLicenseChain());
    }
    
    private IEnumerator ConnectToLicenseChain()
    {
        var connectTask = client.ConnectAsync();
        yield return new WaitUntil(() => connectTask.IsCompleted);
        
        if (connectTask.IsFaulted)
        {
            Debug.LogError($"Failed to connect: {connectTask.Exception}");
        }
        else
        {
            Debug.Log("Connected to LicenseChain successfully!");
        }
    }
}
```

### User Authentication

```csharp
// Register a new user
public IEnumerator RegisterUser(string username, string password, string email)
{
    var registerTask = client.RegisterAsync(username, password, email);
    yield return new WaitUntil(() => registerTask.IsCompleted);
    
    if (registerTask.IsFaulted)
    {
        Debug.LogError($"Registration failed: {registerTask.Exception}");
    }
    else
    {
        var user = registerTask.Result;
        Debug.Log($"User registered successfully! ID: {user.Id}");
    }
}

// Login existing user
public IEnumerator LoginUser(string username, string password)
{
    var loginTask = client.LoginAsync(username, password);
    yield return new WaitUntil(() => loginTask.IsCompleted);
    
    if (loginTask.IsFaulted)
    {
        Debug.LogError($"Login failed: {loginTask.Exception}");
    }
    else
    {
        var user = loginTask.Result;
        Debug.Log($"User logged in successfully! Session ID: {user.SessionId}");
    }
}
```

### License Management

```csharp
// Validate a license
public IEnumerator ValidateLicense(string licenseKey)
{
    var validateTask = client.ValidateLicenseAsync(licenseKey);
    yield return new WaitUntil(() => validateTask.IsCompleted);
    
    if (validateTask.IsFaulted)
    {
        Debug.LogError($"License validation failed: {validateTask.Exception}");
    }
    else
    {
        var license = validateTask.Result;
        Debug.Log($"License is valid! Key: {license.Key}, Status: {license.Status}");
        Debug.Log($"Expires: {license.Expires}, Features: {string.Join(", ", license.Features)}");
    }
}

// Get user's licenses
public IEnumerator GetUserLicenses()
{
    var licensesTask = client.GetUserLicensesAsync();
    yield return new WaitUntil(() => licensesTask.IsCompleted);
    
    if (licensesTask.IsFaulted)
    {
        Debug.LogError($"Failed to get licenses: {licensesTask.Exception}");
    }
    else
    {
        var licenses = licensesTask.Result;
        Debug.Log($"Found {licenses.Count} licenses:");
        for (int i = 0; i < licenses.Count; i++)
        {
            var license = licenses[i];
            Debug.Log($"  {i + 1}. {license.Key} - {license.Status} (Expires: {license.Expires})");
        }
    }
}
```

### Hardware ID Validation

```csharp
// Get hardware ID (automatically generated)
public string GetHardwareId()
{
    var hardwareId = client.GetHardwareId();
    Debug.Log($"Hardware ID: {hardwareId}");
    return hardwareId;
}

// Validate hardware ID with license
public IEnumerator ValidateHardwareId(string licenseKey, string hardwareId)
{
    var validateTask = client.ValidateHardwareIdAsync(licenseKey, hardwareId);
    yield return new WaitUntil(() => validateTask.IsCompleted);
    
    if (validateTask.IsFaulted)
    {
        Debug.LogError($"Hardware ID validation failed: {validateTask.Exception}");
    }
    else
    {
        var isValid = validateTask.Result;
        if (isValid)
        {
            Debug.Log("Hardware ID is valid for this license!");
        }
        else
        {
            Debug.Log("Hardware ID is not valid for this license.");
        }
    }
}
```

### Webhook Integration

```csharp
// Set up webhook handler
public void SetupWebhookHandler()
{
    client.SetWebhookHandler((eventName, data) =>
    {
        Debug.Log($"Webhook received: {eventName}");
        
        switch (eventName)
        {
            case "license.created":
                Debug.Log($"New license created: {data["licenseKey"]}");
                break;
            case "license.updated":
                Debug.Log($"License updated: {data["licenseKey"]}");
                break;
            case "license.revoked":
                Debug.Log($"License revoked: {data["licenseKey"]}");
                break;
        }
    });
    
    // Start webhook listener
    StartCoroutine(StartWebhookListener());
}

private IEnumerator StartWebhookListener()
{
    var startTask = client.StartWebhookListenerAsync();
    yield return new WaitUntil(() => startTask.IsCompleted);
    
    if (startTask.IsFaulted)
    {
        Debug.LogError($"Failed to start webhook listener: {startTask.Exception}");
    }
    else
    {
        Debug.Log("Webhook listener started successfully!");
    }
}
```

## 📚 API Reference

### LicenseChainClient

#### Constructor

```csharp
var config = new LicenseChainConfig
{
    ApiKey = "your-api-key",
    AppName = "your-app-name",
    Version = "1.0.0",
    BaseUrl = "https://api.licensechain.com" // Optional
};

var client = new LicenseChainClient(config);
```

#### Methods

##### Connection Management

```csharp
// Connect to LicenseChain
var connectTask = client.ConnectAsync();

// Disconnect from LicenseChain
var disconnectTask = client.DisconnectAsync();

// Check connection status
bool isConnected = client.IsConnected;
```

##### User Authentication

```csharp
// Register a new user
var registerTask = client.RegisterAsync(username, password, email);

// Login existing user
var loginTask = client.LoginAsync(username, password);

// Logout current user
var logoutTask = client.LogoutAsync();

// Get current user info
var userTask = client.GetCurrentUserAsync();
```

##### License Management

```csharp
// Validate a license
var validateTask = client.ValidateLicenseAsync(licenseKey);

// Get user's licenses
var licensesTask = client.GetUserLicensesAsync();

// Create a new license
var createTask = client.CreateLicenseAsync(userId, features, expires);

// Update a license
var updateTask = client.UpdateLicenseAsync(licenseKey, updates);

// Revoke a license
var revokeTask = client.RevokeLicenseAsync(licenseKey);

// Extend a license
var extendTask = client.ExtendLicenseAsync(licenseKey, days);
```

##### Hardware ID Management

```csharp
// Get hardware ID
string hardwareId = client.GetHardwareId();

// Validate hardware ID
var validateTask = client.ValidateHardwareIdAsync(licenseKey, hardwareId);

// Bind hardware ID to license
var bindTask = client.BindHardwareIdAsync(licenseKey, hardwareId);
```

##### Webhook Management

```csharp
// Set webhook handler
client.SetWebhookHandler(handler);

// Start webhook listener
var startTask = client.StartWebhookListenerAsync();

// Stop webhook listener
var stopTask = client.StopWebhookListenerAsync();
```

##### Analytics

```csharp
// Track event
var trackTask = client.TrackEventAsync(eventName, properties);

// Get analytics data
var analyticsTask = client.GetAnalyticsAsync(timeRange);
```

## 🔧 Configuration

### Unity Settings

Configure the SDK through Unity's Project Settings or a configuration file:

```csharp
// Assets/LicenseChain/Config/LicenseChainSettings.asset
[CreateAssetMenu(fileName = "LicenseChainSettings", menuName = "LicenseChain/Settings")]
public class LicenseChainSettings : ScriptableObject
{
    [Header("API Configuration")]
    public string apiKey;
    public string appName;
    public string version;
    public string baseUrl = "https://api.licensechain.com";
    
    [Header("Advanced Settings")]
    public int timeout = 30;
    public int retries = 3;
    public bool debug = false;
}
```

### Environment Variables

Set these in your build process or through Unity Cloud Build:

```bash
# Required
export LICENSECHAIN_API_KEY=your-api-key
export LICENSECHAIN_APP_NAME=your-app-name
export LICENSECHAIN_APP_VERSION=1.0.0

# Optional
export LICENSECHAIN_BASE_URL=https://api.licensechain.com
export LICENSECHAIN_DEBUG=true
```

### Advanced Configuration

```csharp
var config = new LicenseChainConfig
{
    ApiKey = "your-api-key",
    AppName = "your-app-name",
    Version = "1.0.0",
    BaseUrl = "https://api.licensechain.com",
    Timeout = 30,        // Request timeout in seconds
    Retries = 3,         // Number of retry attempts
    Debug = false,       // Enable debug logging
    UserAgent = "MyGame/1.0.0"  // Custom user agent
};
```

## 🛡️ Security Features

### Hardware ID Protection

The SDK automatically generates and manages hardware IDs to prevent license sharing:

```csharp
// Hardware ID is automatically generated and stored
string hardwareId = client.GetHardwareId();

// Validate against license
var validateTask = client.ValidateHardwareIdAsync(licenseKey, hardwareId);
```

### Secure Communication

- All API requests use HTTPS
- API keys are securely stored and transmitted
- Session tokens are automatically managed
- Webhook signatures are verified

### License Validation

- Real-time license validation
- Hardware ID binding
- Expiration checking
- Feature-based access control

## 📊 Analytics and Monitoring

### Event Tracking

```csharp
// Track custom events
var properties = new Dictionary<string, object>
{
    ["level"] = 1,
    ["playerCount"] = 10
};
var trackTask = client.TrackEventAsync("app.started", properties);

// Track license events
var licenseProperties = new Dictionary<string, object>
{
    ["licenseKey"] = "LICENSE-KEY",
    ["features"] = "premium,unlimited"
};
var licenseTrackTask = client.TrackEventAsync("license.validated", licenseProperties);
```

### Performance Monitoring

```csharp
// Get performance metrics
var metricsTask = client.GetPerformanceMetricsAsync();
yield return new WaitUntil(() => metricsTask.IsCompleted);

if (metricsTask.IsCompletedSuccessfully)
{
    var metrics = metricsTask.Result;
    Debug.Log($"API Response Time: {metrics.AverageResponseTime}ms");
    Debug.Log($"Success Rate: {metrics.SuccessRate:P}");
    Debug.Log($"Error Count: {metrics.ErrorCount}");
}
```

## 🔄 Error Handling

### Custom Exception Types

```csharp
try
{
    var license = await client.ValidateLicenseAsync("invalid-key");
}
catch (InvalidLicenseException ex)
{
    Debug.LogError("License key is invalid");
}
catch (ExpiredLicenseException ex)
{
    Debug.LogError("License has expired");
}
catch (NetworkException ex)
{
    Debug.LogError($"Network connection failed: {ex.Message}");
}
catch (LicenseChainException ex)
{
    Debug.LogError($"LicenseChain error: {ex.Message}");
}
```

### Retry Logic

```csharp
// Automatic retry for network errors
var config = new LicenseChainConfig
{
    ApiKey = "your-api-key",
    AppName = "your-app-name",
    Version = "1.0.0",
    Retries = 3,        // Retry up to 3 times
    Timeout = 30        // Wait 30 seconds for each request
};
```

## 🧪 Testing

### Unit Tests

```bash
# Run tests in Unity Test Runner
# Or via command line
Unity -batchmode -quit -projectPath . -runTests -testResults results.xml
```

### Integration Tests

```bash
# Test with real API
# Use Unity Test Runner with integration test category
```

## 📝 Examples

See the `Examples/` directory for complete examples:

- `LicenseChainExample.cs` - Basic SDK usage
- `AdvancedFeaturesExample.cs` - Advanced features and configuration
- `WebhookIntegrationExample.cs` - Webhook handling

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install Unity 2021.3 or later
3. Open the project in Unity
4. Install dependencies
5. Run tests in Unity Test Runner

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: [https://docs.licensechain.com/unity](https://docs.licensechain.com/unity)
- **Issues**: [GitHub Issues](https://github.com/LicenseChain/LicenseChain-Unity-SDK/issues)
- **Discord**: [LicenseChain Discord](https://discord.gg/licensechain)
- **Email**: support@licensechain.com

## 🔗 Related Projects

- [LicenseChain C# SDK](https://github.com/LicenseChain/LicenseChain-CSharp-SDK)
- [LicenseChain JavaScript SDK](https://github.com/LicenseChain/LicenseChain-JavaScript-SDK)
- [LicenseChain Customer Panel](https://github.com/LicenseChain/LicenseChain-Customer-Panel)

---

**Made with ❤️ for the Unity community**
