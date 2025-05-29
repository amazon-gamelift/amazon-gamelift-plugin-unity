// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;

namespace AmazonGameLiftPlugin.Core.BucketManagement.Models
{
    public class PutLifecycleConfigurationRequest
    {
        public string BucketName { get; set; }

        public BucketPolicy BucketPolicy { get; set; }
    }

    public class PutLifecycleConfigurationResponse : Response
    {
    }
}
