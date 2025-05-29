// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    [TestFixture]
    public class GameLiftComputeManagerTests
    {
        private Mock<IAmazonGameLiftWrapper> _gameLiftWrapperMock;
        private Mock<IAmazonGameLiftWrapperFactory> _amazonGameLiftClientFactoryMock;

        private const string FleetId = "fleetId-12345-12345-12345-12345";
        private const string ComputeName = "TestComputeName";
        private const string Location = "TestLocation";
        private const string IPAddress = "127.0.0.1";
        private const string Endpoint = "wss://test.com";
        
        [SetUp]
        public void Setup()
        {
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftWrapper>();
            _amazonGameLiftClientFactoryMock = new Mock<IAmazonGameLiftWrapperFactory>();
        }

        private GameLiftComputeManager ArrangeRegisterComputeHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel
            {
                LocationName = "custom-location-1"
            });

            _gameLiftWrapperMock.Setup(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>())).Returns(Task.FromResult(
                new RegisterComputeResponse()
                {
                    Compute = new Compute()
                    {
                        FleetId = FleetId,
                        ComputeName = ComputeName,
                        Location = Location,
                        IpAddress = IPAddress,
                        GameLiftServiceSdkEndpoint = Endpoint
                    }
                }));

            _amazonGameLiftClientFactoryMock.Setup(f => f.Get(It.IsAny<string>()))
                .Returns(_gameLiftWrapperMock.Object);

            return new GameLiftComputeManager(_gameLiftWrapperMock.Object);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftComputeManager = ArrangeRegisterComputeHappyPath();
            
            //Act
            var registerComputeResponse =  gameLiftComputeManager.RegisterFleetCompute(ComputeName, FleetId, Location, IPAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            
            Assert.IsTrue(registerComputeResponse.Success);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenNullWrapper_DoesNotCallRegister()
        {
            //Arrange
            ArrangeRegisterComputeHappyPath();
        
            var gameLiftFleetManager = new GameLiftComputeManager(null);
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(ComputeName, FleetId, Location, IPAddress).GetAwaiter().GetResult();
            
            //Assert
            Assert.IsFalse(createFleetResult.Success);
        }
        
        [Test]
        public void RegisterFleetCompute_WhenNullComputeName_ComputeNotRegistered()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeRegisterComputeHappyPath();
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(null, FleetId, Location, IPAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void RegisterFleetCompute_WhenNullIpAddress_ComputeNotRegistered()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeRegisterComputeHappyPath();
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(ComputeName, FleetId, Location, null).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }
        
        
        [Test] 
        public void RegisterFleetCompute_WhenThrowErrorOnListLocation_DoesNotRegister()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeRegisterComputeHappyPath();
            
            _gameLiftWrapperMock.Setup(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>())).Throws(new NullReferenceException());
        
            //Act
            var createFleetResult =  gameLiftFleetManager.RegisterFleetCompute(ComputeName, FleetId, Location, IPAddress).GetAwaiter().GetResult();
            
            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.RegisterCompute(It.IsAny<RegisterComputeRequest>()), Times.Once);
            
            Assert.IsFalse(createFleetResult.Success);
        }
    }
}
