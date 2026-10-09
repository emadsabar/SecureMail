#Requires -Version 7.0
<#
.SYNOPSIS
  Manual helper to publish a public certificate DER/PEM to on-premises AD.

.DESCRIPTION
  Prefer letting the Certificate Management Server publish via LDAPS.
  This script is an operator runbook for break-glass / validation only.
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $DistinguishedName,

    [Parameter(Mandatory = $true)]
    [ValidateSet('userCertificate', 'userSMIMECertificate')]
    [string] $Attribute,

    [Parameter(Mandatory = $true)]
    [string] $CertificatePath
)

$bytes = if ($CertificatePath -match '\.pem$') {
    $pem = Get-Content -Raw -Path $CertificatePath
    $b64 = ($pem -replace '-----BEGIN CERTIFICATE-----', '' -replace '-----END CERTIFICATE-----', '' -replace '\s', '')
    [Convert]::FromBase64String($b64)
} else {
    [System.IO.File]::ReadAllBytes($CertificatePath)
}

Set-ADUser -Identity $DistinguishedName -Add @{ $Attribute = $bytes }
Write-Output "Published public certificate bytes to $Attribute for $DistinguishedName (on-premises AD only)."
