// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEditor;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class AwsCredentialsCreationTests
    {
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        #region Refresh

        [Test]
        public void Refresh_WhenAnyProfilesAndCurrentSaved_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();
            List<string> testProfiles = coreApiMock.SetUpWithTestProfileListOf2();
            coreApiMock.SetUpCoreApiWithProfile(success: true, testProfiles[1]);

            AwsCredentialsCreation underTest = GetAwsCredentials(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.AreEqual(testProfiles[1], underTest.CurrentProfileName);
            Assert.IsNull(underTest.AccessKeyId);
            Assert.IsNull(underTest.SecretKey);
        }

        [Test]
        public void Refresh_WhenAnyProfilesAndCurrentInvalid_AllPropertiesAreUpdated()
        {
            string invalidProfile = "Invalid" + DateTime.UtcNow.ToBinary().ToString();

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpWithTestProfileListOf2();
            coreApiMock.SetUpCoreApiWithProfile(success: true, invalidProfile);

            AwsCredentialsCreation underTest = GetAwsCredentials(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.IsNull(underTest.CurrentProfileName);
            Assert.IsNull(underTest.AccessKeyId);
            Assert.IsNull(underTest.SecretKey);
        }

        [Test]
        public void Refresh_WhenAnyProfilesAndNoCurrent_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpWithTestProfileListOf2();
            coreApiMock.SetUpCoreApiWithProfile(success: false);

            AwsCredentialsCreation underTest = GetAwsCredentials(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.IsNull(underTest.CurrentProfileName);
            Assert.IsNull(underTest.AccessKeyId);
            Assert.IsNull(underTest.SecretKey);
        }

        [Test]
        public void Refresh_WhenNoProfilesAndNoCurrent_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();
            var testProfiles = new List<string>();
            var profilesResponse = new GetProfilesResponse()
            {
                Profiles = testProfiles
            };
            profilesResponse = Response.Ok(profilesResponse);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(profilesResponse)
                .Verifiable();

            AwsCredentialsCreation underTest = GetAwsCredentials(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.IsNull(underTest.CurrentProfileName);
            Assert.IsNull(underTest.AccessKeyId);
            Assert.IsNull(underTest.SecretKey);
        }

        [Test]
        public void Refresh_WhenGetProfilesError_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();

            GetProfilesResponse profilesResponse = Response.Fail(new GetProfilesResponse());
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(profilesResponse)
                .Verifiable();

            AwsCredentialsCreation underTest = GetAwsCredentials(coreApiMock.Object);

            // Act
            underTest.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.IsNull(underTest.CurrentProfileName);
            Assert.IsNull(underTest.AccessKeyId);
            Assert.IsNull(underTest.SecretKey);
        }

        #endregion

        #region CanCreate

        [Test]
        public void CanCreate_WhenNewInstance_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);

            Assert.AreEqual(false, awsCredentials.CanCreate);
        }

        [Test]
        public void CanCreate_WhenSecretKeyEmpty_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = "TestProfile";
            awsCredentials.AccessKeyId = "NonEmpty";

            Assert.AreEqual(false, awsCredentials.CanCreate);
        }

        [Test]
        public void CanCreate_WhenAccessKeyIdEmpty_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = "TestProfile";
            awsCredentials.SecretKey = "NonEmpty";

            Assert.AreEqual(false, awsCredentials.CanCreate);
        }

        [Test]
        public void CanCreate_WhenProfileNameEmpty_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.AccessKeyId = "NonEmptyId";
            awsCredentials.SecretKey = "NonEmpty";

            Assert.AreEqual(false, awsCredentials.CanCreate);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CanCreate_WhenNonEmptyFieldsAndCanSaveRegionIsParam_IsExpected(bool canSaveRegion)
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object, canSaveRegion: canSaveRegion);
            awsCredentials.ProfileName = "TestProfile";
            awsCredentials.AccessKeyId = "NonEmptyId";
            awsCredentials.SecretKey = "NonEmpty";

            Assert.AreEqual(canSaveRegion, awsCredentials.CanCreate);
        }

        #endregion

        #region Create

        [Test]
        public void Create_WhenCanCreate_SavesAwsCredentialsAndSetsCurrentProfile()
        {
            const string testProfileName = "TestProfile";
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";
            const string testRegion = "eu-west-1";

            string actualProfileName = null;
            string actualAccessKeyId = null;
            string actualSecretKey = null;
            string actualRegion = null;

            var coreApiMock = new Mock<CoreApi>();
            SaveAwsCredentialsResponse saveResponse = Response.Ok(new SaveAwsCredentialsResponse());
            coreApiMock.Setup(target => target.SaveAwsCredentials(testProfileName, testAccessKeyId, testSecretKey, testRegion))
                .Returns(saveResponse)
                .Callback<string, string, string, string>((profileName, accessKeyId, secretKey, region) =>
                {
                    actualProfileName = profileName;
                    actualAccessKeyId = accessKeyId;
                    actualSecretKey = secretKey;
                    actualRegion = region;
                })
                .Verifiable();

            PutSettingResponse writeResponse = Response.Ok(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, testProfileName))
                .Returns(writeResponse)
                .Verifiable();
            coreApiMock.Setup(target => target.ListAvailableRegions()).Returns(new List<string> { testRegion });

            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = testProfileName;
            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Create();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testProfileName, awsCredentials.CurrentProfileName);
            Assert.AreEqual(testProfileName, actualProfileName);
            Assert.AreEqual(testAccessKeyId, actualAccessKeyId);
            Assert.AreEqual(testSecretKey, actualSecretKey);
            Assert.AreEqual(testRegion, actualRegion);
        }

        [Test]
        public void OnCreated_WhenCreateSuccess_IsRaised()
        {
            
            SetUpForCreateMethodSuccess(out Mock<CoreApi> coreApiMock, out AwsCredentialsCreation awsCredentials);

            bool isEventRaised = false;
            awsCredentials.OnCreated += () =>
            {
                isEventRaised = true;
            };
            
            // Act
            awsCredentials.Create();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(isEventRaised);
        }

        [Test]
        public void Status_WhenCreateSuccess_DisplaysCreated()
        {
            SetUpForCreateMethodSuccess(out Mock<CoreApi> coreApiMock, out AwsCredentialsCreation awsCredentials);

            // Act
            awsCredentials.Create();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Info, awsCredentials.Status.Type);
            Assert.AreEqual(_textProvider.Get(Strings.StatusProfileCreated), awsCredentials.Status.Message);
        }

        [Test]
        public void Status_WhenCreateFailsToSetCurrent_DisplaysError()
        {
            const string testProfileName = "TestProfile";
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";
            const string testRegion = "eu-west-1";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.ListAvailableRegions()).Returns(new List<string> { testRegion });
            SaveAwsCredentialsResponse saveResponse = Response.Ok(new SaveAwsCredentialsResponse());
            coreApiMock.Setup(target => target.SaveAwsCredentials(testProfileName, testAccessKeyId, testSecretKey, testRegion))
                .Returns(saveResponse)
                .Verifiable();

            PutSettingResponse writeResponse = Response.Fail(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, testProfileName))
                .Returns(writeResponse)
                .Verifiable();

            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = testProfileName;
            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Create();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Error, awsCredentials.Status.Type);
            Assert.IsNotNull(awsCredentials.Status.Message);
        }

        [Test]
        public void Status_WhenCreateFailsToCreate_DisplaysError()
        {
            const string testProfileName = "TestProfile";
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";
            const string testRegion = "eu-west-1";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.ListAvailableRegions()).Returns(new List<string> { testRegion });
            SaveAwsCredentialsResponse saveResponse = Response.Fail(new SaveAwsCredentialsResponse());
            coreApiMock.Setup(target => target.SaveAwsCredentials(testProfileName, testAccessKeyId, testSecretKey, testRegion))
                .Returns(saveResponse)
                .Verifiable();

            AwsCredentialsCreation awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = testProfileName;
            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Create();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Error, awsCredentials.Status.Type);
            Assert.IsNotNull(awsCredentials.Status.Message);
        }

        private void SetUpForCreateMethodSuccess(out Mock<CoreApi> coreApiMock, out AwsCredentialsCreation awsCredentials)
        {
            const string testProfileName = "TestProfile";
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";
            const string testRegion = "eu-west-1";

            coreApiMock = new Mock<CoreApi>();
            coreApiMock.Setup(target => target.ListAvailableRegions()).Returns(new List<string> { testRegion });
            
            SaveAwsCredentialsResponse saveResponse = Response.Ok(new SaveAwsCredentialsResponse());
            coreApiMock.Setup(target => target.SaveAwsCredentials(testProfileName, testAccessKeyId, testSecretKey, testRegion))
                .Returns(saveResponse)
                .Verifiable();

            PutSettingResponse writeResponse = Response.Ok(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, testProfileName))
                .Returns(writeResponse)
                .Verifiable();

            awsCredentials = GetAwsCredentials(coreApiMock.Object);
            awsCredentials.ProfileName = testProfileName;
            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;
        }

        #endregion

        private AwsCredentialsCreation GetAwsCredentials(CoreApi coreApi,
            Mock<RegionBootstrap> regionMock = null, bool canSaveRegion = true)
        {
            regionMock = regionMock ?? new Mock<RegionBootstrap>(coreApi);
            regionMock.Setup(target => target.CanSave)
                .Returns(canSaveRegion)
                .Verifiable();
            regionMock.Setup(target => target.Refresh())
                .Verifiable();
            regionMock.Setup(target => target.Save())
                .Returns((true, null))
                .Verifiable();

            return new AwsCredentialsCreation(_textProvider, regionMock.Object, coreApi, new MockLogger());
        }
    }
}
