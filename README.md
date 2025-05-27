# Amazon GameLift Plugin for Unity Engine

![GitHub license](https://img.shields.io/github/license/aws/amazon-gamelift-plugin-unity)
![GitHub latest release version (by date)](https://img.shields.io/github/v/release/aws/amazon-gamelift-plugin-unity)
![GitHub downloads all releases](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/total)
![GitHub downloads latest release (by date)](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/latest/total)

See Releases page for compatible Unity versions.

# Overview

Amazon GameLift is a fully managed service that lets game developers to manage and scale dedicated game servers for session-based multiplayer games. The Amazon GameLift plugin for Unity provides tools that makes it quicker and easier to set up your Unity project for hosting on Amazon GameLift. Once the plugin is installed, you can access the plugin from within the Unity editor and start using it to integrate Amazon GameLift functionality into your client and server code. The plugin contains functionality to automatically bootstrap your game runtime environment to the AWS Cloud, fully test your game server integration with Amazon GameLift locally, and deploy your game servers on Amazon GameLift. For more information about using the plugin for Unity, see the [Amazon GameLift plugin for Unity guide](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html).

You can use built-in templates to deploy your game for the following common scenarios. 
* Single-region fleet: Deploy your game server to one fleet in a single AWS Region. Use this scenario to experiment with your install scripts and runtime deployment, as well as your integration.
* FlexMatch fleet: Deploy your game server for hosting with a FlexMatch matchmaking solution. [Amazon GameLift FlexMatch](https://docs.aws.amazon.com/gamelift/latest/flexmatchguide/match-intro.html) is a highly scalable and customizable matchmaking service for multiplayer games. Use this scenario to set up basic matchmaking components (including a rule set) that you can customize.

Each scenario uses an AWS CloudFormation template to deploy a resource stack for your game server solution. You can view and manage your resource stacks in the AWS Management Console for CloudFormation.

- [Prerequisites](#prerequisites)
- [Install the plugin](#install-the-plugin)
- [Contributing to this plugin](#contributing-to-this-plugin)
- [FAQ](#faq)
- [Amazon GameLift resources](#amazon-gamelift-resources)

## Prerequisites

* Amazon GameLift plugin for Unity download package. Download a zip file from [the GitHub Releases page](https://github.com/aws/amazon-gamelift-plugin-unity/releases). Or clone the plugin from the [Github repo](https://github.com/aws/amazon-gamelift-plugin-unity).
* A compatible Unity editor (2021.3 LTS, 2022.3 LTS) with Dedicated Server Build Support module for Windows (and Linux if desired).
* (Optional) A C# multiplayer game project with game code.
* An AWS account with access permissions to use Amazon GameLift, Amazon S3, and AWS CloudFormation. See [Set up programmatic access](https://docs.aws.amazon.com/gamelift/latest/developerguide/setting-up-aws-login.html) with long-term credentials.

## Install the plugin

Complete the following steps to install and enable the plugin for your multiplayer game project. For more details, see the [Amazon GameLift documentation](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-install.html).

1. Install the Amazon GameLift Plugin for Unity.
    1. Find the `com.amazonaws.gamelift-<version>.tgz` file within the downloaded release zip or follow the [contribution guide](CONTRIBUTING.md) to build the tarball yourself.
    1. In your Unity project, open `Window > Package Manager`.
    1. Click `+ > Add package from tarball...` and select the above tarball.

1. Install the Amazon GameLift C# Server SDK for Unity plugin (aka. lightweight Unity plugin).
    1. Find and unzip the `GameLift-CSharp-ServerSDK-UnityPlugin-<version>.zip` file within the downloaded release zip or download it from [Amazon GameLift's Getting Started](https://aws.amazon.com/gamelift/getting-started/).
    1. In your Unity project, open `Edit > Project Settings > Package Manager`.
    1. Under `Scoped Registries`, click on the `+` button and enter the values for the [UnityNuGet](https://github.com/xoofx/UnityNuGet) scoped registry:
        ```
        Name: Unity NuGet
        Url: https://unitynuget-registry.openupm.com
        Scope(s): org.nuget
        ```
    1. In your Unity project, open `Window > Package Manager`.
    1. Click `+ > Add package from tarball...` and select the tarball within the unzipped folder, `com.amazonaws.gameliftserver.sdk-<version>.tgz`.

1. (Optional) Import the sample project and configure the build settings.
    1. In your Unity project, select `Amazon GameLift > Sample Game > Import Sample Game` and import all assets.
    1. In your Unity project, select `Amazon GameLift > Sample Game > Initialize Settings`.

## Contributing to this plugin

### Prerequisites

* Administrator rights on a Microsoft Windows OS
* A supported Unity version
    * You also need to add the Unity editor folder (e.g. `C:\Program Files\Unity\Hub\Editor\<version>\Editor\ `) to the Windows PATH environment variable.
* AWS CLI 2.x
    * Ensure the AWS CLI is in the Windows PATH environment. This should be handled by the installation wizard. A restart may be required after installation is complete.
* Visual Studio 2019 (can be installed with Unity) or Jetbrains Rider 2024
* .NET Core 6 to build the core plugin source.
* NodeJS/npm: https://nodejs.org/en/download/ to package the plugin.

### Modifying the plugin code

1. Clone the [`amazon-gamelift-plugin-unity`](https://github.com/aws/amazon-gamelift-plugin-unity) repository from GitHub.
1. Run `Scripts~\windows\release.ps1 -Sdk <version>` in PowerShell to build the plugin and dependent libraries (only needed once).
1. In Unity Hub, create a new project.
1. Open Unity Package Manager, import project from disk, and select the `package.json` located in the plugin's root folder.
1. Setup code debugging in Unity: https://docs.unity3d.com/Manual/ManagedCodeDebugging.html, and change Unity project to Debug Mode.
1. A .sln file should be created in the Unity project root, you can open that with Visual Studio.
1. Make changes to the plugin code, and Unity should recompile after each change.
1. Once changes are made, run the unit tests via `Window > General > Test Runner`.

### Packaging the plugin

Run `Scripts~\windows\release.ps1 -Sdk <version>` to clean, build, export, and package the plugin with the server SDK in a single command.

Alternatively:
1. Run `Scripts~\windows\clean.ps1` to delete all dlls and temp files (If you want to build faster, you can comment out `.clean-download-files` execution).
1. Run `Scripts~\windows\build.ps1` to build dlls and sample game.
1. Run `Scripts~\windows\export.ps1 -Sdk <version>` to export the plugin into a tarball (.tgz) and package it with the server SDK in the project root folder.

### Testing the plugin

Follow instructions in [Unity Docs](https://docs.unity3d.com/Manual/cus-tests.html#tests) to enable your project for testing:
1. Open the Project manifest (located at `<project>/Packages/manifest.json`).
1. Verify `com.amazonaws.gamelift` is present as a dependency.
1. Add to the bottom of the file:

````
    "testables": [ "com.amazonaws.gamelift" ]
````

After enabling testing, the project tests can be run via [Unity Test Runner](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html).

## FAQ

### What Unity versions are supported?

The Amazon GameLift Plug-in for Unity is compatible only with officially supported versions of Unity 2021.3 LTS and 2022.3 LTS for Windows and Mac OS.

### Where are the logs?

An additional error log file related to the Unity game project can be found in the following location: `
logs/amazon-gamelift-plugin-logs[YYYYMMDD].txt`. Note that the log file is created once a day.

## Amazon GameLift Resources

* [About Amazon GameLift](https://aws.amazon.com/gamelift/)
* [Amazon GameLift documentation](https://docs.aws.amazon.com/gamelift/)
* [AWS Game Tech forum](https://repost.aws/topics/TAo6ggvxz6QQizjo9YIMD35A/game-tech/c/amazon-gamelift)
* [AWS for Games blog](https://aws.amazon.com/blogs/gametech/)
* [AWS Support Center](https://console.aws.amazon.com/support/home)
* [GitHub issues](https://github.com/aws/amazon-gamelift-plugin-unity/issues)
* [Contributing guidelines](CONTRIBUTING.md)
