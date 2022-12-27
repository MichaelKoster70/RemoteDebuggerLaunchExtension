# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Sets the Version attribute in a VXIS manifest file.

.DESCRIPTION
    Replaces the Version attribute value in the supplied vsixmanifest file.

.PARAMETER SourceFilePath
    The absolute path to thevsixmanifest file holding the attribute values to change.

.PARAMETER VersionPrefix
    A mandatory string holding the version prefix consisting of <major>.<minor>.<path>

.INPUTS
    None

.OUTPUTS
    None

.NOTES
  
.EXAMPLE
#>

Param(
    [Parameter(Mandatory=$true)][string]$SourceFilePath,
    [Parameter(Mandatory=$true)][ValidatePattern("^\d+\.\d+\.\d+$")][string]$VersionPrefix
)

function Update-VersionInFile
{
    Param ([string]$filePath, [string]$version)

    [xml]$vsixManifestXml = Get-Content $filePath

    $ns = New-Object System.Xml.XmlNamespaceManager $vsixManifestXml.NameTable
    $ns.AddNamespace("ns", $vsixManifestXml.DocumentElement.NamespaceURI) | Out-Null

    $attrVersion = $vsixManifestXml.SelectSingleNode("//ns:Identity", $ns).Attributes["Version"]

    $attrVersion.InnerText = $version

    $vsixManifestXml.Save($filePath)
}

if (-not (Test-Path $sourceFilePath))
{
    Write-Error "'$sourceFilePath' does not exist"
    exit 1
}

Write-Host "Processing File: $sourceFilePath"

$absolutePath = (Resolve-Path $SourceFilePath).Path
Update-VersionInFile $absolutePath $VersionPrefix