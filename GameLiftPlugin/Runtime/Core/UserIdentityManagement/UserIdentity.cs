// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement
{
    public class UserIdentity : IUserIdentity
    {
        private readonly IAmazonCognitoIdentityWrapper _amazonCognitoIdentityWrapper;

        public UserIdentity(IAmazonCognitoIdentityWrapper amazonCognitoIdentityWrapper)
        {
            _amazonCognitoIdentityWrapper = amazonCognitoIdentityWrapper;
        }

        public Models.ConfirmSignUpResponse ConfirmSignUp(Models.ConfirmSignUpRequest request)
        {
            try
            {
                Models.ConfirmSignUpResponse response =
                    _amazonCognitoIdentityWrapper.ConfirmSignUp(new Models.ConfirmSignUpRequest
                    {
                        ClientId = request.ClientId,
                        Username = request.Username,
                        ConfirmationCode = request.ConfirmationCode
                    });

                return Response.Ok(new Models.ConfirmSignUpResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.ConfirmSignUpResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public Models.SignUpResponse SignUp(Models.SignUpRequest request)
        {
            try
            {
                Models.SignUpResponse response =
                    _amazonCognitoIdentityWrapper.SignUp(new Models.SignUpRequest
                    {
                        ClientId = request.ClientId,
                        Username = request.Username,
                        Password = request.Password
                    });

                return Response.Ok(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.SignUpResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public SignInResponse SignIn(SignInRequest request)
        {
            try
            {
                InitiateAuthResponse response =
                    _amazonCognitoIdentityWrapper.InitiateAuth(new InitiateAuthRequest
                    {
                        AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                        ClientId = request.ClientId,
                        AuthParameters = new Dictionary<string, string>
                        {
                            { "USERNAME",request.Username },
                            { "PASSWORD",request.Password }
                        }
                    });

                AuthenticationResultType authDetails =
                    response.AuthenticationResult;

                return Response.Ok(new SignInResponse
                {
                    AccessToken = authDetails.AccessToken,
                    IdToken = authDetails.IdToken,
                    RefreshToken = authDetails.RefreshToken
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                string errorCode = ex is UserNotConfirmedException ?
                    ErrorCode.UserNotConfirmed :
                    ErrorCode.UnknownError;

                return Response.Fail(new SignInResponse
                {
                    ErrorCode = errorCode,
                    ErrorMessage = ex.Message
                });
            }
        }

        public SignOutResponse SignOut(SignOutRequest request)
        {
            try
            {
                GlobalSignOutResponse response =
                    _amazonCognitoIdentityWrapper.SignOut(new GlobalSignOutRequest
                    {
                        AccessToken = request.AccessToken
                    });

                return Response.Ok(new SignOutResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new SignOutResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public RefreshTokenResponse RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                InitiateAuthResponse response =
                _amazonCognitoIdentityWrapper.InitiateAuth(new InitiateAuthRequest
                {
                    ClientId = request.ClientId,
                    AuthFlow = AuthFlowType.REFRESH_TOKEN,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "REFRESH_TOKEN", request.RefreshToken }
                    }
                });

                return Response.Ok(new RefreshTokenResponse
                {
                    IdToken = response.AuthenticationResult.IdToken
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new RefreshTokenResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
