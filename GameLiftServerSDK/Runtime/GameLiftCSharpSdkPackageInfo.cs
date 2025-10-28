// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SERVER

using System;
using UnityEngine;
using WebSocketSharp;
#if UNITY_EDITOR
    using UnityEditor.Compilation;
    using UnityEditor;
#endif

namespace AmazonGameLift.Runtime
{
    // GameLiftCSharpSdkPackageInfo will serialize the GameLift SDK Unity plugin name and version 
    // whenever code is recompiled. During runtime, the plugin will 
    // report this information to GameLift Server analytics to help with debugging.   
    public class GameLiftCSharpSdkPackageInfo : ScriptableObject
    {
        [HideInInspector] public string pluginName = "";
        [HideInInspector] public string version = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetEnvironmentVariables()
        {
            GameLiftCSharpSdkPackageInfo pluginInfoAsset = Resources.Load<GameLiftCSharpSdkPackageInfo>(nameof(GameLiftCSharpSdkPackageInfo));

            if (pluginInfoAsset == null)
            {
                Debug.LogWarning("GameLiftCSharpSdkPackageInfo could not be found. GameLift SDK version won't be reported for debugging. Please report this to the developer.");
                return;
            }
        
            // The lightweight Unity3D SDK plugin is loaded with the standalone plugin.
            // Don't set the lightweight tool name, if tool name is already defined by the standalone plugin.
            const string environmentVariableSdkToolName = "GAMELIFT_SDK_TOOL_NAME";
            if (System.Environment.GetEnvironmentVariable(environmentVariableSdkToolName, EnvironmentVariableTarget.Process).IsNullOrEmpty())
            {
                System.Environment.SetEnvironmentVariable(environmentVariableSdkToolName, pluginInfoAsset.pluginName, EnvironmentVariableTarget.Process);
                System.Environment.SetEnvironmentVariable("GAMELIFT_SDK_TOOL_VERSION", pluginInfoAsset.version, EnvironmentVariableTarget.Process);
            }
        }
        
#if UNITY_EDITOR
            [InitializeOnLoadMethod]
            private static void Init()
            {
                CompilationPipeline.compilationFinished -= OnCompilationFinished;
                CompilationPipeline.compilationFinished += OnCompilationFinished;
            }

            private static void OnCompilationFinished(object obj)
            {
                System.Reflection.Assembly assembly = typeof(GameLiftCSharpSdkPackageInfo).Assembly;
                UnityEditor.PackageManager.PackageInfo gameLiftPackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
                
                // Create Resources/GameLiftCSharpSdkPackageInfo.asset if one doesn't already exist 
                GameLiftCSharpSdkPackageInfo packageInfoAsset = AssetDatabase.LoadAssetAtPath<GameLiftCSharpSdkPackageInfo>($"{gameLiftPackageInfo.assetPath}/Resources/{nameof(GameLiftCSharpSdkPackageInfo)}.asset");
                if (packageInfoAsset == null) 
                {
                    packageInfoAsset = ScriptableObject.CreateInstance<GameLiftCSharpSdkPackageInfo>();
                    packageInfoAsset.hideFlags = HideFlags.NotEditable;

                    if (!AssetDatabase.IsValidFolder($"{gameLiftPackageInfo.assetPath}/Resources"))
                    {
                        AssetDatabase.CreateFolder(gameLiftPackageInfo.assetPath, "Resources");
                    }
                    
                    AssetDatabase.CreateAsset(packageInfoAsset, $"{gameLiftPackageInfo.assetPath}/Resources/{nameof(GameLiftCSharpSdkPackageInfo)}.asset"); 
                }
                
                // Save plugin name and version
                string newPluginName = "Unity3D_" + gameLiftPackageInfo.name;  // Example: Unity3D_com.amazonaws.gameliftserver.sdk
                string newVersion = gameLiftPackageInfo.version;

                if (packageInfoAsset.pluginName != newPluginName || packageInfoAsset.version != newVersion)
                {
                    packageInfoAsset.pluginName = newPluginName;
                    packageInfoAsset.version = newVersion;
                    EditorUtility.SetDirty(packageInfoAsset);
                    AssetDatabase.SaveAssets();
                }
            }
#endif // UNITY_EDITOR
    }
} 
#endif // UNITY_SERVER
