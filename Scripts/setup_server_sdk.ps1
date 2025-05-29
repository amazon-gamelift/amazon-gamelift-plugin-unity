# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$SERVER_SDK_DOWNLOAD_URL = "https://github.com/amazon-gamelift/amazon-gamelift-servers-csharp-server-sdk/releases/download/v5.3.0/GameLift-CSharp-ServerSDK-5.3.0.zip"
$CURRENT_DIRECTORY = Get-Location
$TEMP_FOLDER_PATH = Join-Path $CURRENT_DIRECTORY "Temp"
$DOWNLOADS_FOLDER_PATH = Join-Path $TEMP_FOLDER_PATH "downloads"
$DOWNLOADED_SERVER_SDK_FILE_PATH = Join-Path $DOWNLOADS_FOLDER_PATH "GameLift-CSharp-ServerSDK.zip"
$SERVER_SDK_EXTRACT_PATH = Join-Path $TEMP_FOLDER_PATH "GameLiftServerSDK"
$COMMON_PACKAGE_SOURCE_DIRECTORY = Join-Path $SERVER_SDK_EXTRACT_PATH "src\GameLiftServerSDK\Common\"
$SERVER_PACKAGE_SOURCE_DIRECTORY = Join-Path $SERVER_SDK_EXTRACT_PATH "src\GameLiftServerSDK\Server\"
$RELEASE_NOTES_SOURCE_DIRECTORY = Join-Path $SERVER_SDK_EXTRACT_PATH "release-notes\"
$COMMON_PACKAGE_DESTINATION_DIRECTORY = Join-Path $CURRENT_DIRECTORY "GameLiftServerSDK\Runtime\Core\Common\"
$SERVER_PACKAGE_DESTINATION_DIRECTORY = Join-Path $CURRENT_DIRECTORY "GameLiftServerSDK\Runtime\Core\Server\"
$RELEASE_NOTES_DESTINATION_DIRECTORY = Join-Path $CURRENT_DIRECTORY "GameLiftServerSDK\release-notes\"

try {
    if (!(Test-Path $COMMON_PACKAGE_DESTINATION_DIRECTORY)) {
        throw "Target folder: $COMMON_PACKAGE_DESTINATION_DIRECTORY does not exist."
    }
    if (!(Test-Path $SERVER_PACKAGE_DESTINATION_DIRECTORY)) {
        throw "Target folder: $SERVER_PACKAGE_DESTINATION_DIRECTORY does not exist."
    }
    if (!(Test-Path $RELEASE_NOTES_DESTINATION_DIRECTORY)) {
        throw "Target folder: $RELEASE_NOTES_DESTINATION_DIRECTORY does not exist."
    }
    if (!(Test-Path $DOWNLOADS_FOLDER_PATH)) {
        New-Item -ItemType Directory -Path $DOWNLOADS_FOLDER_PATH -Force | Out-Null
    }
    if (!(Test-Path $TEMP_FOLDER_PATH)) {
        New-Item -ItemType Directory -Path $TEMP_FOLDER_PATH -Force | Out-Null
    }

    if (!(Test-Path $DOWNLOADED_SERVER_SDK_FILE_PATH)) {
        Write-Host "Downloading GameLift SDK..."
        Invoke-WebRequest -Uri $SERVER_SDK_DOWNLOAD_URL -OutFile $DOWNLOADED_SERVER_SDK_FILE_PATH -ErrorAction Stop
    } else {
        Write-Host "SDK zip file already exists in Temp/downloads folder, skipping download."
    }

    if (Test-Path $SERVER_SDK_EXTRACT_PATH) {
        Remove-Item -Recurse -Force $SERVER_SDK_EXTRACT_PATH -ErrorAction Stop
    }
    Expand-Archive -Path $DOWNLOADED_SERVER_SDK_FILE_PATH -DestinationPath $SERVER_SDK_EXTRACT_PATH -Force -ErrorAction Stop

    Write-Host "Copying Common folder..."
    New-Item -ItemType Directory -Path $COMMON_PACKAGE_DESTINATION_DIRECTORY -Force | Out-Null
    Copy-Item -Path "$COMMON_PACKAGE_SOURCE_DIRECTORY*" -Destination $COMMON_PACKAGE_DESTINATION_DIRECTORY -Recurse -Force -ErrorAction Stop

    Write-Host "Copying Server folder..."
    New-Item -ItemType Directory -Path $SERVER_PACKAGE_DESTINATION_DIRECTORY -Force | Out-Null
    Copy-Item -Path "$SERVER_PACKAGE_SOURCE_DIRECTORY*" -Destination $SERVER_PACKAGE_DESTINATION_DIRECTORY -Recurse -Force -ErrorAction Stop

    Write-Host "Copying release-notes folder..."
    New-Item -ItemType Directory -Path $RELEASE_NOTES_DESTINATION_DIRECTORY -Force | Out-Null
    Copy-Item -Path "$RELEASE_NOTES_SOURCE_DIRECTORY*" -Destination $RELEASE_NOTES_DESTINATION_DIRECTORY -Recurse -Force -ErrorAction Stop

    Write-Host "Cleaning up extracted files..."
    Remove-Item -Recurse -Force $SERVER_SDK_EXTRACT_PATH -ErrorAction Stop

    Write-Host "Done."
    exit 0

} catch {
    Write-Host "ERROR: $_"
    exit 1
}
