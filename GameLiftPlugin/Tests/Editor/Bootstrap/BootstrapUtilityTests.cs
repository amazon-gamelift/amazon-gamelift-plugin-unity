// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BootstrapUtilityTests
    {
        [Test]
        public void GetBootstrapData_WhenBucketNotSavedAndRegionSaved_SuccessIsFalse()
        {
            const string testRegion = "test-region";
            var coreApiMock = new Mock<CoreApi>();

            GetSettingResponse profileResponse = Response.Fail(new GetSettingResponse());
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse() { Value = testRegion };
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            var underTest = new BootstrapUtility(coreApiMock.Object);

            // Act
            GetBootstrapDataResponse actualResponse = underTest.GetBootstrapData();

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(actualResponse.Success);
        }

        [Test]
        public void GetBootstrapData_WhenBucketSavedAndRegionNotSaved_SuccessIsFalse()
        {
            const string testProfile = "test-profile";
            var coreApiMock = new Mock<CoreApi>();

            var profileResponse = new GetSettingResponse() { Value = testProfile };
            profileResponse = Response.Ok(profileResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse);

            var regionResponse = new GetSettingResponse();
            regionResponse = Response.Fail(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            var underTest = new BootstrapUtility(coreApiMock.Object);

            // Act
            GetBootstrapDataResponse actualResponse = underTest.GetBootstrapData();

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(actualResponse.Success);
        }

        [Test]
        public void GetBootstrapData_WhenBucketSavedAndRegionSaved_SuccessIsTrue()
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";

            Mock<CoreApi> coreApiMock = SetUpCoreApiForSuccess(testProfile, testRegion);

            var underTest = new BootstrapUtility(coreApiMock.Object);

            // Act
            GetBootstrapDataResponse actualResponse = underTest.GetBootstrapData();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(actualResponse.Success);
        }

        [Test]
        public void GetBootstrapData_WhenBucketSavedAndRegionSaved_ProfileIsExpected()
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";

            Mock<CoreApi> coreApiMock = SetUpCoreApiForSuccess(testProfile, testRegion);

            var underTest = new BootstrapUtility(coreApiMock.Object);

            // Act
            GetBootstrapDataResponse actualResponse = underTest.GetBootstrapData();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testProfile, actualResponse.Profile);
        }

        [Test]
        public void GetBootstrapData_WhenBucketSavedAndRegionSaved_RegionIsExpected()
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";

            Mock<CoreApi> coreApiMock = SetUpCoreApiForSuccess(testProfile, testRegion);

            var underTest = new BootstrapUtility(coreApiMock.Object);

            // Act
            GetBootstrapDataResponse actualResponse = underTest.GetBootstrapData();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testRegion, actualResponse.Region);
        }

        private Mock<CoreApi> SetUpCoreApiForSuccess(string testProfile, string testRegion)
        {
            var coreApiMock = new Mock<CoreApi>();

            var profileResponse = new GetSettingResponse() { Value = testProfile };
            profileResponse = Response.Ok(profileResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentProfileName))
                .Returns(profileResponse)
                .Verifiable();

            var regionResponse = new GetSettingResponse() { Value = testRegion };
            regionResponse = Response.Ok(regionResponse);
            coreApiMock.Setup(target => target.GetSetting(SettingsKeys.CurrentRegion))
                .Returns(regionResponse)
                .Verifiable();

            return coreApiMock;
        }
    }
}
