# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Sets the Assembly Version attributes of all file contained in a folder recursivly.

.DESCRIPTION
    Sets the AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion attribute values
    in each AssemblyInfo.cs file available in the specified directory tree
    The script only sets the values for the attributes found in the source file.

    The AssemblyVersion version has the following format: <major>.<minor>.<path>.<revision>
    The AssemblyFileVersion version has the following format: <major>.<minor>.<path>.<revision>
    The AssemblyInformationalVersion tas the following format: major.minor.patch[-<prerelease>][+<build>]
    where <prerelease> and <build> are optional.
    
.PARAMETER SourceDirectory
    The absolute path to the folder holding the AssemblyInfo.cs files to change

.PARAMETER VersionPrefix
    A mandatory string holding the version prefix consisting of <major>.<minor>.<path>

.PARAMETER VersionRevision
    A optional int holding the <revision> value, defaults to 0

.PARAMETER VersionSuffix
    A optional string holding the <prerelease> value.

.PARAMETER VersionBuild
    A optional string holding the <build> value.

.INPUTS
    None

.OUTPUTS
    None

.NOTES
  
.EXAMPLE
    Example 1: ./AssemblyInfoFilesSetVersion.ps1 C:\demo 1.0.0
    Produces the following attribute values:
    [assembly: AssemblyVersion("1.0.0.0")]
    [assembly: AssemblyFileVersion("1.0.0.0")]
    [assembly: AssemblyInformationalVersion("1.0.0")]

    Example 2: ./AssemblyInfoFilesSetVersion.ps1 C:\demo 1.0.0 42
    Produces the following attribute values:
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0")]

    Example 3: ./AssemblyInfoFilesSetVersion.ps1 C:\demo 1.0.0 42 -VersionSuffix dev
    Produces the following attribute values:
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0.dev")]

    Example 3: ./AssemblyInfoFilesSetVersion.ps1 C:\demo 1.0.0 42 -VersionSuffix dev -VersionBuild ef02d2
    Produces the following attribute values:
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0.dev+ef02d2")]
#>

Param(
    [Parameter(Mandatory=$true)][string]$SourceDirectory,
    [Parameter(Mandatory=$true)][ValidatePattern("^\d+\.\d+\.\d+$")][string]$VersionPrefix,
    [Parameter(Mandatory=$false)][int]$VersionRevision = 0,
    [Parameter(Mandatory=$false)][string]$VersionSuffix,
    [Parameter(Mandatory=$false)][string]$VersionBuild
)

# Make sure path to source directory is available
if (-not (Test-Path $SourceDirectory))
{
    Write-Error "'$SourceDirectory' does not exist"
    exit 1
}

Write-Host "Source Directory: $SourceDirectory"

(Get-ChildItem -Path $SourceDirectory -Recurse -File -Include "AssemblyInfo.cs") | ForEach-Object {
    Start-Job -FilePath "$PSScriptRoot\AssemblyInfoFileSetVersion.ps1" -ArgumentList $_, $VersionPrefix, $VersionRevision, $VersionSuffix, $VersionBuild | 
    Wait-Job | Receive-Job
}