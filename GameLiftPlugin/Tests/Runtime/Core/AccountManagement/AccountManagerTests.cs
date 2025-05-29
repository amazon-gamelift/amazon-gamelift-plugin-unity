// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.SecurityToken;
using AmazonGameLiftPlugin.Core.AccountManagement;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.AccountManagement
{
    [TestFixture]
    public class AccountManagerTests
    {
        [Test]
        public void RetrieveAccountIdByCredentials_WhenCredentialsIsCorrect_IsSuccessful()
        {
            string secretKey = "nonEmptySecretKey";
            string accessKey = "nonemptyAccessKey";
            string account = "123";

            var mock = new Mock<IAmazonSecurityTokenServiceClientWrapper>();
            mock.Setup(x => x.GetCallerIdentity(accessKey, secretKey)).Returns(
                new Amazon.SecurityToken.Model.GetCallerIdentityResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Account = account
                });

            var accountManager = new AccountManager(mock.Object);
            RetrieveAccountIdByCredentialsResponse response = accountManager
                .RetrieveAccountIdByCredentials(new RetrieveAccountIdByCredentialsRequest
                {
                    SecretKey = secretKey,
                    AccessKey = accessKey
                });

            mock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(account, response.AccountId);
        }

        [Test]
        public void RetrieveAccountIdByCredentials_WhenExternalApiReturnsFail_IsNotSuccessful()
        {
            string secretKey = "nonEmptySecretKey";
            string accessKey = "nonemptyAccessKey";

            var mock = new Mock<IAmazonSecurityTokenServiceClientWrapper>();
            mock.Setup(x => x.GetCallerIdentity(accessKey, secretKey)).Returns(
                new Amazon.SecurityToken.Model.GetCallerIdentityResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                });

            var accountManager = new AccountManager(mock.Object);
            RetrieveAccountIdByCredentialsResponse response = accountManager
                .RetrieveAccountIdByCredentials(new RetrieveAccountIdByCredentialsRequest
                {
                    SecretKey = secretKey,
                    AccessKey = accessKey
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.AwsError, response.ErrorCode);
        }

        [Test]
        public void RetrieveAccountIdByCredentials_WhenTokenClientThrowsException_IsNotSuccessful()
        {
            string secretKey = "nonEmptySecretKey";
            string accessKey = "nonemptyAccessKey";
            string exceptionMessage = "ErrorMessage";
            var mock = new Mock<IAmazonSecurityTokenServiceClientWrapper>();
            mock.Setup(x => x.GetCallerIdentity(accessKey, secretKey))
                .Throws(new AmazonSecurityTokenServiceException(exceptionMessage));

            var accountManager = new AccountManager(mock.Object);
            RetrieveAccountIdByCredentialsResponse response = accountManager
                .RetrieveAccountIdByCredentials(new RetrieveAccountIdByCredentialsRequest
                {
                    AccessKey = accessKey,
                    SecretKey = secretKey
                });

            mock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(exceptionMessage, response.ErrorMessage);
        }

        [Test]
        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("nonempty", null)]
        [TestCase(null, "nonEmpty")]
        public void RetrieveAccountIdByCredentials_WhenCredentialsIsEmptyOrNull_IsNotSuccessful(string accessKey, string secretKey)
        {
            var mock = new Mock<IAmazonSecurityTokenServiceClientWrapper>();

            var accountManager = new AccountManager(mock.Object);
            RetrieveAccountIdByCredentialsResponse response = accountManager
                .RetrieveAccountIdByCredentials(new RetrieveAccountIdByCredentialsRequest
                {
                    AccessKey = accessKey,
                    SecretKey = secretKey
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }
    }
}
