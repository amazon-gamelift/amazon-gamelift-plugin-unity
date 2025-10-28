# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ErrorActionPreference = 'Stop'

# Change to the script's directory to ensure relative paths work
Set-Location $PSScriptRoot

function Select-DeploymentMode {
    Write-Host "Select deployment mode:"
    Write-Host "1) EC2"
    Write-Host "2) Container"
    $choice = Read-Host "Enter the number corresponding to your choice [1]"
    if ([string]::IsNullOrWhiteSpace($choice)) { $choice = '1' }
    switch ($choice) {
        '1' { $env:DEPLOYMENT_MODE = 'ec2' }
        '2' { $env:DEPLOYMENT_MODE = 'container' }
        default { Write-Error "Invalid choice. Please select 1 for EC2 or 2 for Container."; exit 1 }
    }
    Write-Host "Using deployment mode: $($env:DEPLOYMENT_MODE)"
}

function Select-LinuxArch {

    Write-Host "Select the Linux architecture for your deployment:"
    Write-Host "1) amd64 (x86_64)"
    Write-Host "2) arm64 (aarch64)"
    $choice = Read-Host "Enter the number corresponding to your choice [1]"
    if ([string]::IsNullOrWhiteSpace($choice)) { $choice = '1' }
    switch ($choice) {
        '1' { $env:LINUX_ARCH = 'amd64' }
        '2' { $env:LINUX_ARCH = 'arm64' }
        default { Write-Error "Invalid choice. Please select 1 for amd64 (x86_64) or 2 for arm64 (aarch64)."; exit 1 }
    }

    Write-Host "Using Linux architecture: $($env:LINUX_ARCH)"
}

function Select-OsType {
    Write-Host "Select the servers target OS:"
    Write-Host "1) Linux"
    Write-Host "2) Windows"
    $choice = Read-Host "Enter the number corresponding to your OS [1]"
    if ([string]::IsNullOrWhiteSpace($choice)) { $choice = '1' }
    switch ($choice) {
        '1' { $env:OS_TYPE = 'linux' }
        '2' { $env:OS_TYPE = 'windows' }
        default { Write-Error "Invalid choice. Please select 1 for Linux or 2 for Windows."; exit 1 }
    }
    Write-Host "Using OS: $($env:OS_TYPE)"
}

# Function to prepare files needed for Windows EC2 deployment
function Prepare-Ec2-Windows {
    Write-Host "Preparing files for Windows EC2 deployment..."

    # Generate scripts from templates
    Copy-Item "otel-collector/ec2/templates/gamelift/install.bat" "out/install.bat"
    Copy-Item "otel-collector/ec2/templates/gamelift/install.ps1.tpl" "out/install.ps1"
    (Get-Content "out/install.ps1") -replace "\{\{AMP_REMOTE_WRITE_ENDPOINT_VAR\}\}", $env:AMP_REMOTE_WRITE_ENDPOINT `
                                -replace "\{\{REGION_VAR\}\}", $env:REGION | Set-Content "out/install.ps1"

    Write-Host "Copying the GameLift Servers OTel Collector package..."
    if (Test-Path 'otel-collector/common/gl-otel-collector.msi') {
        Copy-Item -LiteralPath 'otel-collector/common/gl-otel-collector.msi' -Destination 'out/' -Force
    } else {
        Write-Warning 'MSI package not found at otel-collector/common/gl-otel-collector.msi'
    }
}

# Function to prepare files needed for Linux EC2 deployment
function Prepare-Ec2-linux {
    # Generate scripts from templates
    Write-Host "Preparing files for Linux EC2 deployment..."
    Copy-Item "otel-collector/ec2/templates/gamelift/install.sh.tpl" "out/install.sh"
    (Get-Content "out/install.sh" -Raw) -replace "\{\{AMP_REMOTE_WRITE_ENDPOINT_VAR\}\}", $env:AMP_REMOTE_WRITE_ENDPOINT `
                                -replace "\{\{REGION_VAR\}\}", $env:REGION `
                                -replace "`r`n", "`n" | Set-Content -NoNewline -Encoding UTF8 "out/install.sh"

    Write-Host "Copying the GameLift Servers OTel Collector package..."
    if (Test-Path "otel-collector/common/gl-otel-collector.$($env:LINUX_ARCH).rpm") {
        Copy-Item -LiteralPath "otel-collector/common/gl-otel-collector.$($env:LINUX_ARCH).rpm" -Destination 'out/gl-otel-collector.rpm' -Force
    } else {
        Write-Warning "RPM package not found at otel-collector/common/gl-otel-collector.$($env:LINUX_ARCH).rpm"
    }
}

# Function to prepare common files needed for EC2 deployment (both Windows and Linux)
function Prepare-Ec2Files {
    # Copy common configuration files
    Write-Host "Copying necessary configuration files..."
    New-Item -ItemType Directory -Path 'out/conf/gl-otel-collector' -Force | Out-Null
    Copy-Item -LiteralPath 'otel-collector/common/conf/gl-otel-collector/gamelift-base.yaml' -Destination 'out/conf/gl-otel-collector/' -Force
    Copy-Item -LiteralPath 'otel-collector/common/conf/gl-otel-collector/gamelift-ec2.yaml' -Destination 'out/conf/gl-otel-collector/' -Force
}

# Function to prepare files needed for container deployment
function Prepare-ContainerFiles {
    Write-Host "Copying the GameLift Servers OTel Collector packages..."
    Copy-Item -LiteralPath "otel-collector/common/gl-otel-collector.$($env:LINUX_ARCH).rpm" -Destination 'out/gl-otel-collector.rpm' -Force
    Copy-Item -LiteralPath "otel-collector/common/gl-otel-collector.$($env:LINUX_ARCH).deb" -Destination 'out/gl-otel-collector.deb' -Force

    Write-Host "Copying necessary configuration files..."
    Copy-Item -LiteralPath 'otel-collector/common/conf/gl-otel-collector/gamelift-base.yaml' -Destination 'out/' -Force
    Copy-Item -LiteralPath 'otel-collector/common/conf/gl-otel-collector/gamelift-container.yaml' -Destination 'out/' -Force

    Write-Host "Copying container deployment files..."
    # Copy dotfile .env first (wildcard doesn't include dotfiles)
    Copy-Item -LiteralPath 'otel-collector/container/.env' -Destination 'out/' -Force
    Copy-Item -Path 'otel-collector/container/*' -Destination 'out/' -Recurse -Force
}

Select-DeploymentMode

# Check if .env file exists in deployment-mode specific directory
$envFilePath = "otel-collector/$($env:DEPLOYMENT_MODE)/.env"
if (!(Test-Path $envFilePath)) {
    Write-Host "otel-collector/$($env:DEPLOYMENT_MODE)/.env file not found! Make sure you create a .env file with your values for AMP_REMOTE_WRITE_ENDPOINT and REGION. See METRICS.md for more details."
    exit 1
}

# Load .env file (robust parsing similar to 'set -a; source ...')
foreach ($rawLine in Get-Content $envFilePath) {
    $line = $rawLine.Trim()
    if (-not $line) { continue }
    if ($line.StartsWith('#')) { continue }
    $eqIndex = $line.IndexOf('=')
    if ($eqIndex -lt 1) { continue }
    $key = $line.Substring(0, $eqIndex).Trim()
    $value = $line.Substring($eqIndex + 1).Trim()
    # Remove optional surrounding quotes
    if ($value.Length -ge 2 -and (
        ($value.StartsWith('"') -and $value.EndsWith('"')) -or
        ($value.StartsWith("'") -and $value.EndsWith("'"))
    )) { $value = $value.Substring(1, $value.Length - 2) }
    if ($key) { Set-Item -Path "env:$key" -Value $value }
}

# After loading the .env entries
# Fast check via already-loaded env vars
if ((Get-Content $envFilePath -Raw) -match 'UUID_HERE') {
    Write-Warning "$envFilePath file contains UUID_HERE. Please set a valid UUID for metrics"
}

# Clean up old out folder and recreate
Write-Host "Preparing the out folder..."
if (Test-Path 'out') { Remove-Item -Recurse -Force 'out' }
New-Item -ItemType Directory -Path 'out' | Out-Null

# Run deployment-mode specific preparation
if ($env:DEPLOYMENT_MODE -eq 'ec2') {
    Select-OsType
    if ($env:OS_TYPE -eq 'windows') {
        Prepare-Ec2-Windows
    } elseif ($env:OS_TYPE -eq 'linux') {
        # GameLift Servers EC2 fleets support multiple architectures on Linux.
        Select-LinuxArch

        Prepare-Ec2-linux
    }
    Prepare-Ec2Files
} else {
    # Containers are Linux only, but we need to know the architecture.
    Select-LinuxArch
    Prepare-ContainerFiles
    New-Item -ItemType Directory -Path 'out/game' | Out-Null
}

Write-Host "`nDone! All files required for your game server build are in the 'out/' directory."
