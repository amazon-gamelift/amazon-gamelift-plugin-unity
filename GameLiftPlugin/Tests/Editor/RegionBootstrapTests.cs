// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class RegionBootstrapTests
    {
        [Test]
        public void CanSave_WhenRefreshAndNoCurrentRegion_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            GetSettingResponse regionResponse = Response.Fail(new GetSettingResponse());
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.ListAvailableRegions())
                .Returns(new List<string> { "test-region" });

            var underTest = new RegionBootstrap(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(underTest.CanSave);
        }

        [Test]
        public void CanSave_WhenRefreshAndHasCurrentRegion_IsTrue()
        {
            string testRegion = "eu-west-1";
            var coreApiMock = new Mock<CoreApi>();

            var regionResponse = new GetSettingResponse()
            {
                Value = testRegion
            };
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            coreApiMock.Setup(target => target.ListAvailableRegions())
                .Returns(new List<string> { testRegion });

            var underTest = new RegionBootstrap(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(underTest.CanSave);
        }

        [Test]
        public void CanSave_WhenSetInvalidRegionIndex_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();

            coreApiMock.Setup(target => target.ListAvailableRegions())
                .Returns(new List<string> { "test-region" });

            var underTest = new RegionBootstrap(coreApiMock.Object)
            {

                // Act
                RegionIndex = -2
            };

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(underTest.CanSave);
        }

        [Test]
        public void CanSave_WhenSetValidRegionIndex_IsTrue()
        {
            var coreApiMock = new Mock<CoreApi>();

            coreApiMock.Setup(target => target.ListAvailableRegions())
                .Returns(new List<string> { "test-region" });

            var underTest = new RegionBootstrap(coreApiMock.Object)
            {

                // Act
                RegionIndex = 0
            };

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(underTest.CanSave);
        }
    }
}
