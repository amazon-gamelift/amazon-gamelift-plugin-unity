// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement
{
    public interface IUserIdentity
    {
        SignUpResponse SignUp(SignUpRequest request);

        ConfirmSignUpResponse ConfirmSignUp(ConfirmSignUpRequest request);

        SignInResponse SignIn(SignInRequest request);

        SignOutResponse SignOut(SignOutRequest request);

        RefreshTokenResponse RefreshToken(RefreshTokenRequest request);
    }
}
