$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$SAMPLES_PATH="$ROOT_DIR\Samples~"
$SAMPLE_GAME_PATH="$SAMPLES_PATH\SampleGame"
$SAMPLE_GAME_OUTPUT_PATH="$SAMPLES_PATH\SampleGame.unitypackage"
$SAMPLE_GAME_BUILD_LOG_PATH="$ROOT_DIR\build\SampleGameBuildLog.txt"
$SAMPLE_GAME_EXPORT_TIMEOUT=30
$EXPORT_START_TIME = Get-Date

Write-Host "Exporting Sample Game..."

if (Test-Path -Path $SAMPLE_GAME_OUTPUT_PATH)
{
	Remove-Item $SAMPLE_GAME_OUTPUT_PATH
}

if ((Get-Command "Unity.exe" -ErrorAction SilentlyContinue) -eq $null) {
	Write-Host "Cannot run Unity.exe. Please add the Unity editor folder (e.g. 'C:\Program Files\Unity\Hub\Editor\<version>\Editor\') to the Windows PATH environment variable." -ForegroundColor Red
	Write-Host "Export failed" -ForegroundColor Red
	exit 1
}

Unity.exe -batchmode -quit -projectPath $SAMPLE_GAME_PATH -logFile $SAMPLE_GAME_BUILD_LOG_PATH -executeMethod UnityPackageExporter.Export

Write-Host "Unity is exporting the sample game... The final build artifact will be saved in $SAMPLE_GAME_OUTPUT_PATH. Check log in $SAMPLE_GAME_BUILD_LOG_PATH to troubleshoot any issues." -ForegroundColor DarkYellow

Write-Host "Waiting for export to complete..."
while ((-not (Test-Path $SAMPLE_GAME_OUTPUT_PATH)) -and $EXPORT_START_TIME.AddSeconds($SAMPLE_GAME_EXPORT_TIMEOUT) -gt (Get-Date)) {
	Start-Sleep -m 1000
}

if (Test-Path -Path $SAMPLE_GAME_OUTPUT_PATH)
{
	Write-Host "Sample Game export completed! The exported artifact is saved in $SAMPLE_GAME_OUTPUT_PATH" -ForegroundColor Green
}
else
{
	Write-Host "Sample Game export failed! Check logs in $SAMPLE_GAME_BUILD_LOG_PATH to troubleshoot any issues." -ForegroundColor DarkRed
	exit 1
}

exit 0
