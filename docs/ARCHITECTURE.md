# Architecture — on-premises Certificate Management Server

## Scope

SmimeLifecycle is an **on-premises Certificate Management Server (CMS)** for S/MIME certificate lifecycle.

- Identity and eligibility: **on-premises Active Directory** (LDAPS).
- Certificate publication: **on-premises AD** (`userCertificate` / `userSMIMECertificate`).
- Endpoints: managed Windows devices + classic Outlook.
- **No cloud identity or cloud mail dependency** in this deployment model (no Entra ID, no Microsoft Graph, no Entra Connect, no Exchange Online requirement).

## Non-negotiable CA mediation rule

```text
Windows agent / Outlook add-in / admin UI
        │
        │  HTTPS only
        ▼
Certificate Management Server (this API + workers)
        │
        │  only CMS may call the CA
        ▼
Certificate Authority (Sectigo or lab Development CA)
```

1. Clients **never** call the certificate authority directly.
2. Clients submit CSRs and poll **only** the CMS (`/api/agent/*`, `/api/*`).
3. Only `SmimeLifecycle.Infrastructure` hosts `ICertificateAuthority` implementations.
4. `SmimeLifecycle.WindowsAgent` must not reference Infrastructure or any CA SDK/HTTP client aimed at the CA.

## Trust boundaries

| Component | Talks to | Must not talk to |
|---|---|---|
| Windows agent | CMS API | CA, AD (except local cert store) |
| Admin UI | CMS API | CA |
| CMS API / Worker | PostgreSQL, AD (LDAPS), CA | Endpoint private keys |
| Outlook adapter | Local Outlook / CMS status | CA |

## Vertical slice (on-prem)

```text
AD eligibility (group/OU)
    ↓
CMS starts enrollment
    ↓
Agent generates non-exportable key + CSR on endpoint
    ↓
Agent posts CSR to CMS
    ↓
CMS submits CSR to CA and tracks issuance
    ↓
Agent installs public cert into CurrentUser\My
    ↓
CMS publishes public cert to AD
    ↓
Outlook readiness check
    ↓
Dashboard: Active
```

Private keys never leave the endpoint and are never stored in PostgreSQL.
