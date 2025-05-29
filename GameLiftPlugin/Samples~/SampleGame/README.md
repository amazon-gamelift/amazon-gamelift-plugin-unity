# Amazon GameLift Plugin for Unity Sample Client/Server

![GitHub license](https://img.shields.io/github/license/aws/amazon-gamelift-plugin-unity)
![GitHub latest release version (by date)](https://img.shields.io/github/v/release/aws/amazon-gamelift-plugin-unity)
![GitHub downloads all releases](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/total)
![GitHub downloads latest release (by date)](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/latest/total)

Compatible with Unity 2021.3 LTS and 2022.3 LTS.

Compatible with Amazon GameLift Server SDK v5.x.

Compatible with Amazon GameLift Plugin for Unity v2.x.

## Overview

This sample Unity project demonstrates the steps to integrate a game client and server with the Amazon GameLift Plugin for Unity.

This sample project supports the following core features of the plugin:
* Deployment scenarios (Single-region and FlexMatch).
* Local testing with Amazon GameLift Anywhere.
* Running the client or server code within the Unity Editor.

- [Prerequisites](#prerequisites)
- [Integration with the deployment scenarios](#integration-with-the-deployment-scenarios)
- [Integration with Amazon GameLift Anywhere](#integration-with-amazon-gamelift-anywhere)
- [Playing the game](#playing-the-game)
- [Running tests](#running-tests)

## Prerequisites

Follow the plugin [prerequisites](../../README.md#prerequisites) and [install instructions](../../README.md#install-the-plugin).
* Make sure to install a compatible Server SDK version (C# Server SDK for Unity plugin v5.x).
* Make sure to initialize the plugin settings after successfully importing the sample project.
    1. In your Unity project, select `Amazon GameLift > Sample Game > Import Sample Game` and import all assets.
    1. In your Unity project, select `Amazon GameLift > Sample Game > Initialize Settings`.

## Building the Client vs the Server

The sample project uses the `UNITY_SERVER` preprocessor to distinguish between client and server build targets. When you do `Initialize Settings`, this preprocessor is set up for the dedicated server build target. As long as the sample has been initialized, switching between build targets is enough to ensure the correct code path is compiled.

## Integration with the deployment scenarios

The client will need the AWS Region, Cognito Client ID and API Gateway Endpoint which are stored in the scriptable object at `Assets\Settings\GameLiftClientSettings.asset`. When the scenario is deployed, these properties are imported automatically when clicking `Apply Managed EC2 Settings` from the `Host with Managed EC2` tab.

The server code does not use these settings and integrates with the Amazon GameLift Server SDK instead of with the plugin.

In this mode, the client can be run directly from the editor or as a standalone executable while the server is run by GameLift in the cloud.

## Integration with Amazon GameLift Anywhere

The asset at `Assets\Settings\GameLiftClientSettings.asset` additionally stores a setting, `IsGameLiftAnywhere`, which generates setting YAML files when building the client and server executables. After creating an Anywhere fleet and registering compute, this property is imported automatically when clicking 'Apply Anywhere Settings' from the 'Host with Anywhere' tab.

The server settings are the parameters needed to call the [Amazon GameLift Server SDK API, InitSDK](https://docs.aws.amazon.com/gamelift/latest/developerguide/integration-server-sdk5-csharp-actions.html#integration-server-sdk5-csharp-initsdk-anywhere).

The server additionally requires an AuthToken generated from the [GetComputeAuthToken GameLift API](https://docs.aws.amazon.com/gamelift/latest/apireference/API_GetComputeAuthToken.html). When you run the server through the Unity editor, this token is generated and used automatically. If you run the server from a standalone executable, get a valid token and start the server from a command line, providing the auth token as the argument `--authToken`.

The client settings are the parameters needed to call GameLift APIs from the AWS SDK (including a fleet ID, location, and AWS credential profile).

## Playing the game

1. When starting the game client, sign up and log in. If using Anywhere, you can enter any values to login.
1. Press "ENTER" to get ready. There needs to be at least 2 players connected to the same server and ready for the game to start.
1. Use NUMPAD keys to match colored dots. Keys Y U I, H J K, N M "," may be used instead. The game will score matches until the ESC key is pressed.

## Running tests

1. Unit tests can be found at `GameLift-SampleGame\Assets\Tests\Editor\Unit`, or in the `SampleTests.Unit` project in your IDE. They can be run from the Unity Editor: `Window > General > Test Runner, EditMode`.
1. Play mode integration UI tests can be found at `GameLift-SampleGame\Assets\Tests\UI`, or in the `SampleTests.UI` project in your IDE. They can be run from the Unity Editor: `Window > General > Test Runner, PlayMode`. These tests need `GameLift-SampleGame\UiTestSettings.json` filled with your test parameters.

