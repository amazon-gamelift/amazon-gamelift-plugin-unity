# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

$ErrorActionPreference = 'Stop'

function Assert-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $p = New-Object Security.Principal.WindowsPrincipal($id)
    if (-not $p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Host 'Elevating privileges...' -ForegroundColor Yellow
        $psi = New-Object System.Diagnostics.ProcessStartInfo PowerShell
        $psi.Arguments = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
        $psi.Verb = 'runas'
        try { [Diagnostics.Process]::Start($psi) | Out-Null } catch { exit 1 }
        exit 0
    }
}

Assert-Admin

$filesLoc = 'C:\Game'
$msiPath = Join-Path $filesLoc 'gl-otel-collector.msi'
$configDir = Join-Path $filesLoc 'conf\gl-otel-collector'
$configDest   = 'C:\Program Files\Amazon\GLOTelCollector'
$metaFile     = 'C:\GameMetadata\gamelift-metadata.json'
$envDir       = Join-Path $filesLoc 'conf\gl-otel-collector'
$envFile      = Join-Path $envDir '.env'

Write-Host 'Installing (if MSI present)...' -ForegroundColor Cyan
if (Test-Path $msiPath) {
    Write-Host "Running MSI silently and waiting..." -ForegroundColor DarkCyan
    $proc = Start-Process msiexec.exe -ArgumentList @('/i', $msiPath, '/qn', '/norestart') -Wait -PassThru
    $exit = $proc.ExitCode
    Write-Host "MSI exit code: $exit" -ForegroundColor Gray
    if ($exit -ne 0 -and $exit -ne 3010) {
        Write-Error "MSI installation failed with code $exit"
        exit $exit
    }
    if ($exit -eq 3010) {
        Write-Host 'Reboot requested by MSI (3010) - continuing without reboot.' -ForegroundColor Yellow
    }
} else {
    Write-Warning "Collector MSI not found at $msiPath"
}

Write-Host 'Deploying configuration...' -ForegroundColor Cyan
try {
    Copy-Item (Join-Path $configDir 'gamelift-base.yaml') (Join-Path $configDest 'config-base.yaml')  -Force
    Write-Host "gamelift-base.yaml copied to $configDest" -ForegroundColor Green
    Copy-Item (Join-Path $configDir 'gamelift-ec2.yaml') (Join-Path $configDest 'config-ec2.yaml') -Force
    Write-Host "gamelift-ec2.yaml copied to $configDest" -ForegroundColor Green
} catch { Write-Warning "Failed to copy config: $_" }

Write-Host 'Parsing metadata...' -ForegroundColor Cyan
$fleetId = ''
$computeType = 'EC2'
if (Test-Path $metaFile) {
    try {
        $json = Get-Content $metaFile -Raw | ConvertFrom-Json
        $fleetId = $json.fleetId
        if ($json.PSObject.Properties.Name -contains 'ComputeType' -and $json.ComputeType) {
            $computeType = $json.ComputeType
        }
    } catch { Write-Warning "Failed to parse metadata JSON: $_" }
} else {
    Write-Warning "Metadata file not found at $metaFile (defaults will be used)"
}
if ([string]::IsNullOrEmpty($computeType)) { $computeType = 'EC2' }

Write-Host 'Setting machine environment variables...' -ForegroundColor Cyan
[Environment]::SetEnvironmentVariable('AWS_PROFILE','FleetRoleCredentials','Machine')
[Environment]::SetEnvironmentVariable('AWS_SHARED_CREDENTIALS_FILE','C:\Credentials\credentials','Machine')
[Environment]::SetEnvironmentVariable('AMP_REMOTE_WRITE_ENDPOINT','{{AMP_REMOTE_WRITE_ENDPOINT_VAR}}','Machine')
[Environment]::SetEnvironmentVariable('REGION','{{REGION_VAR}}','Machine')
if ($fleetId) { [Environment]::SetEnvironmentVariable('GAMELIFT_FLEET_ID',$fleetId,'Machine') }
[Environment]::SetEnvironmentVariable('GAMELIFT_COMPUTE_TYPE',$computeType,'Machine')

Write-Host 'Persisting .env file...' -ForegroundColor Cyan
if (-not (Test-Path $envDir)) { New-Item -ItemType Directory -Path $envDir -Force | Out-Null }
@(
    'AWS_PROFILE=FleetRoleCredentials'
    'AWS_SHARED_CREDENTIALS_FILE=C:\Credentials\credentials'
    'AMP_REMOTE_WRITE_ENDPOINT={{AMP_REMOTE_WRITE_ENDPOINT_VAR}}'
    'REGION={{REGION_VAR}}'
    ('GAMELIFT_FLEET_ID=' + $fleetId) | Where-Object { $fleetId }
    ('GAMELIFT_COMPUTE_TYPE=' + $computeType)
) | Where-Object { $_ -and $_ -notmatch '^\s*$' } | Set-Content -Path $envFile -Encoding UTF8
Write-Host "Wrote $envFile" -ForegroundColor Green

Write-Host 'Configuring service (if present)...' -ForegroundColor Cyan
$svc = Get-Service -Name 'WaitForGLCredentials' -ErrorAction SilentlyContinue
if ($svc) {
    try {
        Set-Service -Name 'WaitForGLCredentials' -StartupType Automatic
        if ($svc.Status -ne 'Running') { Start-Service -Name 'WaitForGLCredentials' }
        Write-Host 'Service ensured running.' -ForegroundColor Green
    } catch { Write-Warning "Failed to start service: $_" }
} else {
    Write-Host 'Service WaitForGLCredentials not found (MSI may not have installed it).' -ForegroundColor Yellow
}

# Helper to run an installer and check exit code
function runInstaller {
    param (
        [string]$Path,
        [string[]]$Arguments
    )
    if (Test-Path $Path) {
        Write-Host "Running installer $Path with args: $($Arguments -join ' ')" -ForegroundColor DarkCyan
        $proc = Start-Process -FilePath $Path -ArgumentList $Arguments -Wait -PassThru
        $exit = $proc.ExitCode
        Write-Host "Installer exit code: $exit" -ForegroundColor Gray
        if ($exit -ne 0) {
            Write-Error "Installer failed with code $exit"
            exit $exit
        }
    } else {
        Write-Warning "Installer not found at $path"
    }
}

###Add any game specific install steps below###

## Install .NET runtime (if needed)
## Uncomment if your game server requires .NET
## Note: installer files must be packaged inside the zip uploaded to GameLift
## You can download the latest .NET runtime from https://dotnet.microsoft.com/en-us/download
#$dotNetPath = Join-Path $filesLoc 'dotnet-runtime-8.0.19-win-x64.exe'
#runInstaller -Path $dotNetPath -Arguments '/quiet'
#$aspCorePath = Join-Path $filesLoc 'aspnetcore-runtime-8.0.19-win-x64.exe'
#runInstaller -Path $aspCorePath -Arguments '/quiet'


###End game specific install steps###

Write-Host 'Install script completed.' -ForegroundColor Green
exit 0

