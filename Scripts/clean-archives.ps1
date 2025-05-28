# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ROOT_DIR=Resolve-Path "$PSScriptRoot\.."
$OUT_DIR="$ROOT_DIR\Out"

if (Test-Path -Path $OUT_DIR)
{
	Write-Host "Removing Out directory to clear out release artifacts..."
	Remove-Item -Recurse -Force $OUT_DIR -ErrorAction Stop
}
else
{
	Write-Host "$OUT_DIR is already cleaned up. Continuing..."
}

Write-Host "Out directory clean up completed!" -ForegroundColor Yellow

exit 0
