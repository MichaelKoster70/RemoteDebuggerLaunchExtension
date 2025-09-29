# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Writes a version.json file with the specified version to the \src\Extension\RemoteDebuggerLauncher\ToolsRemote folder

.DESCRIPTION
    NOPSIS
    Writes a version.json file with the specified version to the \src\Extension\RemoteDebuggerLauncher\ToolsRemote folder

    The Version version has the following format: <major>.<minor>
    
.PARAMETER VersionPrefix
    A mandatory string holding the version prefix consisting of <major>.<minor>.<path>

.INPUTS
    None

.OUTPUTS
    None

.NOTES
  
.EXAMPLE
    Example 1: ./WriteVersionJson.ps1 1.0.0
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$toolsRemoteDir = Join-Path $PSScriptRoot "..\src\Extension\RemoteDebuggerLauncher\ToolsRemote" | Resolve-Path -Relative

# Get all subdirectories (do not include the root directory itself)
$targetDirectories = Get-ChildItem -Path $toolsRemoteDir -Directory -Recurse | ForEach-Object { $_.FullName }

# Create the version object
$versionObj = @{
    version = $Version
}

foreach ($dir in $targetDirectories) {
    # Ensure the directory exists (should always be true, but for safety)
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
    $versionFile = Join-Path $dir "version.json"
    $versionObj | ConvertTo-Json -Depth 2 | Set-Content -Path $versionFile -Encoding UTF8
    Write-Host "version.json written to $versionFile with version $Version"
}
