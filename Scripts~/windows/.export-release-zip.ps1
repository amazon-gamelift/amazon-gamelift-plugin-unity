param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$BUILD_DIR="$ROOT_DIR\build"
$STAGING_PATH="$BUILD_DIR\staging-release"
$README_PATH="$ROOT_DIR\Scripts~\PACKAGE_FOR_RELEASE.md"
$SERVER_SDK_FILENAME="GameLift-CSharp-ServerSDK-UnityPlugin-$ServerSdkVersion.zip";
$SERVER_SDK_PATH="$BUILD_DIR\$SERVER_SDK_FILENAME"

Write-Host "Getting version number from 'package.json'"

$PLUGIN_VERSION=Select-String -LiteralPath "$ROOT_DIR\package.json" -Pattern '"version": ".*",' | % { $_.Matches.Value } | % { $_.substring(12, $_.length-14) }

Write-Host "Identified package version as $PLUGIN_VERSION" -ForegroundColor DarkYellow

$TARBALL_PATH="$BUILD_DIR\com.amazonaws.gamelift-$PLUGIN_VERSION.tgz"
$DESTINATION_PATH="$BUILD_DIR\amazon-gamelift-plugin-unity-release-$PLUGIN_VERSION.zip"

Write-Host "Staging files for release..."

New-Item -Force -Type Directory -Path $STAGING_PATH -ErrorAction Stop
Move-Item -Force -LiteralPath $TARBALL_PATH, $SERVER_SDK_PATH -Destination $STAGING_PATH -ErrorAction Stop
Copy-Item -Force -LiteralPath $README_PATH -Destination "$STAGING_PATH\README.md" -ErrorAction Stop

Write-Host "Packaging zip file for release..."

Compress-Archive -Force -Path "$STAGING_PATH\*" -DestinationPath $DESTINATION_PATH -ErrorAction Stop

Write-Host "Packaging successful! Release artifact located at '$DESTINATION_PATH'" -ForegroundColor Green

exit 0