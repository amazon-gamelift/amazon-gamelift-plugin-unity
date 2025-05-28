// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;

namespace AmazonGameLiftPlugin.Core.AccountManagement
{
    public class AccountManager : IAccountManager
    {
        private readonly IAmazonSecurityTokenServiceClientWrapper _tokenServiceClient;

        public AccountManager(IAmazonSecurityTokenServiceClientWrapper tokenServiceClient)
        {
            _tokenServiceClient = tokenServiceClient;
        }

        public RetrieveAccountIdByCredentialsResponse RetrieveAccountIdByCredentials(RetrieveAccountIdByCredentialsRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessKey) || string.IsNullOrEmpty(request.SecretKey))
            {
                return Response.Fail(new RetrieveAccountIdByCredentialsResponse
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            try
            {
                GetCallerIdentityResponse callerIdentityResponse = _tokenServiceClient
                    .GetCallerIdentity(request.AccessKey, request.SecretKey);

                if (callerIdentityResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Response.Ok(new RetrieveAccountIdByCredentialsResponse
                    {
                        AccountId = callerIdentityResponse.Account
                    });
                }
                else
                {
                    return Response.Fail(new RetrieveAccountIdByCredentialsResponse
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {callerIdentityResponse.HttpStatusCode}"
                    });
                }
            }
            catch (AmazonSecurityTokenServiceException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new RetrieveAccountIdByCredentialsResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
