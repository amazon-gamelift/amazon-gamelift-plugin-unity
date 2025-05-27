param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

& "$PSScriptRoot\.export-plugin-tarball.ps1"
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\.export-server-sdk.ps1" $ServerSdkVersion }
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\.export-release-zip.ps1" $ServerSdkVersion }
exit $LASTEXITCODE
