# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Sets the Assembly Version attributes of a single file.

.DESCRIPTION
    Sets the AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion attribute values
    in the supplied C# source file.
    The script only sets the values for the attributes found in the source file.

    The AssemblyVersion version has the following format: <major>.<minor>.<path>.<revision>
    The AssemblyFileVersion version has the following format: <major>.<minor>.<path>.<revision>
    The AssemblyInformationalVersion tas the following format: major.minor.patch[-<prerelease>][+<build>]
    where <prerelease> and <build> are optional.
    
.PARAMETER SourceFilePath
    The absolute path to the C# (.cs) file holding the attribute values to change.

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
    Example 1: ./AssemblyInfoFileSetVersion.ps1 AssemblyInfo.cs 1.0.0
    Produces the following attribute values
    [assembly: AssemblyVersion("1.0.0.0")]
    [assembly: AssemblyFileVersion("1.0.0.0")]
    [assembly: AssemblyInformationalVersion("1.0.0")]

    Example 2: ./AssemblyInfoFileSetVersion.ps1 AssemblyInfo.cs 1.0.0 42
    Produces the following attribute values
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0")]

    Example 3: ./AssemblyInfoFileSetVersion.ps1 AssemblyInfo.cs 1.0.0 42 -VersionSuffix dev
    Produces the following attribute values
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0.dev")]

    Example 3: ./AssemblyInfoFileSetVersion.ps1 AssemblyInfo.cs 1.0.0 42 -VersionSuffix dev -VersionBuild ef02d2
    Produces the following attribute values
    [assembly: AssemblyVersion("1.0.0.42")]
    [assembly: AssemblyFileVersion("1.0.0.42")]
    [assembly: AssemblyInformationalVersion("1.0.0.dev+ef02d2")]
#>

Param(
    [Parameter(Mandatory=$true)][string]$SourceFilePath,
    [Parameter(Mandatory=$true)][ValidatePattern("^\d+\.\d+\.\d+$")][string]$VersionPrefix,
    [Parameter(Mandatory=$false)][int]$VersionRevision = 0,
    [Parameter(Mandatory=$false)][string]$VersionSuffix,
    [Parameter(Mandatory=$false)][string]$VersionBuild
)

# regex pattern to search for in the supplied C# file
$patternAssemblyVersion = '\[assembly: AssemblyVersion\("(.*)"\)\]'
$patternAssemblyFileVersion = '\[assembly: AssemblyFileVersion\("(.*)"\)\]'
$patternAssemblyInformationalVersion = '\[assembly: AssemblyInformationalVersion\("(.*)"\)\]'


function Update-VersionInAssemblyInfoFile
{
    Param ([string]$filePath, [string]$assemblyVersion, [string]$assemblyFileVersion, [string]$assemblyInformationalVersion)

    # replace the contents in the file
    (Get-Content $filePath) | ForEach-Object {
        if($_ -match $patternAssemblyVersion)
        {
            Write-Host "[assembly: AssemblyVersion(`"$assemblyVersion`")]"
            '[assembly: AssemblyVersion("{0}")]' -f $assemblyVersion
        }
        elseif ($_ -match $patternAssemblyFileVersion)
        {
            Write-Host "[assembly: AssemblyFileVersion(`"$assemblyFileVersion`")]"
            '[assembly: AssemblyFileVersion("{0}")]' -f $assemblyFileVersion
        }
        elseif ($_ -match $patternAssemblyInformationalVersion)
        {
            Write-Host "[assembly: AssemblyInformationalVersion(`"$assemblyInformationalVersion`")]"
            '[assembly: AssemblyInformationalVersion("{0}")]' -f $assemblyInformationalVersion
        }
        else
        {
            # Output line as is
            $_
        }
    } | Set-Content $filePath
}

# Build the versions to set
$assemblyVersion = "$VersionPrefix.$VersionRevision"
$assemblyFileVersion = "$VersionPrefix.$VersionRevision"
$assemblyInformationalVersion = $VersionPrefix
if ($VersionSuffix) { $assemblyInformationalVersion += "-$VersionSuffix" }
if ($VersionBuild) { $assemblyInformationalVersion += "+$VersionBuild" }

Write-Host "Processing File: $sourceFilePath"

Update-VersionInAssemblyInfoFile $SourceFilePath $assemblyVersion $assemblyFileVersion $assemblyInformationalVersion