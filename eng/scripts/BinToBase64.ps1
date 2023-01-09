# ----------------------------------------------------------------------------
# <copyright company="Michael Koster">
#   Copyright (c) Michael Koster. All rights reserved.
#   Licensed under the MIT License.
# </copyright>
# ----------------------------------------------------------------------------

<#
.SYNOPSIS
    Encodes a arbiary binary file as base64 encoded string.

.DESCRIPTION
     Encodes a arbiary binary file as base64 encoded string.

.PARAMETER SourceFilePath
    The absolute path to the source file to encode.

.PARAMETER TargetFilePath
    The path of the target file.

.INPUTS
    None

.OUTPUTS
    None

.NOTES
  
.EXAMPLE
#>

Param(
    [Parameter(Mandatory=$true)][string]$SourceFilePath,
    [Parameter(Mandatory=$true)][string]$TargetFilePath
)

[convert]::ToBase64String((Get-Content -Path $SourceFilePath -Encoding Byte)) | Set-Content -Path $TargetFilePath
