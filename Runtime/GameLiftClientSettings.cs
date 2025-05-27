// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core;
using UnityEngine;

namespace AmazonGameLift.Runtime
{
    [CreateAssetMenu(fileName = "GameLiftClientSettings", menuName = "GameLift/Client Settings")]
    public sealed class GameLiftClientSettings : ScriptableObject
    {
        public string AwsRegion;
        public string UserPoolClientId;
        public string ApiGatewayUrl;
        public bool IsGameLiftAnywhere;

        public GameLiftConfiguration GetConfiguration()
        {
            return new GameLiftConfiguration
            {
                ApiGatewayEndpoint = ApiGatewayUrl,
                AwsRegion = AwsRegion,
                UserPoolClientId = UserPoolClientId,
                IsGameLiftAnywhere = IsGameLiftAnywhere
            };
        }

        public void ConfigureAnywhereClientSettings()
        {
            IsGameLiftAnywhere = true;
            AwsRegion = "";
            ApiGatewayUrl = "";
            UserPoolClientId = "";
        }

        public void ConfigureManagedEC2ClientSettings(string awsRegion, string apiGatewayUrl, string userPoolClientId) 
        {
            IsGameLiftAnywhere = false;
            AwsRegion = awsRegion;
            ApiGatewayUrl = apiGatewayUrl;
            UserPoolClientId = userPoolClientId;
        }

        public void ConfigureContainersClientSettings(string awsRegion, string apiGatewayUrl, string userPoolClientId)
        {
            IsGameLiftAnywhere = false;
            AwsRegion = awsRegion;
            ApiGatewayUrl = apiGatewayUrl;
            UserPoolClientId = userPoolClientId;
        }
    }
}