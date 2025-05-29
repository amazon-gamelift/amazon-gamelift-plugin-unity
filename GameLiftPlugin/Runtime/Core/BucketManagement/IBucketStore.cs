// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.BucketManagement.Models;

namespace AmazonGameLiftPlugin.Core.BucketManagement
{
    public interface IBucketStore
    {
        CreateBucketResponse CreateBucket(CreateBucketRequest request);

        PutLifecycleConfigurationResponse PutLifecycleConfiguration(PutLifecycleConfigurationRequest request);

        GetBucketsResponse GetBuckets(GetBucketsRequest request);

        GetAvailableRegionsResponse GetAvailableRegions(GetAvailableRegionsRequest request);

        GetBucketPoliciesResponse GetBucketPolicies(GetBucketPoliciesRequest request);
    }
}
