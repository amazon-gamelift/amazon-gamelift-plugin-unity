# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ROOT_DIR=Resolve-Path "$PSScriptRoot\.."
$LIGHTWEIGHT_PATH="$ROOT_DIR\GameLiftServerSDK"
$OUT_DIR="$ROOT_DIR\Out"

# Function to validate Unity meta files
function Validate-UnityMeta {
    param (
        [string]$CurrentDir
    )

    $valid = $true

    Get-ChildItem $CurrentDir | ForEach-Object {
        $fileRoot = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
        $fileExtension = [System.IO.Path]::GetExtension($_.Name)
        $currentFilePath = $_.FullName

        if ($fileExtension -eq ".meta") {
            # Check if the corresponding asset file exists
            $currentFilePathWithoutMeta = Join-Path $CurrentDir $fileRoot
            if (-not (Test-Path $currentFilePathWithoutMeta)) {
                $valid = $false
                Write-Host "ERROR - '$currentFilePath' does not have the corresponding asset file."
            }
        }
        else {
            # Check if the corresponding meta file exists
            # Skip special folders ending with a '~'. Unity ignores those folders
            if (-not ($_.PSIsContainer -and $_.Name.EndsWith('~'))) {
                if (-not (Test-Path ($currentFilePath + ".meta"))) {
                    $valid = $false
                    Write-Host "ERROR - '$currentFilePath' does not have the corresponding meta file."
                }
            }
        }

        # Recursively check all subdirectories
        if ($_.PSIsContainer) {
            $subDirValid = Validate-UnityMeta -CurrentDir $currentFilePath
            if (-not $subDirValid) {
                $valid = $false
            }
        }
    }

    return $valid
}

if (!(Test-Path $OUT_DIR)) {
    New-Item -ItemType Directory -Path $OUT_DIR -Force | Out-Null
}

# Validate Unity meta files
Write-Host "Validating Unity meta files..."
$metaValidationResult = Validate-UnityMeta -CurrentDir $LIGHTWEIGHT_PATH

if (-not $metaValidationResult) {
    Write-Host "Meta file validation failed. Please fix the above error(s) before creating the tarball for release. You might need to import the package into Unity via 'Add package from disk...' to have the missing meta files generated." -ForegroundColor Red
    exit 1
}

Write-Host "Unity meta file validation passed." -ForegroundColor Green


Write-Host "Exporting Lightweight Plugin source to tarball..."

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null)
{
	Write-Host "Unable to find 'npm' executable in your PATH. Please install nodejs first: https://nodejs.org/en/download/" -ForegroundColor Red
	exit 1
}

npm pack --pack-destination $OUT_DIR $LIGHTWEIGHT_PATH

exit 0
