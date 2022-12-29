# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Publishes a VSIX package to the Visual Studio marketplace.

.DESCRIPTION
    Puhblishes the supplied VSIX package to the Visual Studio marketplace using the provided metadata.

.PARAMETER ExtensionFile
    The mandatory absolute path to VSIX extension tu publish.

.PARAMETER PublishManifestFile
    The mandatory absolute path to the publish manifest JSON file.

.PARAMETER PersonalAccessToken
    The Personal Access Token (PAT) that's used to authenticate the publisher.

.INPUTS
    None

.OUTPUTS
    None

.NOTES
  
.EXAMPLE
#>

Param(
    [Parameter(Mandatory=$true)][string]$ExtensionFile,
    [Parameter(Mandatory=$true)][string]$PublishManifestFile,
    [Parameter(Mandatory=$true)][string]$PersonalAccessToken
)

function Find-VisualStudio
{
    [string]$vsWherePath = ""

    # try to find the tool on the path
    $vsWhere = Get-Command "vshere.exe" -ErrorAction SilentlyContinue
    if ($vsWhere -ne $null)
    {
        $vsWherePath =  $vsWhere.Path
    }
    else
    {
        # fallback to VS installed location
        $vsWherePath = "${env:ProgramFiles(x86)}\\Microsoft Visual Studio\Installer\vswhere.exe"
    }

    # for now, just request the base installation path
    return & "$vsWherePath" "-property" "installationPath"
}

function Find-VsixPublisher
{
    $vsPath = Find-VisualStudio
    return "$vsPath\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"
}

if (-not (Test-Path $ExtensionFile))
{
    Write-Error "'$ExtensionFile' does not exist"
    exit 1
}

if (-not (Test-Path $PublishManifestFile))
{
    Write-Error "'$PublishManifestFile' does not exist"
    exit 1
}

$vsixPublisher = Find-VsixPublisher
$ "$vsixPublisher" "-payload" $ExtensionFile "-publishManifest" $PublishManifestFile "-personalAccessToken" $PersonalAccessToken
