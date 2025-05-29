// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class DescribeChangeSetRequest
    {
        public string StackName { get; set; }
        public string ChangeSetName { get; set; }
    }

    public class DescribeChangeSetResponse : Response
    {
        public string StackId { get; set; }
        public string ChangeSetId { get; set; }
        public string ExecutionStatus { get; set; }
        public IEnumerable<Change> Changes { get; set; }
    }

    public class Change
    {
        public string Action { get; set; }

        public string LogicalId { get; set; }

        public string PhysicalId { get; set; }

        public string ResourceType { get; set; }

        public string Replacement { get; set; }

        public string Module { get; set; }
    }
}
