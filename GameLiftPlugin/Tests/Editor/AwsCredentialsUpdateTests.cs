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
    public class AwsCredentialsUpdateTests
    {
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        #region Refresh

        [Test]
        public void Refresh_WhenAnyProfilesAndCurrentSaved_AllPropertiesAreUpdated()
        {
            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out Mock<CoreApi> coreApiMock);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.IsNotNull(awsCredentials.AllProlfileNames);
            Assert.AreEqual(testProfiles.Count, awsCredentials.AllProlfileNames.Length);
            Assert.AreEqual(1, awsCredentials.CurrentProfileIndex);
            Assert.AreEqual(testProfiles[1], awsCredentials.CurrentProfileName);
            Assert.AreEqual(1, awsCredentials.SelectedProfileIndex);
            Assert.AreEqual(false, awsCredentials.CanUpdateCurrentProfile);
            Assert.AreEqual(true, awsCredentials.CanUpdate);
            Assert.AreEqual("NonEmptyKey", awsCredentials.AccessKeyId);
            Assert.AreEqual("NonEmptySecret", awsCredentials.SecretKey);
        }

        [Test]
        public void Refresh_WhenAnyProfilesAndCurrentCredentialsError_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();
            List<string> testProfiles = coreApiMock.SetUpWithTestProfileListOf2();
            coreApiMock.SetUpCoreApiWithProfile(success: true, testProfiles[1]);

            RetriveAwsCredentialsResponse credentialsResponse = Response.Fail(new RetriveAwsCredentialsResponse());
            coreApiMock.Setup(target => target.RetrieveAwsCredentials(testProfiles[1]))
                .Returns(credentialsResponse)
                .Verifiable();

            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();

            Assert.IsNotNull(awsCredentials.AllProlfileNames);
            Assert.AreEqual(testProfiles.Count, awsCredentials.AllProlfileNames.Length);
            Assert.AreEqual(1, awsCredentials.CurrentProfileIndex);
            Assert.AreEqual(testProfiles[1], awsCredentials.CurrentProfileName);
            Assert.AreEqual(1, awsCredentials.SelectedProfileIndex);
            Assert.AreEqual(false, awsCredentials.CanUpdateCurrentProfile);
            Assert.AreEqual(false, awsCredentials.CanUpdate);
            Assert.IsNull(awsCredentials.AccessKeyId);
            Assert.IsNull(awsCredentials.SecretKey);
        }

        [Test]
        public void Refresh_WhenAnyProfilesAndNoCurrent_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();
            List<string> testProfiles = coreApiMock.SetUpWithTestProfileListOf2();
            coreApiMock.SetUpCoreApiWithProfile(success: false);

            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();

            AssertCurrentProfileNotLoaded(awsCredentials);
            Assert.IsNotNull(awsCredentials.AllProlfileNames);
            Assert.AreEqual(testProfiles.Count, awsCredentials.AllProlfileNames.Length);
            Assert.IsNull(awsCredentials.AccessKeyId);
            Assert.IsNull(awsCredentials.SecretKey);
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

            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();

            AssertCurrentProfileNotLoaded(awsCredentials);
            Assert.IsNotNull(awsCredentials.AllProlfileNames);
            Assert.AreEqual(0, awsCredentials.AllProlfileNames.Length);
            Assert.IsNull(awsCredentials.AccessKeyId);
            Assert.IsNull(awsCredentials.SecretKey);
        }

        [Test]
        public void Refresh_WhenGetProfilesError_AllPropertiesAreUpdated()
        {
            var coreApiMock = new Mock<CoreApi>();

            var profilesResponse = new GetProfilesResponse()
            {
                ErrorCode = "TestError"
            };
            profilesResponse = Response.Fail(profilesResponse);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(profilesResponse)
                .Verifiable();

            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();

            AssertCurrentProfileNotLoaded(awsCredentials);
            Assert.IsNotNull(awsCredentials.AllProlfileNames);
            Assert.AreEqual(0, awsCredentials.AllProlfileNames.Length);
            Assert.IsNull(awsCredentials.AccessKeyId);
            Assert.IsNull(awsCredentials.SecretKey);
        }

        #endregion

        #region CanUpdate

        [Test]
        public void CanUpdate_WhenNewInstance_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);

            Assert.AreEqual(false, awsCredentials.CanUpdate);
        }

        [Test]
        public void CanUpdate_WhenSecretKeyEmpty_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);
            awsCredentials.AccessKeyId = "NonEmpty";
            Assert.AreEqual(false, awsCredentials.CanUpdate);
        }

        [Test]
        public void CanUpdate_WhenAccessKeyIdEmpty_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);
            awsCredentials.SecretKey = "NonEmpty";

            Assert.AreEqual(false, awsCredentials.CanUpdate);
        }

        [Test]
        public void CanUpdate_WhenNonEmptyFieldsAndNotRefreshed_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            AwsCredentialsUpdate awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);
            awsCredentials.AccessKeyId = "NonEmptyId";
            awsCredentials.SecretKey = "NonEmpty";

            Assert.AreEqual(false, awsCredentials.CanUpdate);
        }

        [Test]
        public void CanUpdate_WhenRefreshedAndUpdatedFields_IsTrue()
        {
            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out _, out _);

            awsCredentials.Refresh();

            // Act
            awsCredentials.AccessKeyId = DateTime.Now.ToString();
            awsCredentials.SecretKey = DateTime.Now.ToString();

            Assert.AreEqual(true, awsCredentials.CanUpdate);
        }

        #endregion

        #region Update

        [Test]
        public void Update_WhenCanUpdate_SavesAwsCredentialsAndSetsCurrentProfile()
        {
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";

            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out Mock<CoreApi> coreApiMock);
            awsCredentials.Refresh();
            string testProfileName = testProfiles[awsCredentials.SelectedProfileIndex];

            string actualProfileName = null;
            string actualAccessKeyId = null;
            string actualSecretKey = null;

            // Credentials mock
            UpdateAwsCredentialsResponse saveResponse = Response.Ok(new UpdateAwsCredentialsResponse());
            coreApiMock.Setup(target => target.UpdateAwsCredentials(testProfileName, testAccessKeyId, testSecretKey))
                .Returns(saveResponse)
                .Callback<string, string, string>((profileName, accessKeyId, secretKey) =>
                {
                    actualProfileName = profileName;
                    actualAccessKeyId = accessKeyId;
                    actualSecretKey = secretKey;
                })
                .Verifiable();

            // Settings mock
            PutSettingResponse writeResponse = Response.Ok(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, testProfileName))
                .Returns(writeResponse)
                .Verifiable();

            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Update();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(testProfileName, awsCredentials.CurrentProfileName);
            Assert.AreEqual(testProfileName, actualProfileName);
            Assert.AreEqual(testAccessKeyId, actualAccessKeyId);
            Assert.AreEqual(testSecretKey, actualSecretKey);
        }

        [Test]
        public void Update_WhenCanUpdate_ClearsBootstrapBucket()
        {
            AwsCredentialsUpdate awsCredentials = SetUpCredentialsForProfileSuccess(out Mock<CoreApi> coreApiMock);

            coreApiMock.Setup(target => target.ClearSetting(SettingsKeys.CurrentBucketName))
                .Verifiable();

            // Act
            awsCredentials.Update();

            // Assert
            coreApiMock.Verify();
        }

        private AwsCredentialsUpdate SetUpCredentialsForProfileSuccess(out Mock<CoreApi> coreApiMock)
        {
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";

            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out coreApiMock);
            awsCredentials.Refresh();
            string testProfileName = testProfiles[awsCredentials.SelectedProfileIndex];

            string actualProfileName = null;
            string actualAccessKeyId = null;
            string actualSecretKey = null;

            // Credentials mock
            UpdateAwsCredentialsResponse saveResponse = Response.Ok(new UpdateAwsCredentialsResponse());
            coreApiMock.Setup(target => target.UpdateAwsCredentials(testProfileName, testAccessKeyId, testSecretKey))
                .Returns(saveResponse)
                .Callback<string, string, string>((profileName, accessKeyId, secretKey) =>
                {
                    actualProfileName = profileName;
                    actualAccessKeyId = accessKeyId;
                    actualSecretKey = secretKey;
                })
                .Verifiable();

            // Settings mock
            PutSettingResponse writeResponse = Response.Ok(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, testProfileName))
                .Returns(writeResponse)
                .Verifiable();

            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;
            return awsCredentials;
        }

        [Test]
        public void Status_WhenUpdateSuccess_DisplaysUpdated()
        {
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";

            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out Mock<CoreApi> coreApiMock);

            awsCredentials.Refresh();

            // Credentials mock
            UpdateAwsCredentialsResponse saveResponse = Response.Ok(new UpdateAwsCredentialsResponse());
            coreApiMock.Setup(target => target.UpdateAwsCredentials(It.IsAny<string>(), testAccessKeyId, testSecretKey))
                .Returns(saveResponse)
                .Verifiable();

            // Settings mock
            PutSettingResponse writeResponse = Response.Ok(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, It.IsAny<string>()))
                .Returns(writeResponse)
                .Verifiable();

            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Update();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Info, awsCredentials.Status.Type);
            Assert.AreEqual(_textProvider.Get(Strings.StatusProfileUpdated), awsCredentials.Status.Message);
        }

        [Test]
        public void Status_WhenUpdateFailsToSetCurrent_DisplaysError()
        {
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";

            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out Mock<CoreApi> coreApiMock);

            awsCredentials.Refresh();

            // Credentials mock
            UpdateAwsCredentialsResponse saveResponse = Response.Ok(new UpdateAwsCredentialsResponse());
            coreApiMock.Setup(target => target.UpdateAwsCredentials(It.IsAny<string>(), testAccessKeyId, testSecretKey))
                .Returns(saveResponse)
                .Verifiable();

            // Settings mock
            PutSettingResponse writeResponse = Response.Fail(new PutSettingResponse());
            coreApiMock.Setup(target => target.PutSetting(SettingsKeys.CurrentProfileName, It.IsAny<string>()))
                .Returns(writeResponse)
                .Verifiable();

            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Update();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Error, awsCredentials.Status.Type);
            Assert.IsNotNull(awsCredentials.Status.Message);
        }

        [Test]
        public void Status_WhenUpdateFailsToUpdate_DisplaysError()
        {
            const string testAccessKeyId = "TestKey";
            const string testSecretKey = "TestSecret";

            PrepareProfileList(out AwsCredentialsUpdate awsCredentials, out List<string> testProfiles,
                out Mock<CoreApi> coreApiMock);

            awsCredentials.Refresh();

            // Credentials mock
            UpdateAwsCredentialsResponse saveResponse = Response.Fail(new UpdateAwsCredentialsResponse());
            coreApiMock.Setup(target => target.UpdateAwsCredentials(It.IsAny<string>(), testAccessKeyId, testSecretKey))
                .Returns(saveResponse)
                .Verifiable();

            awsCredentials.AccessKeyId = testAccessKeyId;
            awsCredentials.SecretKey = testSecretKey;

            // Act
            awsCredentials.Update();

            // Assert
            coreApiMock.Verify();
            Assert.IsTrue(awsCredentials.Status.IsDisplayed);
            Assert.AreEqual(MessageType.Error, awsCredentials.Status.Type);
            Assert.IsNotNull(awsCredentials.Status.Message);
        }

        #endregion

        private static void AssertCurrentProfileNotLoaded(AwsCredentialsUpdate awsCredentials)
        {
            Assert.AreEqual(-1, awsCredentials.CurrentProfileIndex);
            Assert.IsNull(awsCredentials.CurrentProfileName);
            Assert.AreEqual(-1, awsCredentials.SelectedProfileIndex);
            Assert.AreEqual(false, awsCredentials.CanUpdateCurrentProfile);
            Assert.AreEqual(false, awsCredentials.CanUpdate);
        }

        private AwsCredentialsUpdate GetUpdateInstanceWithMockRegion(CoreApi coreApi)
        {
            var regionMock = new Mock<RegionBootstrap>(coreApi);
            regionMock.Setup(target => target.CanSave)
                .Returns(true)
                .Verifiable();
            regionMock.Setup(target => target.Refresh())
                .Verifiable();
            regionMock.Setup(target => target.Save())
                .Returns((true, null))
                .Verifiable();
            return new AwsCredentialsUpdate(_textProvider, regionMock.Object, coreApi, new MockLogger());
        }

        private void PrepareProfileList(out AwsCredentialsUpdate awsCredentials,
            out List<string> testProfiles, out Mock<CoreApi> coreApiMock)
        {
            coreApiMock = new Mock<CoreApi>();
            testProfiles = coreApiMock.SetUpWithTestProfileListOf2();
            string currentProfile = testProfiles[1];
            coreApiMock.SetUpCoreApiWithProfile(success: true, currentProfile);

            var credentialsResponse = new RetriveAwsCredentialsResponse()
            {
                AccessKey = "NonEmptyKey",
                SecretKey = "NonEmptySecret"
            };
            credentialsResponse = Response.Ok(credentialsResponse);
            coreApiMock.Setup(target => target.RetrieveAwsCredentials(currentProfile))
                .Returns(credentialsResponse)
                .Verifiable();

            awsCredentials = GetUpdateInstanceWithMockRegion(coreApiMock.Object);
        }
    }
}
