// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class DescribeStackRequest
    {
        public string StackName { get; set; }

        public IEnumerable<string> OutputKeys { get; set; }
    }

    public class DescribeStackResponse : Response
    {
        public string StackStatus { get; set; }
        public Dictionary<string, string> Outputs { get; set; }
        public string StackId { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string GameName { get; set; }
    }
}
