// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if !UNITY_SERVER

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using Moq.Language;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SampleTests.Unit
{
    public sealed class GameLiftClientTests
    {
        private readonly ClientCredentials _credentials = new ClientCredentials
        {
            RefreshToken = "RefreshToken",
            AccessToken = "AccessToken",
            IdToken = "IdToken"
        };

        private readonly GameLiftConfiguration _gameLiftConfiguration = new GameLiftConfiguration
        {
            ApiGatewayEndpoint = "TestApiGatewayEndpoint",
            AwsRegion = "eu-central-1",
            UserPoolClientId = "TestUserPoolClientId",
        };

        private readonly Logger _logger = new Mock<Logger>().Object;

        [UnityTest]
        public IEnumerator GetConnectionInfo_WhenStartGameFails_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var delayMock = new Mock<Delay>();
                var coreApiMock = new Mock<GameLiftCoreApi>(_gameLiftConfiguration);
                SetUpCoreApiMockStartGame(coreApiMock, success: false);
                SetUpCoreApiMockGetLatencies(coreApiMock, success: true);

                var underTest = new GameLiftClient(coreApiMock.Object, delayMock.Object, _logger)
                {
                    ClientCredentials = _credentials
                };

                // Act
                (bool success, ConnectionInfo _) = await underTest.GetConnectionInfo();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(success);
            }
        }

        [UnityTest]
        public IEnumerator GetConnectionInfo_WhenStartGameSucceedsAndGetGameConnectionSucceedsAndNotReady_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var delayMock = new Mock<Delay>();
                SetUpDelayMockWait(delayMock);

                var coreApiMock = new Mock<GameLiftCoreApi>(_gameLiftConfiguration);
                SetUpCoreApiMockStartGame(coreApiMock, success: true);
                SetUpCoreApiMockGetLatencies(coreApiMock, success: true);

                GetGameConnectionResponse connectionResponse = Response.Ok(new GetGameConnectionResponse
                {
                    IdToken = _credentials.IdToken,
                });
                coreApiMock.Setup(target => target.GetGameConnection(_credentials.IdToken, _credentials.RefreshToken))
                    .Returns(Task.FromResult(connectionResponse))
                    .Verifiable();

                var underTest = new GameLiftClient(coreApiMock.Object, delayMock.Object, _logger)
                {
                    ClientCredentials = _credentials
                };

                // Act
                (bool success, ConnectionInfo connectionInfo) = await underTest.GetConnectionInfo();

                // Assert
                coreApiMock.Verify();
                delayMock.Verify();
                Assert.IsFalse(success);
            }
        }

        private static readonly TestCaseData[] s_getConnectionInfoTestCases = new[]
        {
            new TestCaseData(0, true).Returns(null),
            new TestCaseData(1, true).Returns(null),
            new TestCaseData(2, true).Returns(null),
            new TestCaseData(3, true).Returns(null),
            new TestCaseData(4, true).Returns(null),
            new TestCaseData(5, true).Returns(null),
            new TestCaseData(6, true).Returns(null),
            new TestCaseData(7, false).Returns(null),
            new TestCaseData(8, false).Returns(null),
            new TestCaseData(10, false).Returns(null),
        };

        [TestCaseSource(nameof(s_getConnectionInfoTestCases))]
        [UnityTest]
        public IEnumerator GetConnectionInfo_WhenGetGameConnectionSucceedsAndReadyAfterNTries_SuccessIsExpected(int failCount, bool success)
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testIp = "testIp";
                const string testPort = "8080";
                const string testPlayerSessionId = "psess-123";

                var delayMock = new Mock<Delay>();
                SetUpDelayMockWait(delayMock);

                var coreApiMock = new Mock<GameLiftCoreApi>(_gameLiftConfiguration);
                SetUpCoreApiMockStartGame(coreApiMock, success: true);
                SetUpCoreApiMockGetLatencies(coreApiMock, success: true);

                GetGameConnectionResponse connectionResponse = Response.Ok(new GetGameConnectionResponse
                {
                    IdToken = _credentials.IdToken,
                    IpAddress = testIp,
                    Port = testPort,
                    PlayerSessionId = testPlayerSessionId,
                    Ready = false,
                });
                GetGameConnectionResponse connectionReadyResponse = Response.Ok(new GetGameConnectionResponse
                {
                    IdToken = _credentials.IdToken,
                    IpAddress = testIp,
                    Port = testPort,
                    PlayerSessionId = testPlayerSessionId,
                    Ready = true,
                });

                ISetupSequentialResult<Task<GetGameConnectionResponse>> sequence = coreApiMock
                    .SetupSequence(target => target.GetGameConnection(_credentials.IdToken, _credentials.RefreshToken));

                for (int i = 0; i < failCount; i++)
                {
                    sequence = sequence.Returns(Task.FromResult(connectionResponse));
                }

                sequence = sequence.Returns(Task.FromResult(connectionReadyResponse));

                var underTest = new GameLiftClient(coreApiMock.Object, delayMock.Object, _logger)
                {
                    ClientCredentials = _credentials
                };

                // Act
                (bool success, ConnectionInfo connectionInfo) info = await underTest.GetConnectionInfo();

                // Assert
                coreApiMock.Verify();
                delayMock.Verify();
                Assert.AreEqual(success, info.success);

                if (success)
                {
                    Assert.AreEqual(testIp, info.connectionInfo.IpAddress);
                    Assert.AreEqual(testPort, info.connectionInfo.Port.ToString());
                    Assert.AreEqual(testPlayerSessionId, info.connectionInfo.PlayerSessionId);
                }
            }
        }

        [UnityTest]
        public IEnumerator GetConnectionInfo_WhenGetGameConnectionSucceedsAndReady_DnsNameExpected()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const string testDns = "test.dns";
                const string testIp = "testIp";
                const string testPort = "8080";

                var delayMock = new Mock<Delay>();
                SetUpDelayMockWait(delayMock);

                var coreApiMock = new Mock<GameLiftCoreApi>(_gameLiftConfiguration);
                SetUpCoreApiMockStartGame(coreApiMock, success: true);
                SetUpCoreApiMockGetLatencies(coreApiMock, success: true);

                GetGameConnectionResponse connectionReadyResponse = Response.Ok(new GetGameConnectionResponse
                {
                    IdToken = _credentials.IdToken,
                    DnsName = testDns,
                    IpAddress = testIp,
                    Port = testPort,
                    Ready = true,
                });

                coreApiMock.Setup(target => target.GetGameConnection(_credentials.IdToken, _credentials.RefreshToken))
                    .Returns(Task.FromResult(connectionReadyResponse))
                    .Verifiable();

                var underTest = new GameLiftClient(coreApiMock.Object, delayMock.Object, _logger)
                {
                    ClientCredentials = _credentials
                };

                // Act
                (bool success, ConnectionInfo connectionInfo) info = await underTest.GetConnectionInfo();

                // Assert
                coreApiMock.Verify();
                delayMock.Verify();
                Assert.AreEqual(testDns, info.connectionInfo.IpAddress);
            }
        }

        [Test]
        public void GetConnectionInfo_WhenStartGameSucceedsAndCancelled_ThrowsException()
        {
            var delayMock = new Mock<Delay>();
            SetUpDelayMockWait(delayMock);

            var coreApiMock = new Mock<GameLiftCoreApi>(_gameLiftConfiguration);
            SetUpCoreApiMockStartGame(coreApiMock, success: true);
            SetUpCoreApiMockGetLatencies(coreApiMock, success: true);

            GetGameConnectionResponse connectionResponse = Response.Ok(new GetGameConnectionResponse
            {
                IdToken = _credentials.IdToken,
            });
            coreApiMock.Setup(target => target.GetGameConnection(_credentials.IdToken, _credentials.RefreshToken))
                .Returns(Task.FromResult(connectionResponse))
                .Verifiable();

            var underTest = new GameLiftClient(coreApiMock.Object, delayMock.Object, _logger)
            {
                ClientCredentials = _credentials
            };

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            AssertAsync.ThrowsAsync<TaskCanceledException>(async () => await underTest.GetConnectionInfo(cts.Token));
        }

        private void SetUpDelayMockWait(Mock<Delay> delayMock)
        {
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        private void SetUpCoreApiMockStartGame(Mock<GameLiftCoreApi> coreApiMock, bool success)
        {
            StartGameResponse okResponse = Response.Ok(new StartGameResponse
            {
                IdToken = _credentials.IdToken
            });
            StartGameResponse startGameResponse = success ? okResponse : Response.Fail(new StartGameResponse());

            coreApiMock.Setup(target => target.StartGame(_credentials.IdToken, _credentials.RefreshToken, It.IsAny<Dictionary<string, long>>()))
                .Returns(Task.FromResult(startGameResponse))
                .Verifiable();
        }

        private void SetUpCoreApiMockGetLatencies(Mock<GameLiftCoreApi> coreApiMock, bool success)
        {
            GetLatenciesResponse okResponse = Response.Ok(new GetLatenciesResponse
            {
                RegionLatencies = new Dictionary<string, long> { }
            });
            GetLatenciesResponse startGameResponse = success ? okResponse : Response.Fail(new GetLatenciesResponse());

            coreApiMock.Setup(target => target.GetLatencies(It.IsAny<string[]>()))
                .Returns(Task.FromResult(startGameResponse))
                .Verifiable();
        }
    }
}

#endif
