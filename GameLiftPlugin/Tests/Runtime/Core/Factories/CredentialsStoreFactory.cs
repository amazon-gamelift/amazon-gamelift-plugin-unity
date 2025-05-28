// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using Moq;

namespace AmazonGameLiftPlugin.Core.Tests.Factories
{
    public class CredentialsStoreFactory
    {
        private readonly string _sharedCredentialsFilePath;

        public CredentialsStoreFactory(string sharedCredentialsFilePath)
        {
            _sharedCredentialsFilePath = sharedCredentialsFilePath;
        }

        public ICredentialsStore CreateCredentialsManager(string testFileContents = "")
        {
            var fileMock = new Mock<IFileWrapper>();
            fileMock.Setup(target => target.ReadAllText(It.IsAny<string>())).Returns(testFileContents);
            return new CredentialsStore(fileMock.Object, _sharedCredentialsFilePath);
        }

        public (bool isSucceed, string profileName, string accessKey, string secretKey) CreateCredentials(string profileName = default, string accessKey = default, string secretKey = default)
        {
            profileName = profileName ?? $"NonEmptyProfileName-{Guid.NewGuid()}";
            accessKey = accessKey ?? $"NonEmptyAccessKey-{Guid.NewGuid()}";
            secretKey = secretKey ?? $"NonEmptySecretKey-{Guid.NewGuid()}";

            // Store Credentials
            ICredentialsStore credentialsManager = CreateCredentialsManager();
            SaveAwsCredentialsResponse response = credentialsManager.SaveAwsCredentials(new SaveAwsCredentialsRequest()
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                ProfileName = profileName
            });

            return (response.Success, profileName, accessKey, secretKey);
        }
    }
}
