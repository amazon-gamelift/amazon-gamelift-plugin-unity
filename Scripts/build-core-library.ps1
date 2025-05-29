# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ROOT_DIR=Resolve-Path "$PSScriptRoot\.."
$CORE_LIBRARY_PATH="$ROOT_DIR\GameLiftPlugin\Runtime\Core"
$CORE_LIBRARY_PLUGINS_PATH="$CORE_LIBRARY_PATH\Plugins"

Write-Host "Building core library dependencies..."

if ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null) 
{ 
	Write-Host "Unable to find 'dotnet' executable in your PATH. See README on how to install .NET in order to build the plugin" -ForegroundColor Red
	exit 1
}

dotnet build "$CORE_LIBRARY_PATH\AmazonGameLiftPlugin.Core.csproj"

Write-Host "Core library dependencies built and saved in $CORE_LIBRARY_PLUGINS_PATH" -ForegroundColor Yellow

exit 0
