// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    public readonly struct DeploymentId
    {
        public string Profile { get; }
        public string Region { get; }
        public string StackName { get; }
        public string ScenarioName { get; }

        public DeploymentId(DeploymentRequest deploymentRequest, string scenarioName)
        {
            if (deploymentRequest is null)
            {
                throw new ArgumentNullException(nameof(deploymentRequest));
            }

            Profile = deploymentRequest.Profile;
            Region = deploymentRequest.Region;
            StackName = deploymentRequest.StackName;
            ScenarioName = scenarioName ?? throw new ArgumentNullException(nameof(scenarioName));
        }

        public DeploymentId(string profile, string region, string stackName, string scenarioName)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Region = region ?? throw new ArgumentNullException(nameof(region));
            StackName = stackName ?? throw new ArgumentNullException(nameof(stackName));
            ScenarioName = scenarioName ?? throw new ArgumentNullException(nameof(scenarioName));
        }
    }
}
