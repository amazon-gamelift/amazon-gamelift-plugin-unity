$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$BUILD_DIR="$ROOT_DIR\build"

if (Test-Path -Path $BUILD_DIR)
{
	Write-Host "Removing build directory to clear out release artifacts..."
	Remove-Item -Recurse -Force $BUILD_DIR -ErrorAction Stop
}
else
{
	Write-Host "$BUILD_DIR is already cleaned up. Continuing..."
}

Write-Host "Exports clean up completed!" -ForegroundColor Yellow

exit 0
