// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.SecurityToken.Model;

namespace AmazonGameLiftPlugin.Core.AccountManagement
{
    public interface IAmazonSecurityTokenServiceClientWrapper
    {
        GetCallerIdentityResponse GetCallerIdentity(string accessKey, string secretKey);
    }
}
