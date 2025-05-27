# Amazon GameLift Plugin for Unity Engine

![GitHub license](https://img.shields.io/github/license/aws/amazon-gamelift-plugin-unity)
![GitHub latest release version (by date)](https://img.shields.io/github/v/release/aws/amazon-gamelift-plugin-unity)
![GitHub downloads all releases](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/total)
![GitHub downloads latest release (by date)](https://img.shields.io/github/downloads/aws/amazon-gamelift-plugin-unity/latest/total)

Compatible with Unity 2021.3 LTS and 2022.3 LTS.

## Install the plugin

Complete the following steps to install and enable the plugin for your multiplayer game project. For more details, see the [Amazon GameLift documentation](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-install.html).

1. Install the Amazon GameLift Plugin for Unity.
    1. In your Unity project, open `Window > Package Manager`.
    1. Click `+ > Add package from tarball...` and select the tarball, `com.amazonaws.gamelift-<version>.tgz`.

1. Install the Amazon GameLift C# Server SDK for Unity plugin (aka. lightweight Unity plugin).
    1. Unzip the `GameLift-CSharp-ServerSDK-UnityPlugin-<version>.zip` file.
    1. In your Unity project, open `Edit > Project Settings > Package Manager`.
    1. Under `Scoped Registries`, click on the `+` button and enter the values for the [UnityNuGet](https://github.com/xoofx/UnityNuGet) scoped registry:
        ```
        Name: Unity NuGet
        Url: https://unitynuget-registry.openupm.com
        Scope(s): org.nuget
        ```
    1. In your Unity project, open `Window > Package Manager`.
    1. Click `+ > Add package from tarball...` and select the tarball within the unzipped folder, `com.amazonaws.gameliftserver.sdk-<version>.tgz`.
