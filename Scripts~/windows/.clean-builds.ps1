$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$CORE_LIBRARY_PLUGINS_PATH="$RUNTIME_PATH\Core\Plugins"
$SERVER_SDK_PLUGINS_PATH="$RUNTIME_PATH\Plugins"
$SAMPLE_GAME_PACKAGE_PATH="$ROOT_DIR\Samples~\SampleGame.unitypackage"

if (Test-Path -Path $CORE_LIBRARY_PLUGINS_PATH)
{
	Write-Host "Removing all Core Library DLLs"
	Get-ChildItem $CORE_LIBRARY_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item -ErrorAction Stop
}
else
{
	Write-Host "$CORE_LIBRARY_PLUGINS_PATH is already cleaned up. Continuing..."
}

if (Test-Path -Path $SERVER_SDK_PLUGINS_PATH)
{
	Write-Host "Removing all GameLift Server SDK DLLs"
	Get-ChildItem $SERVER_SDK_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item -ErrorAction Stop
}
else
{
	Write-Host "$SERVER_SDK_PLUGINS_PATH is already cleaned up. Continuing..."
}

if (Test-Path -Path $SAMPLE_GAME_PACKAGE_PATH)
{
	Write-Host "Removing packaged sample game"
	Remove-Item -Recurse -Force $SAMPLE_GAME_PACKAGE_PATH -ErrorAction Stop
}
else
{
	Write-Host "$SAMPLE_GAME_PACKAGE_PATH is already cleaned up. Continuing..."
}

Write-Host "Builds clean up completed!" -ForegroundColor Yellow

exit 0
