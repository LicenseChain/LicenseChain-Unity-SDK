using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LicenseChain.Unity
{
    /// <summary>
    /// API v1 REST client for LicenseChain endpoints.
    /// </summary>
    public sealed class LicenseChainApiV1Client : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private bool _disposed;

        public LicenseChainApiV1Client(LicenseChainConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.ApiKey))
                throw new ConfigurationException("API key is required");

            _baseUrl = NormalizeBaseUrl(config.BaseUrl ?? "https://api.licensechain.app/v1");
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(Math.Max(1000, config.Timeout))
            };

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", string.IsNullOrWhiteSpace(config.UserAgent)
                ? "LicenseChain-Unity-SDK/1.0.0"
                : config.UserAgent);
        }

        public Task<JObject> RegisterUserAsync(string email, string password, string name = null)
        {
            var payload = new { email, password, name };
            return PostAsync("/auth/register", payload);
        }

        public Task<JObject> GetCurrentUserAsync()
        {
            return GetAsync("/auth/me");
        }

        public Task<JObject> CreateLicenseAsync(string appId, string issuedEmail, string issuedTo = null, string expiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ConfigurationException("appId is required");
            if (string.IsNullOrWhiteSpace(issuedEmail))
                throw new ConfigurationException("issuedEmail is required");

            var payload = new
            {
                appId,
                issuedEmail,
                issuedTo,
                expiresAt
            };
            return PostAsync($"/apps/{appId}/licenses", payload);
        }

        public Task<JObject> ValidateLicenseAsync(string licenseKey, string appId = null, string hwuid = null)
        {
            var payload = new { key = licenseKey, app_id = appId, hwuid = string.IsNullOrWhiteSpace(hwuid) ? null : hwuid.Trim() };
            return PostAsync("/licenses/verify", payload);
        }

        /// <summary>
        /// Full POST /licenses/verify JSON (valid, optional license_token, license_jwks_uri). Supplies default hwuid when omitted.
        /// </summary>
        public Task<JObject> VerifyLicenseWithDetailsAsync(string licenseKey, string appId = null, string hwuid = null)
        {
            var resolved = string.IsNullOrWhiteSpace(hwuid) ? DefaultVerifyHwuid() : hwuid.Trim();
            var payload = new { key = licenseKey, app_id = appId, hwuid = resolved };
            return PostAsync("/licenses/verify", payload);
        }

        private static string DefaultVerifyHwuid()
        {
            var id = SystemInfo.deviceUniqueIdentifier ?? "unknown";
            var raw = $"licensechain|unity|{id}|{Application.platform}";
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public Task<JObject> RevokeLicenseAsync(string licenseId, string reason = null)
        {
            return PatchAsync($"/licenses/{licenseId}/revoke", new { reason });
        }

        public Task<JObject> ActivateLicenseAsync(string licenseId)
        {
            return PatchAsync($"/licenses/{licenseId}/activate", new { });
        }

        public Task<JObject> ExtendLicenseAsync(string licenseId, string expiresAtIso)
        {
            return PatchAsync($"/licenses/{licenseId}/extend", new { expiresAt = expiresAtIso });
        }

        public Task<JObject> GetLicenseAnalyticsAsync(string licenseId)
        {
            return GetAsync($"/licenses/{licenseId}/analytics");
        }

        public Task<JObject> GetAnalyticsStatsAsync(string appId = null, string period = null)
        {
            var query = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(appId)) query["app_id"] = appId;
            if (!string.IsNullOrWhiteSpace(period)) query["period"] = period;
            return GetAsync("/analytics/stats", query);
        }

        public Task<JObject> GetUsageStatsAsync(string appId = null, string period = "30d")
        {
            var query = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(appId)) query["app_id"] = appId;
            if (!string.IsNullOrWhiteSpace(period)) query["period"] = period;
            return GetAsync("/analytics/usage", query);
        }

        public Task<JObject> HealthAsync()
        {
            return GetAsync("/health");
        }

        private async Task<JObject> GetAsync(string path, Dictionary<string, string> query = null)
        {
            var url = BuildUrl(path, query);
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            return await ParseResponseAsync(response).ConfigureAwait(false);
        }

        private async Task<JObject> PostAsync(string path, object payload)
        {
            var response = await _httpClient.PostAsync(
                BuildUrl(path),
                BuildJsonContent(payload)
            ).ConfigureAwait(false);
            return await ParseResponseAsync(response).ConfigureAwait(false);
        }

        private async Task<JObject> PatchAsync(string path, object payload)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), BuildUrl(path))
            {
                Content = BuildJsonContent(payload)
            };
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return await ParseResponseAsync(response).ConfigureAwait(false);
        }

        private HttpContent BuildJsonContent(object payload)
        {
            var json = payload == null ? "{}" : JsonConvert.SerializeObject(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private string BuildUrl(string path, Dictionary<string, string> query = null)
        {
            var normalized = NormalizePath(path, _baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase));
            var url = _baseUrl + normalized;
            if (query == null || query.Count == 0) return url;

            var parts = new List<string>();
            foreach (var kv in query)
            {
                parts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
            }
            return url + "?" + string.Join("&", parts);
        }

        private string NormalizePath(string path, bool baseHasV1)
        {
            if (string.IsNullOrWhiteSpace(path)) return baseHasV1 ? "/" : "/v1";
            if (path.StartsWith("/v1/", StringComparison.OrdinalIgnoreCase))
                return baseHasV1 ? path.Substring(3) : path;
            if (path.StartsWith("/", StringComparison.Ordinal))
                return baseHasV1 ? path : "/v1" + path;
            return baseHasV1 ? "/" + path : "/v1/" + path;
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            var normalized = string.IsNullOrWhiteSpace(baseUrl)
                ? "https://api.licensechain.app/v1"
                : baseUrl.Trim().TrimEnd('/');
            return normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : normalized + "/v1";
        }

        private static async Task<JObject> ParseResponseAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new ApiException(
                    $"HTTP {(int)response.StatusCode}: {body}",
                    (int)response.StatusCode
                );

            if (string.IsNullOrWhiteSpace(body))
                return new JObject();

            try
            {
                return JObject.Parse(body);
            }
            catch (JsonException ex)
            {
                throw new ApiException("Invalid JSON response", (int)response.StatusCode, ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
