// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SERVER

using System;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor.Compilation;
    using UnityEditor;
#endif

namespace AmazonGameLift.Runtime
{
    // GameLiftPackageInfo will serialize the GameLift Unity plugin name and version 
    // whenever code is recompiled in the Editor. During runtime, the plugin can 
    // report this information to GameLift Server analytics to help with debugging.   
    public class GameLiftPackageInfo : ScriptableObject
    {
        [HideInInspector] public string pluginName = "";
        [HideInInspector] public string version = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetEnvironmentVariables()
        {
            GameLiftPackageInfo packageInfo = Resources.Load<GameLiftPackageInfo>(nameof(GameLiftPackageInfo));
            if (packageInfo == null)
            {
                Debug.LogWarning("GameLiftPackageInfo could not be found. GameLift package version won't be reported for debugging. Please report this to the developer.");
                return;
            }

            System.Environment.SetEnvironmentVariable("GAMELIFT_SDK_TOOL_NAME", packageInfo.pluginName, EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable("GAMELIFT_SDK_TOOL_VERSION", packageInfo.version, EnvironmentVariableTarget.Process);
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
                System.Reflection.Assembly assembly = typeof(GameLiftPackageInfo).Assembly;
                UnityEditor.PackageManager.PackageInfo gameLiftPackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
                
                // Create Resources/GameLiftCSharpSdkPluginInfo.asset if one doesn't already exist 
                GameLiftPackageInfo packageInfoAsset = AssetDatabase.LoadAssetAtPath<GameLiftPackageInfo>($"{gameLiftPackageInfo.assetPath}/Resources/{nameof(GameLiftPackageInfo)}.asset");
                if (packageInfoAsset == null) 
                {
                    packageInfoAsset = ScriptableObject.CreateInstance<GameLiftPackageInfo>();
                    packageInfoAsset.hideFlags = HideFlags.NotEditable;

                    if (!AssetDatabase.IsValidFolder($"{gameLiftPackageInfo.assetPath}/Resources"))
                    {
                        AssetDatabase.CreateFolder(gameLiftPackageInfo.assetPath, "Resources");
                    }
                    
                    AssetDatabase.CreateAsset(packageInfoAsset, $"{gameLiftPackageInfo.assetPath}/Resources/{nameof(GameLiftPackageInfo)}.asset"); 
                }
                
                // Save plugin name and version
                string newPluginName = "Unity3D_" + gameLiftPackageInfo.name;  // Example: Unity3D_com.amazonaws.gamelift
                string newVersion = gameLiftPackageInfo.version;

                if (packageInfoAsset.pluginName != newPluginName || packageInfoAsset.version != newVersion)
                {
                    packageInfoAsset.pluginName = newPluginName;
                    packageInfoAsset.version = newVersion;
                    EditorUtility.SetDirty(packageInfoAsset);
                    AssetDatabase.SaveAssets();
                }
            }
        #endif
    }
}
#endif // UNITY_SERVER
