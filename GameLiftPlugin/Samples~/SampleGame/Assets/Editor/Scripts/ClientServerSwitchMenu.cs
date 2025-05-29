// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;

public static class ClientServerSwitchMenu
{
    private const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string UnityServerDefine = "UNITY_SERVER";

    [MenuItem("Amazon GameLift/Sample Game/Initialize Settings", priority = 9101)]
    public static void InitializeSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootstrapScenePath, enabled: true),
            new EditorBuildSettingsScene(GameScenePath, enabled: true),
        };

        EditorSceneManager.OpenScene(BootstrapScenePath);
        new ScriptingDefineSymbolEditor(NamedBuildTarget.Server).Add(UnityServerDefine);
    }
}
