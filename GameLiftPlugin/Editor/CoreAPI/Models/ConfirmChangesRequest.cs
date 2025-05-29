// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;

namespace AmazonGameLift.Editor
{
    public sealed class ConfirmChangesRequest
    {
        public string Region { get; set; }
        public string StackId { get; set; }
        public string ChangeSetId { get; set; }
        public IEnumerable<Change> Changes { get; set; }
    }
}
