// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement
{
    public class AmazonCognitoIdentityWrapper : IAmazonCognitoIdentityWrapper
    {
        private readonly IAmazonCognitoIdentityProvider _amazonCognitoIdentityProvider;
        public AmazonCognitoIdentityWrapper(string region)
        {
            _amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(
                    string.Empty, string.Empty, AwsRegionMapper.GetRegionEndpoint(region)
                );
        }

        public Models.ConfirmSignUpResponse ConfirmSignUp(Models.ConfirmSignUpRequest request)
        {
            ConfirmSignUpResponse response = _amazonCognitoIdentityProvider.ConfirmSignUp(new ConfirmSignUpRequest
            {
                ClientId = request.ClientId,
                ConfirmationCode = request.ConfirmationCode,
                Username = request.Username
            });

            return new Models.ConfirmSignUpResponse();
        }

        public InitiateAuthResponse InitiateAuth(InitiateAuthRequest request)
        {
            return _amazonCognitoIdentityProvider.InitiateAuth(request);
        }

        public Models.SignUpResponse SignUp(Models.SignUpRequest request)
        {
            SignUpResponse response = _amazonCognitoIdentityProvider.SignUp(new SignUpRequest
            {
                ClientId = request.ClientId,
                Username = request.Username,
                Password = request.Password
            });

            return new Models.SignUpResponse();
        }

        public GlobalSignOutResponse SignOut(GlobalSignOutRequest request)
        {
            return _amazonCognitoIdentityProvider.GlobalSignOut(request);
        }
    }
}
