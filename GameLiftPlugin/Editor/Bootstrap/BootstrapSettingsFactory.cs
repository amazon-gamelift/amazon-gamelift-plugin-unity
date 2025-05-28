// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;

namespace AmazonGameLift.Editor
{
    internal class BootstrapSettingsFactory
    {
        public static BootstrapSettings Create(StateManager stateManager)
        {
            var allBucketLifecyclePolicies = (BucketPolicy[])Enum.GetValues(typeof(BucketPolicy));
            BucketLifecyclePolicyTextProvider bucketLifecyclePolicyTextProvider =
                BucketLifecyclePolicyTextProviderFactory.Create();
            IEnumerable<string> allBucketLifecyclePolicyNames =
                bucketLifecyclePolicyTextProvider.GetAllLifecyclePolicies();
            TextProvider textProvider = TextProviderFactory.Create();
            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            return new BootstrapSettings(allBucketLifecyclePolicies, allBucketLifecyclePolicyNames,
                textProvider, new BootstrapBucketFormatter(), logger, stateManager);
        }
    }
}