$TEMP_PATH="C:\Temp\AmazonGameLiftPluginUnity"

if (Test-Path -Path $TEMP_PATH)
{
	Write-Host "Removing temporary folder to clear out downloaded artifacts..."
	Remove-Item -Recurse -Force $TEMP_PATH -ErrorAction Stop
}
else
{
	Write-Host "$TEMP_PATH is already cleaned up. Continuing..."
}

Write-Host "Downloads clean up completed!" -ForegroundColor Yellow
exit 0