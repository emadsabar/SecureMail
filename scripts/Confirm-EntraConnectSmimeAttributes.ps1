<#
.SYNOPSIS
  Notes and checks for synchronizing userCertificate and userSMIMECertificate to Microsoft 365.

.DESCRIPTION
  Exchange Online S/MIME requires those attributes in on-premises AD, then Entra Connect sync.
  This runbook does not invent Connect schema mappings; it prints the Microsoft-documented attributes
  operators must verify in their existing Entra Connect configuration.
#>
Write-Output @"
Verify in Entra Connect / Microsoft 365:
- On-premises AD attributes: userCertificate, userSMIMECertificate
- Connector attribute flow includes both attributes toward Microsoft Entra ID
- User has a mailbox in Exchange Online
- Classic Outlook for Windows is the Phase 1 client
- New Outlook, OWA, mobile Outlook, and EAS are out of Phase 1 claims

Reference: https://learn.microsoft.com/en-us/exchange/security-and-compliance/smime-exo/configure-smime-exo
"@
