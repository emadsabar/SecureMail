<#
.SYNOPSIS
  Packages the Windows agent for Intune Win32 deployment.
#>
[CmdletBinding()]
param(
    [string]$ProjectPath = "$PSScriptRoot\..\src\SmimeLifecycle.WindowsAgent\SmimeLifecycle.WindowsAgent.csproj",
    [string]$Output = "$PSScriptRoot\..\artifacts\agent"
)

New-Item -ItemType Directory -Force -Path $Output | Out-Null
dotnet publish $ProjectPath -c Release -r win-x64 --self-contained false -o $Output
Write-Output "Published agent to $Output. Wrap with Microsoft Win32 Content Prep Tool (IntuneWinAppUtil) using install command: SmimeLifecycle.WindowsAgent.exe"
