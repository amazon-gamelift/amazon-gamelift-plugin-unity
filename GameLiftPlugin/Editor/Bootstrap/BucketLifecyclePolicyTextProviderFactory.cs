// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal static class BucketLifecyclePolicyTextProviderFactory
    {
        private static BucketLifecyclePolicyTextProvider s_textProvider;

        public static BucketLifecyclePolicyTextProvider Create()
        {
            if (s_textProvider == null)
            {
                s_textProvider = new BucketLifecyclePolicyTextProvider(TextProviderFactory.Create());
            }

            return s_textProvider;
        }
    }
}
