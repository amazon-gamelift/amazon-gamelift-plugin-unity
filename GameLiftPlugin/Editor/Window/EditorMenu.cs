// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class EditorMenu
    {
        private static readonly Type _inspectorType = Type.GetType("UnityEditor.GameView,UnityEditor.dll");
        private static readonly string _filePackagePath = $"Packages/{Paths.PackageName}/{Paths.SampleGameInPackage}";

        private static GameLiftPlugin GetPluginWindow()
        {
            var window = EditorWindow.GetWindow<GameLiftPlugin>(_inspectorType);
            window.titleContent = new GUIContent("Amazon GameLift", window.Icon);
            return window;
        }

        [MenuItem("Amazon GameLift/Show Amazon GameLift Window", priority = 0)]
        public static void ShowWindow()
        {
            GetPluginWindow();
        }

        [MenuItem("Amazon GameLift/Bring Panel to Front", priority = 1)]
        public static void FocusPanel()
        {
            ShowWindow();
        }

        [MenuItem("Amazon GameLift/Set up your user profile", priority = 100)]
        public static void OpenAccountProfilesTab()
        {
            GetPluginWindow().OpenTab(GameLiftPlugin.Pages.Credentials);
        }

        [MenuItem("Amazon GameLift/Host with Anywhere", priority = 101)]
        public static void OpenAnywhereTab()
        {
            GetPluginWindow().OpenTab(GameLiftPlugin.Pages.Anywhere);
        }

        [MenuItem("Amazon GameLift/Host with Managed EC2", priority = 102)]
        public static void OpenEC2Tab()
        {
            GetPluginWindow().OpenTab(GameLiftPlugin.Pages.ManagedEC2);
        }

        [MenuItem("Amazon GameLift/Host with Managed Containers", priority = 103)]
        public static void OpenContainersTab()
        {
            GetPluginWindow().OpenTab(GameLiftPlugin.Pages.Containers);
        }

        [MenuItem("Amazon GameLift/Sample Game/Import Sample Game", priority = 104)]
        public static void ImportSampleGame()
        {
            AssetDatabase.ImportPackage(_filePackagePath, interactive: true);
        }

        [MenuItem("Amazon GameLift/Help/Documentation", priority = 200)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(Urls.AwsHelpGameLiftUnityDocumentation);
        }

        [MenuItem("Amazon GameLift/Help/AWS GameTech Forum", priority = 201)]
        public static void OpenGameTechForums()
        {
            Application.OpenURL(Urls.AwsGameTechForums);
        }

        [MenuItem("Amazon GameLift/Help/Report Issues", priority = 202)]
        public static void OpenReportIssues()
        {
            Application.OpenURL(Urls.GitHubAwsIssues);
        }
    }
}