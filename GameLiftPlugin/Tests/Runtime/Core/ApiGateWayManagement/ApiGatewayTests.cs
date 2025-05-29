// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.ApiGateWayManagement
{
    [TestFixture]
    public class ApiGatewayTests
    {
        [Test]
        public void GetGameConnection_WhenValidParametersIsPassed_IsSuccessful()
        {
            var request = new GetGameConnectionRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "RefreshedIdToken"));

            httpClientMock.Setup(x =>
            x.Post(request.ApiGatewayEndpoint, "RefreshedIdToken", "get_game_connection", null))
                .ReturnsAsync((HttpStatusCode.OK, @"{""port"":""1"",""ipAddress"":""1.1"", ""playerSessionId"": ""psess-123""}"));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            GetGameConnectionResponse response = sut.GetGameConnection(request).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual("1", response.Port);
            Assert.AreEqual("1.1", response.IpAddress);
            Assert.AreEqual("psess-123", response.PlayerSessionId);
            Assert.AreEqual("RefreshedIdToken", response.IdToken);
            tokenHandler.Verify();
            httpClientMock.Verify();
        }

        [Test]
        public void GetGameConnection_WhenLambdaReturnsNotReady_ReturnsNotReady()
        {
            var request = new GetGameConnectionRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "RefreshedIdToken"));

            httpClientMock.Setup(
                x => x.Post(request.ApiGatewayEndpoint, "RefreshedIdToken", "get_game_connection", null))
                .ReturnsAsync((HttpStatusCode.NoContent, ""));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            GetGameConnectionResponse response = sut.GetGameConnection(request).Result;

            Assert.IsTrue(response.Success);
            Assert.IsFalse(response.Ready);
            tokenHandler.Verify();
            httpClientMock.Verify();
        }

        [TestCase("", "NonEmpty", "NonEmpty", "NonEmpty")]
        [TestCase("NonEmpty", "", "NonEmpty", "NonEmpty")]
        [TestCase("NonEmpty", "NonEmpty", "", "NonEmpty")]
        [TestCase("NonEmpty", "NonEmpty", "NonEmpty", "")]
        public  void GetGameConnection_WhenInValidParametersIsPassed_IsNotSuccessful(
                string apiGatewayEndpoint,
                string clientId,
                string idToken,
                string refreshToken
            )
        {
            var request = new GetGameConnectionRequest
            {
                ApiGatewayEndpoint = apiGatewayEndpoint,
                ClientId = clientId,
                IdToken = idToken,
                RefreshToken = refreshToken
            };

            var httpClientMock = new Mock<IHttpClientWrapper>();
            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            GetGameConnectionResponse response = sut.GetGameConnection(request).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void GetGameConnection_WhenIdTokenIsInvalid_IsNotSuccessful()
        {
            var request = new GetGameConnectionRequest
            {
                ApiGatewayEndpoint = "NonEmpty",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((false, ErrorCode.InvalidIdToken, string.Empty));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            GetGameConnectionResponse response = sut.GetGameConnection(request).Result;

            tokenHandler.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidIdToken, response.ErrorCode);
        }

        [Test]
        public void GetGameConnection_WhenApiGatewayReturnsFailure_IsNotSuccessful()
        {
            var request = new GetGameConnectionRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "nonEmpty"));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );


            GetGameConnectionResponse response = sut.GetGameConnection(request).Result;

            Assert.IsFalse(response.Success);
            tokenHandler.Verify();
        }

        [Test]
        public void StartGame_WhenValidParametersIsPassed_IsSuccessful()
        {
            var request = new StartGameRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "RefreshedIdToken"));

            httpClientMock.Setup(x => x.Post(request.ApiGatewayEndpoint, "RefreshedIdToken", "start_game", null))
                .ReturnsAsync((HttpStatusCode.Accepted, ""));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            StartGameResponse response = sut.StartGame(request).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual("RefreshedIdToken", response.IdToken);
            tokenHandler.Verify();
            httpClientMock.Verify();
        }

        [TestCase("", "NonEmpty", "NonEmpty", "NonEmpty")]
        [TestCase("NonEmpty", "", "NonEmpty", "NonEmpty")]
        [TestCase("NonEmpty", "NonEmpty", "", "NonEmpty")]
        [TestCase("NonEmpty", "NonEmpty", "NonEmpty", "")]
        public void StartGame_WhenInValidParametersIsPassed_IsNotSuccessful(
                string apiGatewayEndpoint,
                string clientId,
                string idToken,
                string refreshToken
            )
        {
            var request = new StartGameRequest
            {
                ApiGatewayEndpoint = apiGatewayEndpoint,
                ClientId = clientId,
                IdToken = idToken,
                RefreshToken = refreshToken
            };

            var httpClientMock = new Mock<IHttpClientWrapper>();
            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            StartGameResponse response = sut.StartGame(request).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void StartGame_WhenIdTokenIsInvalid_IsNotSuccessful()
        {
            var request = new StartGameRequest
            {
                ApiGatewayEndpoint = "NonEmpty",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((false, ErrorCode.InvalidIdToken, string.Empty));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            StartGameResponse response = sut.StartGame(request).Result;

            tokenHandler.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidIdToken, response.ErrorCode);
        }

        [Test]
        public void StartGame_WhenApiGatewayReturnsFailure_IsNotSuccessful()
        {
            var request = new StartGameRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "nonEmpty"));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );


            StartGameResponse response = sut.StartGame(request).Result;

            Assert.IsFalse(response.Success);
            tokenHandler.Verify();
        }

        [Test]
        public void StartGame_WhenApiGatewayReturnsConflict_IsNotSuccessfulAndCodeConflict()
        {
            var request = new StartGameRequest
            {
                ApiGatewayEndpoint = "http://test.com/stage",
                ClientId = "NonEmpty",
                IdToken = "NonEmpty",
                RefreshToken = "NonEmpty"
            };

            var userIdentity = new Mock<IUserIdentity>();
            var tokenHandler = new Mock<IJwtTokenExpirationCheck>();
            var httpClientMock = new Mock<IHttpClientWrapper>();

            httpClientMock.Setup(x => x.Post(request.ApiGatewayEndpoint, "RefreshedIdToken", "start_game", null))
                .ReturnsAsync((HttpStatusCode.Conflict, ""));

            tokenHandler.Setup(x => x.RefreshTokenIfExpired(request, userIdentity.Object))
                .Returns((true, string.Empty, "RefreshedIdToken"));

            var sut = new ApiGateway(
                    userIdentity.Object,
                    tokenHandler.Object,
                    httpClientMock.Object
                );

            StartGameResponse response = sut.StartGame(request).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.ConflictError, response.ErrorCode);
            tokenHandler.Verify();
        }
    }
}
