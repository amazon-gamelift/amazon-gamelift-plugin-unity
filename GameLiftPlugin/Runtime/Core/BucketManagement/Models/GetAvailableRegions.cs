// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.BucketManagement.Models
{
    public class GetAvailableRegionsRequest
    {
    }

    public class GetAvailableRegionsResponse : Response
    {
        public IEnumerable<string> Regions { get; set; }
    }
}
