// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.BucketManagement.Models
{
    public enum BucketPolicy
    {
        None,
        SevenDaysLifecycle = 7,
        ThirtyDaysLifecycle = 30
    }
}
