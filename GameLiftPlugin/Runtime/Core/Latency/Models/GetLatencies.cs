// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.Latency.Models
{
    public class GetLatenciesRequest
    {
        public IEnumerable<string> Regions { get; set; }
    }

    public class GetLatenciesResponse : Response
    {
        public Dictionary<string, long> RegionLatencies { get; set; }
    }
}
