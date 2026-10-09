<#
.SYNOPSIS
  Operational helper to publish a public certificate DER to AD userCertificate / userSMIMECertificate.

.DESCRIPTION
  The platform publishes through LDAPS in the worker. This script is a runbook for help desk
  recovery, not a substitute for the C# publisher. It never handles private keys or PFX files.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$DistinguishedName,

    [Parameter(Mandatory)]
    [string]$PublicCertificatePath,

    [ValidateSet("userCertificate", "userSMIMECertificate")]
    [string]$Attribute = "userCertificate"
)

if ($PublicCertificatePath -match '\.(pfx|p12)$') {
    throw "PFX/P12 files are not permitted. Publish the public certificate only (.cer/.crt)."
}

$bytes = [System.IO.File]::ReadAllBytes($PublicCertificatePath)
Set-ADUser -Identity $DistinguishedName -Add @{ $Attribute = $bytes }
Write-Output "Published public certificate bytes to $Attribute for $DistinguishedName. Confirm Entra Connect sync of this attribute next."
