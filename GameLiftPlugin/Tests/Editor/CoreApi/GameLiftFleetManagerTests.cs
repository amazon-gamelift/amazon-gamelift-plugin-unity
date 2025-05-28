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
using CreateFleetResponse = Amazon.GameLift.Model.CreateFleetResponse;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    [TestFixture]
    public class GameLiftFleetManagerTests
    {
        private Mock<IAmazonGameLiftWrapper> _gameLiftWrapperMock;
        private Mock<IAmazonGameLiftWrapperFactory> _amazonGameLiftClientFactoryMock;
        
        [SetUp]
        public void Setup()
        {
            _gameLiftWrapperMock = new Mock<IAmazonGameLiftWrapper>();
            _amazonGameLiftClientFactoryMock = new Mock<IAmazonGameLiftWrapperFactory>();
        }

        private GameLiftFleetManager ArrangeAnywhereFleetHappyPath()
        {
            var listLocationModel = new List<LocationModel>();
            listLocationModel.Add(new LocationModel
            {
                LocationName = "custom-location-1"
            });

            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>())).Returns(Task.FromResult(new ListLocationsResponse {Locations = listLocationModel}));
            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(Task.FromResult(
                new CreateFleetResponse
                {
                    FleetAttributes = new FleetAttributes { FleetId = "test" }, 
                    LocationStates = new List<LocationState>()
                }));

            _amazonGameLiftClientFactoryMock.Setup(f => f.Get(It.IsAny<string>()))
                .Returns(_gameLiftWrapperMock.Object);

            return new GameLiftFleetManager(_gameLiftWrapperMock.Object);
        }

        [Test]
        public void CreateAnywhereFleet_WhenCorrectInputs_ExpectSuccess()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test", "testLocation").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            
            Assert.IsTrue(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullWrapper_DoesNotCallCreate()
        {
            //Arrange
            ArrangeAnywhereFleetHappyPath();

            var gameLiftFleetManager = new GameLiftFleetManager(null);

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test", "testLocation").GetAwaiter().GetResult();

            //Assert
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullFleetName_FleetNotCreated()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet(null, "testLocation").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateAnywhereFleet_WhenNullFleetId_FleetNotCreated()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            _gameLiftWrapperMock.Setup(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>())).Returns(
                Task.FromResult(
                    new CreateFleetResponse()
                    {
                        FleetAttributes = new FleetAttributes() { FleetId = null },
                        LocationStates = new List<LocationState>()
                    }));

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test", "testLocation").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Once);
            
            Assert.IsFalse(createFleetResult.Success);
        }

        [Test]
        public void CreateCustomLocationIfNotExists_WhenThrowErrorOnListLocation_DoesNotCallCreate()
        {
            //Arrange
            var gameLiftFleetManager = ArrangeAnywhereFleetHappyPath();

            _gameLiftWrapperMock.Setup(wrapper => wrapper.ListLocations(It.IsAny<ListLocationsRequest>()))
                .Throws(new NullReferenceException());

            //Act
            var createFleetResult = gameLiftFleetManager.CreateFleet("test", "testLocation").GetAwaiter().GetResult();

            //Assert
            _gameLiftWrapperMock.Verify(wrapper => wrapper.CreateFleet(It.IsAny<CreateFleetRequest>()), Times.Never);
            
            Assert.IsFalse(createFleetResult.Success);
        }
    }
}