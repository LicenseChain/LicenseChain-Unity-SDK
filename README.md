# LicenseChain Unity SDK

[![License](https://img.shields.io/badge/license-Elastic--2.0-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3+-blue.svg)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-8.0+-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Package Manager](https://img.shields.io/badge/Package%20Manager-1.0+-green.svg)](https://docs.unity3d.com/Manual/Packages.html)

Official Unity SDK for LicenseChain - Secure license management for Unity games and applications.

## 🚀 Features

- **🔐 Secure Authentication** - User registration, login, and session management
- **📜 License Management** - Create, validate, update, and revoke licenses
- **🛡 Hardware ID Validation** - Prevent license sharing and unauthorized access
- **🔔 Webhook Support** - Real-time license events and notifications
- **📊 Analytics Integration** - Track license usage and performance metrics
- **⚡ High Performance** - Optimized for Unity's runtime
- **🔄 Async Operations** - Non-blocking HTTP requests and data processing
- **🛠 Easy Integration** - Simple API with comprehensive documentation

## License assertion JWT (RS256 + JWKS)

**API base:** `https://api.licensechain.app/v1`

Successful **`POST /v1/licenses/verify`** may include **`license_token`**, **`license_token_expires_at`**, and **`license_jwks_uri`**. The JWT must carry **`token_use`** = **`licensechain_license_v1`**.

**RS256 + JWKS:** Verify using keys from **`GET /v1/licenses/jwks`** (or the returned **`license_jwks_uri`**). For tamper-sensitive titles, prefer **backend verification**; in-client, Unity mirrors the **C#** stack.

**Unity:** Add **`System.IdentityModel.Tokens.Jwt`** (and dependencies) and follow **`LicenseAssertion.VerifyLicenseAssertionJwtAsync`** in [LicenseChain-CSharp-SDK](https://github.com/LicenseChain/LicenseChain-CSharp-SDK) (see **`examples/jwks_only/`**), or verify on your backend. See [THIN_CLIENT_PARITY](https://docs.licensechain.app/), [JWKS_EXAMPLE_PRIORITY](https://docs.licensechain.app/), and the operator quickref [JWKS_THIN_CLIENT_QUICKREF](https://docs.licensechain.app/).

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
    private LicenseChainApiV1Client client;

    void Start()
    {
        var config = new LicenseChainConfig
        {
            ApiKey = "your-api-key",
            BaseUrl = "https://api.licensechain.app/v1",
            Timeout = 30000,
            Retries = 3
        };

        client = new LicenseChainApiV1Client(config);
    }

    async void OnDestroy()
    {
        client?.Dispose();
    }
}
```

### User Authentication

```csharp
// Register a new user and fetch profile
public async void RegisterAndLoadProfile(string email, string password)
{
    await client.RegisterUserAsync(email, password, "Unity User");
    var profile = await client.GetCurrentUserAsync();
    Debug.Log(profile.ToString());
}
```

### License Management

```csharp
// Create a license under an app and validate it
public async void CreateAndValidateLicense(string appId, string email)
{
    var created = await client.CreateLicenseAsync(appId, email, "Unity User");
    var key = created["licenseKey"]?.ToString();
    var validation = await client.ValidateLicenseAsync(key, appId);
    Debug.Log(validation.ToString());
}
```

### Health Check

```csharp
public async void CheckHealth()
{
    var health = await client.HealthAsync();
    Debug.Log(health.ToString());
}
```

## Legacy Compatibility Surface

`LicenseChainManager` is preserved only for older non-v1 integrations that post actions such as `init`, `login`, `license`, and `chatget` to the root API host.

New Unity integrations should use `LicenseChainApiV1Client` and the API v1 examples in this repository.

The legacy manager was intentionally not normalized to `https://api.licensechain.app/v1`, because changing its transport shape would alter a separate compatibility surface rather than the supported API v1 client.

## 📚 API Reference

### LicenseChainApiV1Client

#### Constructor

```csharp
var config = new LicenseChainConfig
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.licensechain.app/v1",
    Timeout = 30000,
    Retries = 3
};

var client = new LicenseChainApiV1Client(config);
```

#### Methods

##### User Authentication

```csharp
var registerTask = client.RegisterUserAsync(email, password, "Unity User");
var userTask = client.GetCurrentUserAsync();
```

##### License Management

```csharp
var createTask = client.CreateLicenseAsync(appId, issuedEmail, issuedTo);
var validateTask = client.ValidateLicenseAsync(licenseKey, appId);
var revokeTask = client.RevokeLicenseAsync(licenseId, "manual revoke");
var activateTask = client.ActivateLicenseAsync(licenseId);
var extendTask = client.ExtendLicenseAsync(licenseId, "2027-01-01T00:00:00Z");
```

##### Analytics

```csharp
var analyticsTask = client.GetAnalyticsStatsAsync(appId, "30d");
var usageTask = client.GetUsageStatsAsync(appId, "30d");
var licenseAnalyticsTask = client.GetLicenseAnalyticsAsync(licenseId);
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
    public string baseUrl = "https://api.licensechain.app/v1";
    
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

# Optional
export LICENSECHAIN_BASE_URL=https://api.licensechain.app/v1
export LICENSECHAIN_DEBUG=true
```

### Advanced Configuration

```csharp
var config = new LicenseChainConfig
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.licensechain.app/v1",
    Timeout = 30000,     // Request timeout in milliseconds
    Retries = 3,         // Number of retry attempts
    EnableLogging = true,
    UserAgent = "MyGame/1.0.0"  // Custom user agent
};
```

## 🛡 Security Features

### Secure Communication

- All API requests use HTTPS
- API keys are securely stored and transmitted
- Webhook signatures are verified

### License Validation

- Real-time license validation
- Expiration checking

## 📊 Analytics and Monitoring

### Stats Queries

```csharp
var stats = await client.GetAnalyticsStatsAsync(appId, "30d");
var usage = await client.GetUsageStatsAsync(appId, "30d");
Debug.Log(stats.ToString());
Debug.Log(usage.ToString());
```

## 🔄 Error Handling

### Custom Exception Types

```csharp
try
{
    var result = await client.ValidateLicenseAsync("invalid-key", appId);
}
catch (NetworkException ex)
{
    Debug.LogError($"Network connection failed: {ex.Message}");
}
catch (ApiException ex)
{
    Debug.LogError($"API failed with {ex.StatusCode}: {ex.Message}");
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
    Retries = 3,        // Retry up to 3 times
    Timeout = 30000     // Wait up to 30 seconds for each request
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
- `AdvancedLicenseChainExample.cs` - Advanced API v1 usage

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install Unity 2021.3 or later
3. Open the project in Unity
4. Install dependencies
5. Run tests in Unity Test Runner

## 📄 License

This project is licensed under the Elastic License 2.0 (ELv2) — see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: [https://docs.licensechain.app/unity](https://docs.licensechain.app/unity)
- **Issues**: [GitHub Issues](https://github.com/LicenseChain/LicenseChain-Unity-SDK/issues)
- **Discord**: [LicenseChain Discord](https://discord.gg/licensechain)
- **Email**: support@licensechain.app

## 🔗 Related Projects

- [LicenseChain C# SDK](https://github.com/LicenseChain/LicenseChain-CSharp-SDK)
- [LicenseChain JavaScript SDK](https://github.com/LicenseChain/LicenseChain-JavaScript-SDK)
---

**Made with ❤️ for the Unity community**


## API Endpoints

Use the canonical API base URL `https://api.licensechain.app/v1` for the API v1 client. The REST client also accepts the root host and normalizes requests to the same API version.

### Base URL
- **Production**: https://api.licensechain.app/v1
- **Development**: https://api.licensechain.app/v1

### Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /v1/health | Health check |
| POST | /v1/auth/register | User registration |
| GET | /v1/auth/me | Current authenticated user |
| GET | /v1/apps | List applications |
| POST | /v1/apps/:id/licenses | Create license for app |
| POST | /v1/licenses/verify | Verify license |
| PATCH | /v1/licenses/:id/revoke | Revoke license |
| PATCH | /v1/licenses/:id/activate | Activate license |
| PATCH | /v1/licenses/:id/extend | Extend license |
| GET | /v1/webhooks | List webhooks |
| POST | /v1/webhooks | Create webhook |
| GET | /v1/analytics/stats | Get analytics |

**Note**: The SDK automatically prepends /v1 to all endpoints, so you only need to specify the path (e.g., /auth/login instead of /v1/auth/login).


## LicenseChain API (v1)

This SDK targets the **LicenseChain HTTP API v1** implemented by the LicenseChain API service.

- **Production base URL:** https://api.licensechain.app/v1
- **API reference:** [docs.licensechain.app](https://docs.licensechain.app/)
- **Baseline REST mapping (documented for integrators):**
  - GET /health
  - POST /auth/register
  - POST /licenses/verify
  - PATCH /licenses/:id/revoke
  - PATCH /licenses/:id/activate
  - PATCH /licenses/:id/extend
  - GET /analytics/stats

