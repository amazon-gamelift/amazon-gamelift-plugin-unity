// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Tests.Factories;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.CredentialManagement
{
    [TestFixture]
    public class CredentialsStoreTests
    {
        private string FilePath { get; set; }

        private CredentialsStoreFactory Factory { get; set; }

        [SetUp]
        public void Init()
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), $"customCredentialsFile.ini");
            Factory = new CredentialsStoreFactory(FilePath);

            if (!File.Exists(FilePath))
            {
                FileStream filestream = File.Create(FilePath);

                filestream.Close();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FilePath);
        }

        [Test]
        public void SaveAwsCredentials_WhenCredentialsAreCorrect_IsSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";
            string accessKey = $"NonEmptyAccessKey-{Guid.NewGuid()}";
            string secretKey = $"NonEmptySecretKey-{Guid.NewGuid()}";

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            SaveAwsCredentialsResponse response = credentialsStore.SaveAwsCredentials(new SaveAwsCredentialsRequest()
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                ProfileName = profileName
            }); ;

            Assert.IsTrue(response.Success);
            Assert.IsNull(response.ErrorCode);
        }

        [Test]
        public void SaveAwsCredentials_WhenAwsProfileAlreadyExists_IsNotSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";

            (string profileName, string accessKey, string secretKey) existingCredentials = EnsureValidCredentialsIsCreated(profileName: profileName);

            string newAccessKey = $"NonEmptyAccessKey-{Guid.NewGuid()}";
            string newSecretKey = $"NonEmptySecretKey-{Guid.NewGuid()}";

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            SaveAwsCredentialsResponse response = credentialsStore.SaveAwsCredentials(new SaveAwsCredentialsRequest()
            {
                AccessKey = newAccessKey,
                SecretKey = newSecretKey,
                ProfileName = profileName
            }); ;

            Assert.IsFalse(response.Success);
            Assert.IsNotEmpty(response.ErrorCode);
            Assert.AreSame(response.ErrorCode, ErrorCode.ProfileAlreadyExists);
        }

        [Test]
        public void SaveAwsCredentials_WhenCredentialsAreEmpty_IsNotSuccessful()
        {
            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            SaveAwsCredentialsResponse response = credentialsStore.SaveAwsCredentials(new SaveAwsCredentialsRequest()
            {
                AccessKey = null,
                SecretKey = null,
                ProfileName = null
            });

            Assert.IsFalse(response.Success);
        }

        [Test]
        public void RetriveAwsCredentials_WhenProfileExists_IsSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";

            (string profileName, string accessKey, string secretKey) existingCredentials = EnsureValidCredentialsIsCreated(profileName: profileName);

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();
            //Retrive
            RetriveAwsCredentialsResponse credentials = credentialsStore.RetriveAwsCredentials(new RetriveAwsCredentialsRequest()
            {
                ProfileName = profileName
            });

            Assert.NotNull(credentials);
            Assert.AreEqual(existingCredentials.accessKey, credentials.AccessKey);
            Assert.AreEqual(existingCredentials.secretKey, credentials.SecretKey);
            Assert.AreEqual(existingCredentials.profileName, profileName);
        }

        [Test]
        public void RetriveAwsCredentials_WhenProfileDoesNotExist_IsNotSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            RetriveAwsCredentialsResponse retriveAwsCredentialsResponse = credentialsStore.RetriveAwsCredentials(new RetriveAwsCredentialsRequest()
            {
                ProfileName = profileName
            });

            Assert.IsFalse(retriveAwsCredentialsResponse.Success);
        }

        [Test]
        public void UpdateAwsCredentials_WhenAwsProfileExists_IsSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";

            (string profileName, string accessKey, string secretKey) existingCredentials = EnsureValidCredentialsIsCreated(profileName: profileName);

            string newAccessKey = $"NonEmptyAccessKey-{Guid.NewGuid()}";
            string newSecretKey = $"NonEmptySecretKey-{Guid.NewGuid()}";

            Assert.AreNotEqual(newAccessKey, existingCredentials.accessKey);
            Assert.AreNotEqual(newSecretKey, existingCredentials.secretKey);

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            UpdateAwsCredentialsResponse updateAwsCredentialsResponse = credentialsStore.UpdateAwsCredentials(new UpdateAwsCredentialsRequest
            {
                ProfileName = profileName,
                AccessKey = newAccessKey,
                SecretKey = newSecretKey
            });

            Assert.IsTrue(updateAwsCredentialsResponse.Success);

            RetriveAwsCredentialsResponse retriveAwsCredentialsResponse = credentialsStore.RetriveAwsCredentials(new RetriveAwsCredentialsRequest
            {
                ProfileName = profileName
            });

            Assert.NotNull(retriveAwsCredentialsResponse);
            Assert.AreEqual(newAccessKey, retriveAwsCredentialsResponse.AccessKey);
            Assert.AreEqual(newSecretKey, retriveAwsCredentialsResponse.SecretKey);
        }

        [Test]
        public void UpdateAwsCredentials_WhenAwsProfileDoesNotExist_IsNotSuccessful()
        {
            string profileName = $"NonEmptyProfileName-{Guid.NewGuid()}";

            string newAccessKey = $"NonEmptyAccessKey-{Guid.NewGuid()}";
            string newSecretKey = $"NonEmptySecretKey-{Guid.NewGuid()}";

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            UpdateAwsCredentialsResponse updateAwsCredentialsResponse = credentialsStore.UpdateAwsCredentials(new UpdateAwsCredentialsRequest
            {
                ProfileName = profileName,
                AccessKey = newAccessKey,
                SecretKey = newSecretKey
            });

            Assert.IsFalse(updateAwsCredentialsResponse.Success);

            Assert.IsNotEmpty(updateAwsCredentialsResponse.ErrorCode);

            RetriveAwsCredentialsResponse retriveAwsCredentialsResponse = credentialsStore.RetriveAwsCredentials(new RetriveAwsCredentialsRequest
            {
                ProfileName = profileName
            });

            Assert.IsFalse(retriveAwsCredentialsResponse.Success);
        }

        [Test]
        public void GetProfiles_WhenProfilesExist_IsSuccessful()
        {
            (string profileName, string accessKey, string secretKey) = EnsureValidCredentialsIsCreated();

            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            GetProfilesResponse getProfilesResponse = credentialsStore.GetProfiles(new GetProfilesRequest());

            Assert.IsTrue(getProfilesResponse.Profiles.Any(x => x == profileName));
        }

        [Test]
        public void GetProfiles_WhenProfilesDoNotExist_IsNotSuccessful()
        {
            ICredentialsStore credentialsStore = Factory.CreateCredentialsManager();

            GetProfilesResponse getProfilesResponse = credentialsStore.GetProfiles(new GetProfilesRequest());

            Assert.IsTrue(!getProfilesResponse.Profiles.Any());
        }

        private (string profileName, string accessKey, string secretKey) EnsureValidCredentialsIsCreated(string profileName = default, string accessKey = default, string secretKey = default)
        {
            (bool isSucceed, string profileName, string accessKey, string secretKey) createCredentialsResponse = Factory.CreateCredentials(profileName, accessKey, secretKey);

            Assert.IsTrue(createCredentialsResponse.isSucceed);

            return (createCredentialsResponse.profileName, createCredentialsResponse.accessKey, createCredentialsResponse.secretKey);
        }
    }
}
