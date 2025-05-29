# Amazon GameLift Servers Plugin for Unity


![GitHub license](https://img.shields.io/github/license/amazon-gamelift/amazon-gamelift-plugin-unity)
![GitHub latest release version (by date)](https://img.shields.io/github/v/release/amazon-gamelift/amazon-gamelift-plugin-unity)
![GitHub downloads all releases](https://img.shields.io/github/downloads/amazon-gamelift/amazon-gamelift-plugin-unity/total)
![GitHub downloads latest release (by date)](https://img.shields.io/github/downloads/amazon-gamelift/amazon-gamelift-plugin-unity/latest/total)

See Releases page for compatible Unity versions.

[Overview](#overview)

[Amazon GameLift Servers Resources](#amazon-gamelift-servers-resources)

[Contributing to this plugin](#contributing-to-this-plugin)


## Overview

This repository contains the C# server SDK plugin for integration with a Unity game project. It includes two variants of the plugin:

- `Amazon GameLift Servers SDK for Unity` includes the C# server SDK only. Use this plugin to add the server SDK to your game projects
  and customize your integration. For more details, see the README file in the `GameLiftServerSDK` folder of this repository.
- `Amazon GameLift Servers Plugin for Unity` includes the server SDK and additional UI components for the Unity Editor. The UI components
  give you guided workflows so you can deploy your game for hosting with Amazon GameLift Servers directly from the Unity editor.
  For more details, see the README file in the `GameLiftPlugin` folder of this repository.

Download the plugin variant that best fits your project requirements from the Releases page of this repository.

## Amazon GameLift Servers Resources

* [About Amazon GameLift Servers](https://aws.amazon.com/gamelift/)
* [Amazon GameLift Servers plugin guide](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html)
* [AWS Game Tech forum](https://repost.aws/topics/TAo6ggvxz6QQizjo9YIMD35A/game-tech/c/amazon-gamelift)
* [AWS for Games blog](https://aws.amazon.com/blogs/gametech/)
* [AWS Support Center](https://console.aws.amazon.com/support/home)
* [GitHub issues](https://github.com/amazon-gamelift/amazon-gamelift-plugin-unity/issues)
* [Contributing guidelines](CONTRIBUTING.md)


## Contributing to this plugin

If you’re interested in contributing to the Amazon GameLift Servers Plugins, clone the source code from the GitHub repository and follow these instructions.
1. Implement your desired changes or additions to the codebase within the cloned repository.
2. Once your changes are ready for testing, navigate to the root directory of the repository and run the following command:

    ```
    powershell -file setup.ps1
    ```
   Once the setup is completed, the Plugin and the SDK are ready for use.
3. Follow the instructions provided in the README of either the GameLiftPlugin or GameLiftServerSDK to install and use the plugins. Ensure that the plugin functions as expected and your modifications work as intended.
4. Submit a pull request to the repository's **develop** branch. Be sure to provide a clear and detailed description of your modifications, as well as any relevant information about your testing process and results.
