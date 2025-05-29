// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
        [Test]
        public void GetLatencies_WhenEndpointIsAccessible_CalculatesCorrectAverageLatencyAndReturns()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            string endpoint = "ec2.us-east-1.amazonaws.com";

            int expectedAverageLatency = 3;

            //Simulate multiple ping to the same address
            pingWrapperMock.SetupSequence(x => x.SendPingAsync(endpoint))
                .ReturnsAsync(new PingResult
                {
                    RoundtripTime = 1
                })
                .ReturnsAsync(new PingResult
                {
                    RoundtripTime = 2
                })
                .ReturnsAsync(new PingResult
                {
                    RoundtripTime = 3
                })
                .ReturnsAsync(new PingResult
                {
                    RoundtripTime = 4
                })
                .ReturnsAsync(new PingResult
                {
                    RoundtripTime = 5
                });

            var sut = new LatencyService(pingWrapperMock.Object);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string>()
                    {
                        "us-east-1"
                    }
                }).Result;

            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.RegionLatencies.Count);
            Assert.IsTrue(response.RegionLatencies.ContainsKey("us-east-1"));
            Assert.AreEqual(expectedAverageLatency, response.RegionLatencies["us-east-1"]);
        }

        [Test]
        public void GetLatencies_WhenRegionIsNull_IsNotSuccessful()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            var sut = new LatencyService(pingWrapperMock.Object);

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

            var sut = new LatencyService(pingWrapperMock.Object);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string>()
                }).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void GetLatencies_WhenPingThrows_IsNotSuccessful()
        {
            var pingWrapperMock = new Mock<IPingWrapper>();

            pingWrapperMock.Setup(x => x.SendPingAsync(It.IsAny<string>()))
                .Throws(new Exception());

            var sut = new LatencyService(pingWrapperMock.Object);

            GetLatenciesResponse response =
                sut.GetLatencies(new GetLatenciesRequest
                {
                    Regions = new List<string> { "us-east-1" }
                }).Result;

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
        }
    }
}
