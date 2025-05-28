// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.UserIdentityManagement
{
    [TestFixture]
    public class UserIdentityTests
    {
        [Test]
        public void SignUp_WhenIdentityProviderReturnSuccess_IsSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string password = "testPassword";
            string username = "testUsername";

            mock.Setup(x => x.SignUp(
                It.Is<AmazonGameLiftPlugin.Core.UserIdentityManagement.Models.SignUpRequest>(
                        p => p.ClientId == clientId &&
                        p.Password == password &&
                        p.Username == username
                    )
                )).Returns(new Core.UserIdentityManagement.Models.SignUpResponse());

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignUpResponse response =
                sut.SignUp(new Core.UserIdentityManagement.Models.SignUpRequest
                {
                    ClientId = clientId,
                    Password = password,
                    Username = username
                });

            mock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void SignUp_WhenIdentityProviderThrows_IsNotSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string password = "testPassword";
            string username = "testUsername";

            mock.Setup(x => x.SignUp(
                It.Is<AmazonGameLiftPlugin.Core.UserIdentityManagement.Models.SignUpRequest>(
                        p => p.ClientId == clientId &&
                        p.Password == password &&
                        p.Username == username
                    )
                )).Throws(new AmazonCognitoIdentityProviderException("Failure"));

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignUpResponse response =
                sut.SignUp(new Core.UserIdentityManagement.Models.SignUpRequest
                {
                    ClientId = clientId,
                    Password = password,
                    Username = username
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
            Assert.AreEqual("Failure", response.ErrorMessage);
        }

        [Test]
        public void ConfirmSignUp_WhenIdentityProviderReturnSuccess_IsSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string code = "testCode";
            string username = "testUsername";

            mock.Setup(x => x.ConfirmSignUp(
                It.Is<Core.UserIdentityManagement.Models.ConfirmSignUpRequest>(
                        p => p.ClientId == clientId &&
                        p.ConfirmationCode == code &&
                        p.Username == username
                    )
                )).Returns(new Core.UserIdentityManagement.Models.ConfirmSignUpResponse());

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.ConfirmSignUpResponse response =
                sut.ConfirmSignUp(new Core.UserIdentityManagement.Models.ConfirmSignUpRequest
                {
                    ClientId = clientId,
                    ConfirmationCode = code,
                    Username = username
                });

            mock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void ConfirmSignUp_WhenIdentityProviderThrows_IsNotSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string code = "testCode";
            string username = "testUsername";

            mock.Setup(x => x.ConfirmSignUp(
                It.Is<Core.UserIdentityManagement.Models.ConfirmSignUpRequest>(
                        p => p.ClientId == clientId &&
                        p.ConfirmationCode == code &&
                        p.Username == username
                    )
                )).Throws(new AmazonCognitoIdentityProviderException("Failure"));

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.ConfirmSignUpResponse response =
                sut.ConfirmSignUp(new Core.UserIdentityManagement.Models.ConfirmSignUpRequest
                {
                    ClientId = clientId,
                    ConfirmationCode = code,
                    Username = username
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
            Assert.AreEqual("Failure", response.ErrorMessage);
        }

        [Test]
        public void SignIn_WhenIdentityProviderReturnSuccess_IsSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string password = "testPassword";
            string username = "testUsername";

            mock.Setup(x => x.InitiateAuth(It.Is<InitiateAuthRequest>
                (
                    p => p.ClientId == clientId &&
                    p.AuthFlow == AuthFlowType.USER_PASSWORD_AUTH &&
                    p.AuthParameters.ContainsValue(username) &&
                    p.AuthParameters.ContainsValue(password)
                )
                )).Returns(new InitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType
                    {
                        AccessToken = "testToken",
                        RefreshToken = "testRefreshToken",
                        IdToken = "testIdToken"
                    }
                });

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignInResponse response =
                sut.SignIn(new Core.UserIdentityManagement.Models.SignInRequest
                {
                    ClientId = clientId,
                    Password = password,
                    Username = username
                });

            mock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual("testToken", response.AccessToken);
            Assert.AreEqual("testRefreshToken", response.RefreshToken);
            Assert.AreEqual("testIdToken", response.IdToken);
        }

        [Test]
        public void SignIn_WhenIdentityProviderThrows_IsNotSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string clientId = "testClientId";
            string password = "testPassword";
            string username = "testUsername";

            mock.Setup(x => x.InitiateAuth(It.Is<InitiateAuthRequest>
                (
                    p => p.ClientId == clientId &&
                    p.AuthFlow == AuthFlowType.USER_PASSWORD_AUTH &&
                    p.AuthParameters.ContainsValue(username) &&
                    p.AuthParameters.ContainsValue(password)
                )
                )).Throws(new UserNotConfirmedException("User is not Confirmed."));

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignInResponse response =
                sut.SignIn(new Core.UserIdentityManagement.Models.SignInRequest
                {
                    ClientId = clientId,
                    Password = password,
                    Username = username
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UserNotConfirmed, response.ErrorCode);
            Assert.AreEqual("User is not Confirmed.", response.ErrorMessage);
        }

        [Test]
        public void SignOut_WhenIdentityProviderReturnSuccess_IsSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string accessToken = "testAccessToken";

            mock.Setup(x => x.SignOut(It.Is<GlobalSignOutRequest>
                (
                    p => p.AccessToken == accessToken
                ))).Returns(new GlobalSignOutResponse());

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignOutResponse response =
                sut.SignOut(new Core.UserIdentityManagement.Models.SignOutRequest
                {
                    AccessToken = accessToken
                });

            mock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void SignOut_WhenIdentityProviderThrows_IsNotSuccessful()
        {
            var mock = new Mock<IAmazonCognitoIdentityWrapper>();

            string accessToken = "testAccessToken";

            mock.Setup(x => x.SignOut(It.Is<GlobalSignOutRequest>
                (
                    p => p.AccessToken == accessToken
                )))
                .Throws(new AmazonCognitoIdentityProviderException("Failure"));

            var sut = new UserIdentity(mock.Object);

            Core.UserIdentityManagement.Models.SignOutResponse response =
                sut.SignOut(new Core.UserIdentityManagement.Models.SignOutRequest
                {
                    AccessToken = accessToken
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
            Assert.AreEqual("Failure", response.ErrorMessage);
        }
    }
}
