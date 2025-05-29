// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using AmazonGameLift.Runtime;
using UnityEditor;
using UnityEngine;

namespace SampleTests.UI
{
    internal sealed class GameLiftClientSettingsOverride
    {
        private GameLiftClientSettings _settings;
        private GameLiftClientSettings _backupConfig;

        public void SetUp(TestSettings testSettings)
        {
            _settings = AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>(AssetPaths.GameLiftClientSettings);
            _backupConfig = Object.Instantiate(_settings);
            _settings.AwsRegion = testSettings.Region;
            _settings.UserPoolClientId = testSettings.UserPoolClientId;
            _settings.ApiGatewayUrl = testSettings.ApiGatewayEndpoint;
        }

        public void TearDown()
        {
            if (_settings != null)
            {
                _settings.AwsRegion = _backupConfig.AwsRegion;
                _settings.UserPoolClientId = _backupConfig.UserPoolClientId;
                _settings.ApiGatewayUrl = _backupConfig.ApiGatewayUrl;
            }
        }
    }
}
