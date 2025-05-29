// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;

namespace AmazonGameLift.Editor
{
    public readonly struct DeploymentInfo
    {
        public string Region { get; }
        public string GameName { get; }
        public string ScenarioDisplayName { get; }
        public DateTime LastUpdatedTime { get; }
        public string StackStatus { get; }
        public Dictionary<string, string> Outputs { get; }

        public DeploymentInfo(DeploymentId deploymentId, DescribeStackResponse describeResponse, string scenarioDisplayName)
        {
            if (describeResponse is null)
            {
                throw new ArgumentNullException(nameof(describeResponse));
            }

            ScenarioDisplayName = scenarioDisplayName;
            Region = deploymentId.Region;

            GameName = describeResponse.GameName;
            LastUpdatedTime = describeResponse.LastUpdatedTime;
            StackStatus = describeResponse.StackStatus;
            Outputs = describeResponse.Outputs;
        }

        public DeploymentInfo(string region, string gameName, string scenarioDisplayName,
            DateTime lastUpdatedTime, string stackStatus, Dictionary<string, string> outputs)
        {
            ScenarioDisplayName = scenarioDisplayName;
            Region = region;
            GameName = gameName;
            LastUpdatedTime = lastUpdatedTime;
            StackStatus = stackStatus;
            Outputs = outputs;
        }
    }
}
