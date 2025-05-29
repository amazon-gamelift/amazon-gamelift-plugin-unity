// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class StackExistsRequest
    {
        public string StackName { get; set; }
    }

    public class StackExistsResponse : Response
    {
        public bool Exists { get; set; }
    }
}
