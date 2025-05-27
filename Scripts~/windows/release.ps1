param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

& "$PSScriptRoot\clean.ps1"
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\build.ps1" }
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\export.ps1" $ServerSdkVersion }
exit $LASTEXITCODE
