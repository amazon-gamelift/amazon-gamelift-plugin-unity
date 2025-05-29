# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ROOT_DIR=Resolve-Path "$PSScriptRoot\.."
$STANDALONE_PATH="$ROOT_DIR\GameLiftPlugin"
$OUT_DIR="$ROOT_DIR\Out"

if (!(Test-Path $OUT_DIR)) {
    New-Item -ItemType Directory -Path $OUT_DIR -Force | Out-Null
}

Write-Host "Exporting Standalone plugin source to tarball..."

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null)
{
	Write-Host "Unable to find 'npm' executable in your PATH. Please install nodejs first: https://nodejs.org/en/download/" -ForegroundColor Red
	exit 1
}

npm pack --pack-destination $OUT_DIR $STANDALONE_PATH

exit 0
