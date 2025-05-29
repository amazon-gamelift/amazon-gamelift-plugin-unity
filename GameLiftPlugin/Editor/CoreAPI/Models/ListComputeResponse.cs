// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Amazon.GameLift.Model;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class ListComputeResponse : Response
    {
        public List<Compute> ComputeList { get; set; }
    }
}