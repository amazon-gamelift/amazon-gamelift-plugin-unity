// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using AmazonGameLift.Editor;
using AmazonGameLift.Runtime;
using UnityEditor;

namespace Editor.Scripts
{
    public static class AnywhereFleetSettingsWriter
    {
        public static void WriteServerSettings(string directory = "")
        {
#if UNITY_SERVER
            var profile = GetProfile();
            if (profile == null)
            {
                return;
            }
            var path = Path.Join(directory, GameLiftServer.ServerConfigFilePath);
            var serverSettings = new Settings<ServerSettingsKeys>(path);
            serverSettings.PutSetting(ServerSettingsKeys.CurrentRegion, profile.Region);
            serverSettings.PutSetting(ServerSettingsKeys.FleetId, profile.AnywhereFleetId);
            serverSettings.PutSetting(ServerSettingsKeys.ComputeName, profile.ComputeName);
            serverSettings.PutSetting(ServerSettingsKeys.WebSocketUrl, profile.WebSocketUrl);
#endif
        }

        public static void WriteClientSettings(string directory = "")
        {
#if !UNITY_SERVER
            var profile = GetProfile();
            if (profile == null)
            {
                return;
            }
            var path = Path.Join(directory, GameLift.ClientConfigFilePath);
            var clientSettings = new Settings<ClientSettingsKeys>(path);
            clientSettings.PutSetting(ClientSettingsKeys.CurrentRegion, profile.Region);
            clientSettings.PutSetting(ClientSettingsKeys.FleetId, profile.AnywhereFleetId);
            clientSettings.PutSetting(ClientSettingsKeys.FleetLocation, profile.AnywhereFleetLocation);
            clientSettings.PutSetting(ClientSettingsKeys.CurrentProfileName, profile.Name);
#endif
        }

        private static UserProfile GetProfile()
        {
            var settings =
                AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>("Assets/Settings/GameLiftClientSettings.asset");
            if (settings == null || !settings.IsGameLiftAnywhere)
            {
                return null;
            }

            var stateManager = new StateManager(new CoreApi());
            return stateManager.SelectedProfile;
        }
    }
}
