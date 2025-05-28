# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ROOT_DIR=Resolve-Path "$PSScriptRoot\.."
$TEMP_DIR="$ROOT_DIR\Temp"
$DOWNLOADS_DIR="$TEMP_DIR\downloads"

if (Test-Path -Path $TEMP_DIR)
{
    Write-Host "Cleaning Temp directory while preserving downloads folder..."

    $items = Get-ChildItem -Path $TEMP_DIR -Exclude "downloads"

    foreach ($item in $items)
    {
        Remove-Item -Recurse -Force $item.FullName -ErrorAction Stop
    }
}
else
{
    Write-Host "$TEMP_DIR is already cleaned up. Continuing..."
}

Write-Host "Temp directory clean up completed!" -ForegroundColor Yellow

exit 0
