// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;

namespace AmazonGameLift.Editor
{
    internal class BucketLifecyclePolicyTextProvider
    {
        private readonly TextProvider _textProvider;
        private string[] _cachedTexts;

        internal BucketLifecyclePolicyTextProvider(TextProvider textProvider) => _textProvider = textProvider;

        public virtual IEnumerable<string> GetAllLifecyclePolicies()
        {
            if (_cachedTexts == null)
            {
                _cachedTexts = GetPolicyTexts().ToArray();
            }

            return _cachedTexts;
        }

        private IEnumerable<string> GetPolicyTexts()
        {
            var bucketPolicies = (BucketPolicy[])Enum.GetValues(typeof(BucketPolicy));

            foreach (BucketPolicy policy in bucketPolicies)
            {
                switch (policy)
                {
                    case BucketPolicy.None:
                        yield return _textProvider.Get(Strings.LifecycleNone);
                        break;
                    case BucketPolicy.SevenDaysLifecycle:
                        yield return _textProvider.Get(Strings.LifecycleSevenDays);
                        break;
                    case BucketPolicy.ThirtyDaysLifecycle:
                        yield return _textProvider.Get(Strings.LifecycleThirtyDays);
                        break;
                    default:
                        yield return policy.ToString();
                        break;
                }
            }
        }
    }
}
