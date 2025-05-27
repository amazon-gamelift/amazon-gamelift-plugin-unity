& "$PSScriptRoot\.build-core-library.ps1"
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\.build-sample-game.ps1" }
exit $LASTEXITCODE
