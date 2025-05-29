// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class CancelDeploymentRequest
    {
        public string StackName { get; set; }

        public string ClientRequestToken { get; set; }
    }

    public class CancelDeploymentResponse : Response
    {
    }
}
