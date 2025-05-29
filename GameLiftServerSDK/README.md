# Amazon GameLift Servers SDK for Unity


## Overview
The Amazon GameLift Servers SDK for Unity provides libraries to integrate Unity based game servers with the Amazon GameLift Servers.

## Documentation
You can find the official Amazon GameLift Servers documentation [here](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html).

## Supported Versions
The Amazon GameLift Servers SDK for Unity is compatible with officially supported versions of Unity 2020.3 LTS, 2021.3 LTS and 2022.3 LTS for Windows and Mac OS, and supports both Unity's .NET Framework and .NET Standard profiles.

## Prerequisites
1. The Amazon GameLift Servers SDK for Unity depends on some third party DLLs that can be managed by Unity via a third party scoped registry called [UnityNuGet](https://github.com/xoofx/UnityNuGet). This is the simpler and recommended way to access the dependent DLLs. However, the DLLs can be downloaded manually instead of using the scoped registry. Please follow one of the below options to set up dependencies before installing the SDK.
2. After setting up dependencies, if you see the **Assembly Version Validation** errors in the console. You need to follow these steps so that binding redirects for [strongly named assemblies](https://learn.microsoft.com/en-us/dotnet/standard/assembly/strong-named) in NuGet packages resolve correctly to paths within the Unity project:
    1. On Unity's top menu bar, select **Edit**, and then choose **Project Settings...**.
    2. Select the **Player** section and uncheck the **Assembly Version Validation** option.

### Recommended Option: Use UnityNuGet Scoped Registry
1. Launch Unity and select your project.
2. On Unity's top menu bar, select **Edit**, and then choose **Project Settings...**. Select the **Package Manager** section and expand the **Scoped Registries** group.
3. Click on the **+** button and enter the values for the [UnityNuGet](https://github.com/xoofx/UnityNuGet) scoped registry:
    ```
    Name: Unity NuGet
    Url: https://unitynuget-registry.openupm.com
    Scope(s): org.nuget
    ```

### Alternate Option: Customize Unity Package (Download DLLs using NuGet CLI)
1. Unpack the provided Amazon GameLift Servers SDK for Unity `com.amazonaws.gameliftserver.sdk-<version>.tgz` file.
2. Open `package/packages.json` in a text editor and reduce the scoped registry `dependencies` list to:
    ```json
    "dependencies": {
        "com.unity.nuget.newtonsoft-json": "3.2.1"
    }
    ```
3. Install [NuGet Command Line Interface](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference#installing-nugetexe).
4. Install [Python](https://www.python.org/downloads/).
5. Make sure NuGet and Python folders are added to the PATH environment variable on Windows or have corresponding aliases on macOS/Linux so they can be executed from anywhere.
6. Open a command/shell prompt and run the provided python script `getPackages.py` to download the dependent DLLs.
    ```shell
    python3 Scripts/getPackages.py
    ``` 
7. Move the newly created `Plugins` folder located in the `Scripts` folder onto the `package` folder of the unpacked .tgz file.

## Installation
1. Launch Unity and select your project.
2. On Unity's top menu bar, select **Window**, and then choose **Package Manager**.
3. In **Package Manager**, select **+**.

### If Using UnityNuGet Scoped Registry
1. Choose **Add package from tarball...**.
2. In the **Select packages on disk** window, select the provided Amazon GameLift Servers SDK for Unity `com.amazonaws.gameliftserver.sdk-<version>.tgz` file, and then choose **Open**.

### If Using Customized Unity Package
1. Choose **Add package from disk...**.
2. Navigate to the edited version of the `package.json` file and then choose **Open**.

## Configuring log4net for File Output
The Amazon GameLift Servers SDK for Unity uses the log4net framework to output log messages.
It is configured to output to the terminal of a server build by default, but requires some extra steps to add
file logging support. This can be added to your project by importing the provided sample inside the Amazon GameLift Servers SDK for Unity package.
1. In **Package Manager**, select **Packages: In Project** from the dropdown menu at the top, and then select the `Amazon Gamelift Server SDK` from the list of packages.
2. In the package details section, expand the **Samples** group and click on the **Import** button.
3. The `log4net.config` file and accompanying `LoggingConfiguration.cs` script that automatically executes the configuration is now set up in the project's `Assets/Samples` folder.
4. If the `log4net.config` file is to be moved to a different folder in the project, its file path in `LoggingConfiguration.cs` will need to be updated with the new path. Refer to the [log4net manual](https://logging.apache.org/log4net/release/manual/configuration.html) on configuring log4net for more information.

## Testing the server SDK install

### Server Build

To verify that the server SDK is properly installed, create a new Unity project with the example script. Export a game server build and deploy it for
testing with an Amazon GameLift Servers Anywhere fleet or managed EC2 fleet. If the game server connects to the Amazon GameLift Servers service and responds
to prompts to start a game session, you can start integrating server SDK functionality into your game project.

1. In your Unity project, right-click on the **Assets** folder (usually at the bottom of the Editor), then select **Create > C# Script (or MonoBehavior Script)** to create a script with name `ServerSDKManualTest.cs`, and replace its contents with the code from the ‘Example Script’ section.
2. Create a GameObject in your scene and attach the script: In the **Hierarchy** panel (usually on the left), right-click and select Create Empty (or another object type). With the new **GameObject** selected, go to the **Inspector** panel (usually on the right), click **Add Component**, and search for ServerSDKManualTest to attach the script you just created.
3. On Unity's top menu bar, select **File**, and then choose **Build Settings...** to open the build window:
   1. Click **Add Open Scenes** to add the current scene to your build.
   2. Select **Dedicated Server** and click **Switch Platform**.
   3. Change **Target Platform** to the platform that you want to build for.
   4. Click **Build And Run** to build and execute the server build.
4. After the above steps, you should see a terminal window popping up. However, it should output that the connection cannot be established with the GameLift Server. This is expected and you need to update the parameters in your script with the ones for your fleet.

To test your server build with Amazon GameLift Servers, you need to set up the type of hosting fleet you want and deploy the server build to it. To learn more about these steps, see [Deploying game server software](https://docs.aws.amazon.com/gamelift/latest/developerguide/gamelift-build-intro.html)
and [Setting up a hosting fleet](https://docs.aws.amazon.com/gamelift/latest/developerguide/fleets-intro.html).


### Example Script

Below is a MonoBehavior script that showcases a simple game server initialization with Amazon GameLift Servers. When you create the script, please make sure the file name is the same as the class name.

```csharp
using System.Collections.Generic;
using Aws.GameLift.Server;
using UnityEngine;

public class ServerSDKManualTest : MonoBehaviour
{    
    // This is an example of a simple integration with Amazon GameLift Servers server SDK that will make game server processes go active on Amazon GameLift Servers!
    void Start()
    {        
        // Identify port number (hard coded here for simplicity) the game server is listening on for player connections
        var listeningPort = 7777;
        
        // WebSocketUrl from RegisterHost call
        var webSocketUrl = "wss://us-west-2.api.amazongamelift.com";
        
        // Unique identifier for this process
        var processId = "myProcess";
        
        // Unique identifier for your host that this process belongs to
        var hostId = "myHost";
        
        // Unique identifier for your fleet that this host belongs to
        var fleetId = "myFleet";
        
        // Authorization token for this host process
        var authToken = "myAuthToken";

        ServerParameters serverParameters = new ServerParameters(
            webSocketUrl,
            processId,
            hostId,
            fleetId,
            authToken);
        
        // InitSDK will establish a local connection with Amazon GameLift Servers agent to enable further communication.
        var initSDKOutcome = GameLiftServerAPI.InitSDK(serverParameters);        
        if (initSDKOutcome.Success)
        {
            ProcessParameters processParameters = new ProcessParameters(
                (gameSession) => {
                    // When a game session is created, Amazon GameLift Servers sends an activation request to the game server and passes along the game session object containing game properties and other settings.
                    // Here is where a game server should take action based on the game session object.
                    // Once the game server is ready to receive incoming player connections, it should invoke GameLiftServerAPI.ActivateGameSession()
                    GameLiftServerAPI.ActivateGameSession();
                },
                (updateGameSession) => {
                    // When a game session is updated (e.g. by FlexMatch backfill), Amazon GameLift Servers sends a request to the game
                    // server containing the updated game session object. The game server can then examine the provided
                    // matchmakerData and handle new incoming players appropriately.
                    // updateReason is the reason this update is being supplied.
                },
                () => {
                    // OnProcessTerminate callback. Amazon GameLift Servers will invoke this callback before shutting down an instance hosting this game server.
                    // It gives this game server a chance to save its state, communicate with services, etc., before being shut down.
                    // In this case, we simply tell Amazon GameLift Servers we are indeed going to shutdown.
                    GameLiftServerAPI.ProcessEnding();
                }, 
                () => {
                    // This is the HealthCheck callback.
                    // Amazon GameLift Servers will invoke this callback every 60 seconds or so.
                    // Here, a game server might want to check the health of dependencies and such.
                    // Simply return true if healthy, false otherwise.
                    // The game server has 60 seconds to respond with its health status. Amazon GameLift Servers will default to 'false' if the game server doesn't respond in time.
                    // In this case, we're always healthy!
                    return true;
                },
                listeningPort, // This game server tells Amazon GameLift Servers that it will listen on port 7777 for incoming player connections.
                new LogParameters(new List<string>()
                {
                    // Here, the game server tells Amazon GameLift Servers what set of files to upload when the game session ends.
                    // Amazon GameLift Servers will upload everything specified here for the developers to fetch later.
                    "/local/game/logs/myserver.log"
                }));

            // Calling ProcessReady tells Amazon GameLift Servers this game server is ready to receive incoming game sessions!
            var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
            if (processReadyOutcome.Success)
            {
                print("ProcessReady success.");
            }
            else
            {
                print("ProcessReady failure : " + processReadyOutcome.Error.ToString());
            }
        }
        else
        {
            print("InitSDK failure : " + initSDKOutcome.Error.ToString());
        }
    }  

    void OnApplicationQuit()
    {
        // Make sure to call GameLiftServerAPI.Destroy() when the application quits. This resets the local connection with Amazon GameLift Servers agent.
        GameLiftServerAPI.Destroy();
    }
}
```
## Troubleshooting

### Seeing 100% CPU utilization on a server written using Unity.

Unity by default will try to maximize FPS until it hits a hardware bottleneck, if a target / limiting framerate isn't set. 
When creating server builds, this behavior can result in 100% CPU usage and thousands of frames per second. 
This can cause slow application performance, longer load times, and unexpected crashes.
It also has the potential to impact other server processes running on the same machine, causing unexpected terminations and other issues. 

To avoid this, you can [set the `Application.targetFrameRate` configuration in your server build script](https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html). This sets a bounded maximum FPS that will override the default maximum.

 

