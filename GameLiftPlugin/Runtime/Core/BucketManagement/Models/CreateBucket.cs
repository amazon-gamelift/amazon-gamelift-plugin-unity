// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.BucketManagement.Models
{
    public class CreateBucketRequest
    {
        public string AccountId { get; set; }
        
        public string BucketName { get; set; }

        public string Region { get; set; }
    }

    public class CreateBucketResponse : Response
    {
    }
}
