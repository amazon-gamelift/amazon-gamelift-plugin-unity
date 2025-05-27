param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

$ROOT_DIR=Resolve-Path "$PSScriptRoot\..\.."
$BUILD_DIR="$ROOT_DIR\build"
$SERVER_SDK_DIR="GameLift-CSharp-ServerSDK-UnityPlugin-$ServerSdkVersion";
$SERVER_SDK_FILENAME="$SERVER_SDK_DIR.zip";
$DESTINATION_PATH="$BUILD_DIR\$SERVER_SDK_FILENAME"
$TEMP_PATH="C:\Temp\AmazonGameLiftPluginUnity"
$TEMP_ZIP_PATH = "$TEMP_PATH\$SERVER_SDK_FILENAME"
$TEMP_EXTRACTED_PATH = "$TEMP_PATH\$SERVER_SDK_DIR"
$SERVER_SDK_S3_LINK_PREFIX="https://gamelift-server-sdk-release.s3.us-west-2.amazonaws.com/unity"
$SERVER_SDK_S3_DOWNLOAD_LINK="$SERVER_SDK_S3_LINK_PREFIX/$SERVER_SDK_FILENAME"

if (Test-Path -Path $DESTINATION_PATH)
{
	Write-Host "Amazon GameLift Server SDK zip has already been prepared, skipping download and extraction." -ForegroundColor DarkYellow
	exit 0
}

Write-Host "Preparing Amazon GameLift Server SDK for release..."

if (-Not (Test-Path -Path $TEMP_ZIP_PATH) )
{
	New-Item -Force -Type Directory -Path $TEMP_PATH -ErrorAction Stop
	Write-Host "Downloading Amazon GameLift Server SDK $ServerSdkVersion..."
	# Download link should be public and require no credentials
	iwr $SERVER_SDK_S3_DOWNLOAD_LINK -OutFile $TEMP_ZIP_PATH
}
else
{
	Write-Host "Amazon GameLift Server SDK is already downloaded and stored at $TEMP_ZIP_PATH, skipping download."
}

Write-Host "Extracting and removing license file from Server SDK zip..."

Expand-Archive -Force -LiteralPath $TEMP_ZIP_PATH -DestinationPath $TEMP_EXTRACTED_PATH -ErrorAction Stop
Remove-Item -Force -LiteralPath "$TEMP_EXTRACTED_PATH\LICENSE.txt" -ErrorAction Stop

Write-Host "Re-packaging Server SDK zip into project directory..."

Compress-Archive -Force -Path "$TEMP_EXTRACTED_PATH\*" -DestinationPath $DESTINATION_PATH -ErrorAction Stop

Write-Host "Server SDK has been re-packaged without license file at '$DESTINATION_PATH'" -ForegroundColor Yellow

exit 0