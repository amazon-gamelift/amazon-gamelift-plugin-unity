// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.Latency
{
    [TestFixture]
    public class LatencyServiceTests
    {
        private const int PingBeaconPort = 7770;

        // Disable the inter-sample delay so tests do not pay the rate-limit spacing.
        private static LatencyService CreateService(IPingWrapper pingWrapper, int pingCount) =>
            new LatencyService(pingWrapper, pingCount, pingIntervalMs: 0);

        [Test]
        public void GetLatencies_WhenBeaconIsAccessible_CalculatesCorrectAverageLatencyAndReturns()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            string endpoint = "gamelift-ping.us-east-1.api.aws";

            int expectedAverageLatency = 2;

            // Simulate multiple UDP ping beacon round trips to the same endpoint.
            pingWrapperMock.SetupSequence(x => x.SendPingAsync(endpoint, PingBeaconPort))
                .ReturnsAsync(new PingResult { RoundtripTime = 1 })
                .ReturnsAsync(new PingResult { RoundtripTime = 2 })
                .ReturnsAsync(new PingResult { RoundtripTime = 3 });

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "us-east-1" }
                }).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.RegionLatencies.Count);
            Assert.IsTrue(response.RegionLatencies.ContainsKey("us-east-1"));
            Assert.AreEqual(expectedAverageLatency, response.RegionLatencies["us-east-1"]);
        }

        [Test]
        public void GetLatencies_WhenSomeSamplesTimeOut_AveragesOnlySuccessfulSamples()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            string endpoint = "gamelift-ping.us-east-1.api.aws";

            pingWrapperMock.SetupSequence(x => x.SendPingAsync(endpoint, PingBeaconPort))
                .ReturnsAsync(new PingResult { RoundtripTime = 10, Success = true })
                .ReturnsAsync(new PingResult { Success = false })
                .ReturnsAsync(new PingResult { RoundtripTime = 20, Success = true });

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "us-east-1" }
                }).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual(15, response.RegionLatencies["us-east-1"]);
        }

        [Test]
        public void GetLatencies_WhenAllSamplesTimeOut_OmitsRegion()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            pingWrapperMock.Setup(x => x.SendPingAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new PingResult { Success = false });

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "us-east-1" }
                }).Result;

            Assert.IsTrue(response.Success);
            Assert.IsEmpty(response.RegionLatencies);
        }

        [Test]
        public void GetLatencies_WhenChinaRegion_UsesChinaPartitionBeaconEndpoint()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            string chinaEndpoint = "gamelift-ping.cn-north-1.api.amazonwebservices.com.cn";

            pingWrapperMock.Setup(x => x.SendPingAsync(chinaEndpoint, PingBeaconPort))
                .ReturnsAsync(new PingResult { RoundtripTime = 42, Success = true });

            var sut = CreateService(pingWrapperMock.Object, pingCount: 1);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "cn-north-1" }
                }).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual(42, response.RegionLatencies["cn-north-1"]);
            pingWrapperMock.Verify(x => x.SendPingAsync(chinaEndpoint, PingBeaconPort), Times.Once);
        }

        [Test]
        public void GetLatencies_WhenRegionIsNull_IsNotSuccessful()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = null
                }).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void GetLatencies_WhenRegionIsEmpty_IsNotSuccessful()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string>()
                }).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void GetLatencies_WhenPingThrows_OmitsRegionButSucceeds()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            pingWrapperMock.Setup(x => x.SendPingAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception());

            var sut = CreateService(pingWrapperMock.Object, pingCount: 3);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "us-east-1" }
                }).Result;

            // A failure measuring one region is isolated: that region is omitted, but
            // the overall request still succeeds with the regions that did respond.
            Assert.IsTrue(response.Success);
            Assert.IsEmpty(response.RegionLatencies);
        }
    }
}
