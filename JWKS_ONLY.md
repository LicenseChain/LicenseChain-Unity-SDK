# Unity — license assertion JWT + JWKS

Unity does not ship JWT DLLs in-package. Use one of these **runnable** patterns:

## Option A — Backend verification (recommended)

Call your server that runs **`POST /v1/licenses/verify`** and optionally validates `license_token` with **`GET /v1/licenses/jwks`** using the **C#** console sample:

```bash
# In LicenseChain-CSharp-SDK repo:
dotnet run --project examples/jwks_only/jwks_only.csproj
```

Set `LICENSECHAIN_LICENSE_TOKEN` and `LICENSECHAIN_LICENSE_JWKS_URI` from the verify response.

## Option B — Embed .NET JWT stack in Unity

For supported player targets, add NuGet packages compatible with Unity’s runtime (e.g. `System.IdentityModel.Tokens.Jwt`) and mirror the C# **`LicenseAssertion.VerifyLicenseAssertionJwtAsync`** flow from **LicenseChain-CSharp-SDK** / **LicenseChain-VB-SDK** `LicenseAssertion`.

## Contract

- API base: `https://api.licensechain.app/v1`
- `token_use` claim: `licensechain_license_v1`
- JWKS URL from verify response: `license_jwks_uri`

Umbrella quick reference: [JWKS_THIN_CLIENT_QUICKREF.md](https://docs.licensechain.app/).
