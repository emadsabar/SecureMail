# SmimeLifecycle — Certificate Management Server

On-premises **S/MIME certificate lifecycle** platform. The CMS manages eligibility, enrollment, issuance tracking, endpoint installation, Active Directory publication, Outlook readiness, renewal, revocation, and audit.

**Not** a mail gateway, SMTP relay, DLP, archive, or content scanner.

## Deployment model

- **On-premises Active Directory only** for identity, eligibility, and public-certificate publication.
- **No cloud identity / Entra ID / Microsoft Graph / Entra Connect** in this model.
- Windows agents and the admin UI talk **only to the CMS**.
- The CMS is the **only** component that talks to the certificate authority (Sectigo or lab Development CA).

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Vertical slice

```text
AD eligibility → CMS enrollment → endpoint CSR → CMS→CA issuance
→ CurrentUser\My install → AD publish → Outlook check → Active
```

## Solution layout

| Path | Role |
|---|---|
| `src/SmimeLifecycle.Api` | Certificate Management Server API |
| `src/SmimeLifecycle.Application` | MediatR commands/queries |
| `src/SmimeLifecycle.Domain` | Entities and lifecycle state machine |
| `src/SmimeLifecycle.Infrastructure` | EF Core, Hangfire, **CA adapter**, AD adapters |
| `src/SmimeLifecycle.WindowsAgent` | Endpoint worker (CMS client only) |
| `src/SmimeLifecycle.OutlookAddIn` | Classic Outlook adapter |
| `src/SmimeLifecycle.Worker` | Background Hangfire host |
| `frontend/admin` | React admin UI (CMS client) |
| `scripts/` | PowerShell AD / packaging runbooks |

## Constraints

- Private keys / PFX / tokens are never stored in PostgreSQL.
- Keys and CSRs are generated on the Windows endpoint.
- CA HTTP details stay behind `ICertificateAuthority` and configuration (do not invent Sectigo routes).
- Agents must not reference Infrastructure or call the CA.

## Local development

```powershell
docker compose up -d
dotnet restore
dotnet test
dotnet run --project src/SmimeLifecycle.Api

cd frontend/admin
npm ci
npm run dev
```

- API / Swagger: http://localhost:5088/swagger  
- Admin UI: http://localhost:5173  

Lab defaults:

- `CertificateAuthority:Provider` = `Development` (CMS-local signer)
- `IdentityDirectory:Provider` = `Configured` (sample AD users in appsettings)
- `ActiveDirectory:Provider` = `Null` (publication recorded without LDAPS)

Production-oriented on-prem settings:

- `IdentityDirectory:Provider` = `ActiveDirectory` + `EligibleGroupDn`
- `ActiveDirectory:Provider` = `Ldaps` with bind credentials from a secrets store
- `CertificateAuthority:Provider` = `Sectigo` with **documented** BaseUrl/paths only
