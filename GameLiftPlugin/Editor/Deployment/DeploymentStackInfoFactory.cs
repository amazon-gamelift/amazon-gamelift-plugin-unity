// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;

namespace AmazonGameLift.Editor
{
    internal static class DeploymentStackInfoFactory
    {
        public static DeploymentStackInfo Create(TextProvider textProvider,
           DescribeStackResponse describeResponse, string currentRegion, string scenarioName)
        {
            string updateTime = FormatTime(describeResponse.LastUpdatedTime);
            string statusFormat = textProvider.Get(Strings.StackStatusTemplate);
            string status = string.Format(statusFormat, describeResponse.StackStatus);
            string detailsFormat = textProvider.Get(Strings.StackDetailsTemplate);
            string details = string.Format(detailsFormat, currentRegion, scenarioName, describeResponse.GameName, updateTime);

            describeResponse.Outputs.TryGetValue(StackOutputKeys.ApiGatewayEndpoint, out string apiGatewayEndpoint);
            describeResponse.Outputs.TryGetValue(StackOutputKeys.UserPoolClientId, out string userPoolClientId);
            return new DeploymentStackInfo(status, describeResponse.StackStatus, details, apiGatewayEndpoint, userPoolClientId, describeResponse.StackId);
        }

        public static DeploymentStackInfo Create(TextProvider textProvider, DeploymentInfo info)
        {
            string updateTime = FormatTime(info.LastUpdatedTime);
            string statusFormat = textProvider.Get(Strings.StackStatusTemplate);
            string status = string.Format(statusFormat, info.StackStatus);
            string detailsFormat = textProvider.Get(Strings.StackDetailsTemplate);
            string details = string.Format(detailsFormat, info.Region, info.ScenarioDisplayName, info.GameName, updateTime);

            info.Outputs.TryGetValue(StackOutputKeys.ApiGatewayEndpoint, out string apiGatewayEndpoint);
            info.Outputs.TryGetValue(StackOutputKeys.UserPoolClientId, out string userPoolClientId);
            return new DeploymentStackInfo(status, info.StackStatus, details, apiGatewayEndpoint, userPoolClientId);
        }

        private static string FormatTime(DateTime time)
        {
            return time != default
                ? time.ToString("yyyy-MM-dd hh:mm tt")
                : "-";
        }
    }
}
