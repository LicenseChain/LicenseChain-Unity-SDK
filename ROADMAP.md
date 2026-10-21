# LicenseChain Unity SDK Roadmap

## Current Focus

- Keep API v1 defaults aligned with `https://api.licensechain.app/v1`.
- Maintain thin-client guidance and C# parity for JWT/JWKS verification.
- Keep UPM/`Assets/LicenseChain/package.json` metadata ready for OpenUPM installs.

## Next Operational Tasks

- Re-run conformance checks after Core API verify/JWKS contract updates.
- Keep docs and examples synchronized with https://docs.licensechain.app/en/sdk.
- Record release-impacting changes in `CHANGELOG.md`; develop experiments on `dev`, release from `main`.


## Active Milestone (2026-10-16)

- **Conformance re-run**: workspace `sdk-conformance` validate reports `errors=0` (remaining WARN = optional README PATCH license lifecycle aliases; DELETE/update paths cover revoke).
- **Docs sync**: README points at `https://docs.licensechain.app/en/sdk`; default API base documented as `https://api.licensechain.app/v1`.
- **Branch opsec**: land experiments on `dev`; promote to `main` only for release-ready changes.

## Ecosystem Notes

- Local Linux hosts lack Unity Editor; EditMode/batchmode tests (`Unity -batchmode -runTests`) require a Unity 2022.3+ install.
- Current validated Editor pin: Unity 2022.3 LTS.
- Evaluate Unity 6 LTS migration only after EditMode regression coverage is available in CI.
