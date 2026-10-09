# SmimeLifecycle

Centralized enterprise **S/MIME certificate lifecycle** management for Microsoft 365 / Exchange Online with hybrid Active Directory.

This product manages Sectigo-issued (or development CA) S/MIME certificates: eligibility, endpoint enrollment, issuance tracking, CurrentUser\My installation, AD publication, Entra Connect sync confirmation, Outlook readiness, renewal, revocation, and audit.

It is **not** a secure email gateway, SMTP relay, DLP platform, archive, or content-scanning system.

## Phase 1 scenario

Microsoft 365 Exchange Online + Entra ID + on-premises AD + Entra Connect + managed Windows devices + classic Outlook for Windows + Sectigo S/MIME.

Vertical slice:

```text
Entra eligibility → local key/CSR on Windows → CA request → issuance
→ install CurrentUser\My → publish AD → Entra Connect sync → Outlook check → Active
```

## Solution layout

| Path | Role |
|---|---|
| `src/SmimeLifecycle.Api` | ASP.NET Core Web API |
| `src/SmimeLifecycle.Application` | MediatR commands/queries, FluentValidation |
| `src/SmimeLifecycle.Domain` | Entities and lifecycle state machine |
| `src/SmimeLifecycle.Infrastructure` | EF Core/PostgreSQL, Hangfire, Sectigo/AD adapters |
| `src/SmimeLifecycle.WindowsAgent` | Endpoint worker: CSR, install, status |
| `src/SmimeLifecycle.OutlookAddIn` | Classic Outlook configuration adapter |
| `src/SmimeLifecycle.Worker` | Background Hangfire host |
| `frontend/admin` | React + TypeScript + Vite + Fluent UI |
| `scripts/` | PowerShell runbooks (AD, Intune, Entra Connect) |
| `tests/` | xUnit unit tests |

## Constraints

- Private keys, PFX passwords, access tokens, and complete PFX files are **not** stored in the backend database.
- Private keys and CSRs are generated on the managed Windows endpoint (prefer non-exportable).
- Sectigo API details stay behind `ICertificateAuthority` and configuration; endpoint names are not invented.
- Outlook configuration is a replaceable adapter.

## Prerequisites

- .NET 8 SDK (or newer SDK targeting `net8.0`)
- Node.js 22+
- Docker Desktop (PostgreSQL via Compose)
- PowerShell 7 for operational scripts

## Local development

```powershell
docker compose up -d
dotnet restore
dotnet test
dotnet run --project src/SmimeLifecycle.Api

# separate terminal
cd frontend/admin
npm ci
npm run dev
```

- API / Swagger: http://localhost:5088/swagger  
- Admin UI: http://localhost:5173  
- Hangfire: http://localhost:5088/hangfire  

Development defaults:

- `Authentication:Mode` = `Development` (no Entra token required)
- `CertificateAuthority:Provider` = `Development` (local CSR signer for lab use)
- `ActiveDirectory:Provider` = `Null` (records publication without LDAPS)

Set `CertificateAuthority:Provider` to `Sectigo` and fill configured path templates when wiring a real Certificate Manager tenant. Set `ActiveDirectory:Provider` to `Ldaps` with Key Vault–backed bind credentials for directory publication.

## Admin UI env

Copy `frontend/admin/.env.example`. Leave `VITE_API_BASE_URL` empty to use the Vite proxy. For Entra ID later, set `VITE_AUTH_MODE=EntraId` and the MSAL client/tenant/scope values.

## CI

GitHub Actions (`.github/workflows/ci.yml`) restores/tests the .NET solution and builds `frontend/admin`.
