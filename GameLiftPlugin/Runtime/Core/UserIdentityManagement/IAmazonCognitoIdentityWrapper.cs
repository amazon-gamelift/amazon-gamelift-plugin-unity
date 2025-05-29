// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.CognitoIdentityProvider.Model;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement
{
    public interface IAmazonCognitoIdentityWrapper
    {
        Models.SignUpResponse SignUp(Models.SignUpRequest request);

        Models.ConfirmSignUpResponse ConfirmSignUp(Models.ConfirmSignUpRequest request);

        InitiateAuthResponse InitiateAuth(InitiateAuthRequest request);

        GlobalSignOutResponse SignOut(GlobalSignOutRequest request);
    }
}
