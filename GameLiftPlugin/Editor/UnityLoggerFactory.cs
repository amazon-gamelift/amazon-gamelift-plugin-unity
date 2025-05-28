// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal static class UnityLoggerFactory
    {
        private static UnityLogger s_textProvider;

        public static UnityLogger Create(TextProvider textProvider)
        {
            if (s_textProvider == null)
            {
                s_textProvider = new UnityLogger(textProvider);
            }

            return s_textProvider;
        }
    }
}
