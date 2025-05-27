$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$BUILD_DIR="$ROOT_DIR\build"

Write-Host "Exporting plugin source to tarball..."

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null)
{
	Write-Host "Unable to find 'npm' executable in your PATH. Please install nodejs first: https://nodejs.org/en/download/" -ForegroundColor Red
	exit 1
}

npm pack --pack-destination $BUILD_DIR

exit 0
