#Requires -Version 7.0
<#
.SYNOPSIS
  Verifies userCertificate / userSMIMECertificate on an on-premises AD user.

.DESCRIPTION
  Used after the Certificate Management Server publishes public certificates.
  This deployment does not use Entra Connect or cloud directory sync.
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $DistinguishedName,

    [Parameter(Mandatory = $false)]
    [string] $Server
)

$getParams = @{
    Identity = $DistinguishedName
    Properties = @('userCertificate', 'userSMIMECertificate')
}
if ($Server) { $getParams.Server = $Server }

$user = Get-ADUser @getParams
[pscustomobject]@{
    DistinguishedName     = $user.DistinguishedName
    UserCertificateCount  = @($user.userCertificate).Count
    UserSmimeCertCount    = @($user.userSMIMECertificate).Count
}
