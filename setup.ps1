# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ErrorActionPreference = "Stop"

$CLEAN_BUILDS_SCRIPT_PATH = "Scripts/clean-builds.ps1"
$CLEAN_TEMP_SCRIPT_PATH = "Scripts/clean-temp.ps1"
$CLEAN_ARCHIVES_SCRIPT_PATH = "Scripts/clean-archives.ps1"
$SETUP_SERVER_SDK_SCRIPT_PATH = "Scripts/setup_server_sdk.ps1"
$GENERATE_LIGHTWEIGHT_TARBALL_SCRIPT_PATH = "Scripts/generate-lightweight-tarball.ps1"
$BUILD_CORE_LIBRARY_SCRIPT_PATH = "Scripts/build-core-library.ps1"
$BUILD_SAMPLE_GAME_SCRIPT_PATH = "Scripts/build-sample-game.ps1"
$GENERATE_STANDALONE_TARBALL_SCRIPT_PATH = "Scripts/generate-standalone-tarball.ps1"
$GENERATE_LIGHTWEIGHT_ARCHIVE_SCRIPT_PATH = "Scripts/generate-lightweight-archive.ps1"
$GENERATE_STANDALONE_ARCHIVE_SCRIPT_PATH = "Scripts/generate-standalone-archive.ps1"

function Run-Script {
    param (
        [string]$scriptPath
    )

    Write-Host "Executing: $scriptPath"
    try {
        & $scriptPath
        if ($LASTEXITCODE -ne 0) {
            throw "Script $scriptPath failed with exit code $LASTEXITCODE"
        }
        Write-Host "Success: $scriptPath"
    } catch {
        Write-Host "Error: $_"
        exit 1
    }
}

Run-Script $CLEAN_BUILDS_SCRIPT_PATH
Run-Script $CLEAN_TEMP_SCRIPT_PATH
Run-Script $CLEAN_ARCHIVES_SCRIPT_PATH

Run-Script $SETUP_SERVER_SDK_SCRIPT_PATH
Run-Script $GENERATE_LIGHTWEIGHT_TARBALL_SCRIPT_PATH

Run-Script $BUILD_CORE_LIBRARY_SCRIPT_PATH
Run-Script $BUILD_SAMPLE_GAME_SCRIPT_PATH
Run-Script $GENERATE_STANDALONE_TARBALL_SCRIPT_PATH

Write-Host "All setup scripts completed successfully!"
