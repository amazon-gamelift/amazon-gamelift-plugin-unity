// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.BucketManagement.Models
{
    public class GetBucketsRequest
    {
        public string Region { get; set; }
    }

    public class GetBucketsResponse : Response
    {
        public IEnumerable<string> Buckets { get; set; }
    }
}
