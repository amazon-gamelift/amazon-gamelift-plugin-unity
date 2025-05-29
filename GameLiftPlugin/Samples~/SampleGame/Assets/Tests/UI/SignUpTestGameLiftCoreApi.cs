// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using CoreConfirmSignUpResponse = AmazonGameLiftPlugin.Core.UserIdentityManagement.Models.ConfirmSignUpResponse;

namespace SampleTests.UI
{
    internal sealed class SignUpTestGameLiftCoreApi : GameLiftCoreApi
    {
        private readonly IAmazonCognitoIdentityProvider _amazonCognitoIdentityProvider;
        private readonly string _userPoolId;

        public SignUpTestGameLiftCoreApi(TestSettings testSettings, GameLiftConfiguration configuration)
            : base(configuration)
        {
            _userPoolId = testSettings.UserPoolId;
            _amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(
                testSettings.AccessKey, testSettings.SecretKey, RegionEndpoint.GetBySystemName(testSettings.Region));
        }

        public override CoreConfirmSignUpResponse ConfirmSignUp(string email, string confirmationCode)
        {
            _amazonCognitoIdentityProvider.AdminConfirmSignUp(new AdminConfirmSignUpRequest
            {
                Username = email,
                UserPoolId = _userPoolId
            });
            return Response.Ok(new CoreConfirmSignUpResponse());
        }
    }
}
