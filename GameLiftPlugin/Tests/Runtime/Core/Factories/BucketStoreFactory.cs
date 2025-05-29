// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.BucketManagement;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using Moq;

namespace AmazonGameLiftPlugin.Core.Tests.Factories
{
    public class BucketStoreFactory
    {
        public BucketStore CreateBucketStore(IAmazonS3Wrapper amazonS3Wrapper = default)
        {
            amazonS3Wrapper = amazonS3Wrapper ?? new Mock<IAmazonS3Wrapper>().Object;

            return new BucketStore(amazonS3Wrapper);
        }
    }
}
