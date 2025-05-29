// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsFactory 
    {
        public static AwsCredentials Create()
        {
            TextProvider textProvider = TextProviderFactory.Create();
            return new AwsCredentials(textProvider, UnityLoggerFactory.Create(textProvider));
        }
    }
}