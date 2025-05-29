// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace AmazonGameLiftPlugin.Core.AccountManagement
{
    public class AmazonSecurityTokenServiceClientWrapper : IAmazonSecurityTokenServiceClientWrapper
    {
        public GetCallerIdentityResponse GetCallerIdentity(string accessKey, string secretKey)
        {
            var basicAWSCredentials = new BasicAWSCredentials(accessKey, secretKey);
            var stsClient = new AmazonSecurityTokenServiceClient(basicAWSCredentials);
            GetCallerIdentityResponse callerIdentityResponse = stsClient.GetCallerIdentity(
                new GetCallerIdentityRequest());

            return callerIdentityResponse;
        }
    }
}
