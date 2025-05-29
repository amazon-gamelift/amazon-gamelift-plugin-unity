// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class DescribeStackResourceRequest
    {
        public string LogicalResourceId { get; set; }

        public string StackName { get; set; }
    }

    public class DescribeStackResourceResponse : Response
    {
        public string StackId { get; set; }

        public string PhysicalResourceId { get; set; }
    }
}
