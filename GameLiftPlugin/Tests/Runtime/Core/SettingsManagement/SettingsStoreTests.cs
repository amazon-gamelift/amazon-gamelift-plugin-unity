// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Tests.Factories;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.SettingsManagement
{
    [TestFixture]
    public class SettingsStoreTests
    {
        private string FilePath { get; set; }
        private SettingsStoreFactory Factory { get; set; }

        [SetUp]
        public void Init()
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), $"settings.yaml");
            Factory = new SettingsStoreFactory(new FileWrapper(), settingsFilePath: FilePath);
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(FilePath);
        }

        [Test]
        public void GetSetting_WhenKeyExists_IsSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = existingSettings.key
            });

            Assert.IsTrue(getSettingsResponse.Success);
            Assert.AreEqual(getSettingsResponse.Value, existingSettings.value);
        }

        [Test]
        public void GetSetting_WhenPassedKeyIsEmpty_IsNotSuccessful()
        {
            SettingsStore settingsStore = Factory.CreateSettingsStore();

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = string.Empty
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
            Assert.AreSame(getSettingsResponse.ErrorCode, ErrorCode.InvalidParameters);
        }

        [Test]
        public void GetSetting_WhenPassedKeyDoesNotExist_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = Guid.NewGuid().ToString()
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
            Assert.AreSame(getSettingsResponse.ErrorCode, ErrorCode.NoSettingsKeyFound);
        }

        [Test]
        public void GetSetting_WhenPassedKeyWasCleanedAndDoesNotExist_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();


            ClearSettingResponse clearResponse = settingsStore.ClearSetting(new ClearSettingRequest() { Key = existingSettings.key });

            Assert.True(clearResponse.Success);

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = existingSettings.key
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
            Assert.AreSame(getSettingsResponse.ErrorCode, ErrorCode.NoSettingsKeyFound);
        }

        [Test]
        public void GetSetting_WhenFileDoesNotExist_IsNotSuccessful()
        {
            SettingsStore settingsStore = Factory.CreateSettingsStore();

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = Guid.NewGuid().ToString()
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
            Assert.AreSame(getSettingsResponse.ErrorCode, ErrorCode.NoSettingsFileFound);
        }

        [Test]
        public void GetSetting_WhenFileIsInvalid_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            var fileWrapperMock = new Mock<IFileWrapper>();

            string emptyContenent = string.Empty;

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(emptyContenent);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            ISettingsStore settingsStore = Factory.CreateSettingsStore(fileWrapperMock.Object);

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = Guid.NewGuid().ToString()
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
        }

        [Test]
        public void PutSettings_WhenKeyValueIsCorrect_IsSuccessful()
        {
            SettingsStore settingsStore = Factory.CreateSettingsStore();

            string key = "NonEmptyKey";
            string value = "NonEmptyValue";

            PutSettingResponse putSettingsResponse = settingsStore.PutSetting(new PutSettingRequest
            {
                Key = key,
                Value = value
            });

            Assert.True(putSettingsResponse.Success);


            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = key
            });

            Assert.IsTrue(getSettingsResponse.Success);
            Assert.AreEqual(value, getSettingsResponse.Value);
        }

        [Test]
        public void PutSettings_WhenKeyValueIsEmpty_IsNotSuccessful()
        {
            SettingsStore settingsStore = Factory.CreateSettingsStore();

            PutSettingResponse putSettingsResponse = settingsStore.PutSetting(new PutSettingRequest
            {
                Key = string.Empty,
                Value = string.Empty,
            });

            Assert.IsFalse(putSettingsResponse.Success);
            Assert.AreSame(putSettingsResponse.ErrorCode, ErrorCode.InvalidParameters);
        }

        [Test]
        public void PutSettings_WhenFileIsInvalid_IsNotSuccessful()
        {
            var fileWrapperMock = new Mock<IFileWrapper>();
            string emptyContenent = string.Empty;

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(emptyContenent);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            ISettingsStore settingsStore = Factory.CreateSettingsStore(fileWrapperMock.Object);

            PutSettingResponse putSettingsResponse = settingsStore.PutSetting(new PutSettingRequest
            {
                Key = Guid.NewGuid().ToString(),
                Value = Guid.NewGuid().ToString(),
            });

            Assert.IsFalse(putSettingsResponse.Success);
            Assert.AreSame(putSettingsResponse.ErrorCode, ErrorCode.InvalidSettingsFile);
        }

        [Test]
        public void PutSettings_WhenKeyExists_IsSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();

            string newValue = Guid.NewGuid().ToString();
            PutSettingResponse putSettingsResponse = settingsStore.PutSetting(new PutSettingRequest
            {
                Key = existingSettings.key,
                Value = newValue,
            });

            Assert.IsTrue(putSettingsResponse.Success);

            Assert.AreNotEqual(newValue, existingSettings.value);


            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = existingSettings.key
            });

            Assert.IsTrue(getSettingsResponse.Success);
            Assert.AreEqual(newValue, getSettingsResponse.Value);
        }

        [Test]
        public void PutSettings_WhenKeyExistsAndValueIsEmpty_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();

            string newValue = string.Empty;
            PutSettingResponse putSettingsResponse = settingsStore.PutSetting(new PutSettingRequest
            {
                Key = existingSettings.key,
                Value = newValue,
            });

            Assert.IsFalse(putSettingsResponse.Success);
            Assert.AreSame(putSettingsResponse.ErrorCode, ErrorCode.InvalidParameters);
        }


        [Test]
        public void ClearSetting_WhenPassedKeyExists_IsSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();


            ClearSettingResponse clearResponse = settingsStore.ClearSetting(new ClearSettingRequest() { Key = existingSettings.key });

            Assert.True(clearResponse.Success);

            GetSettingResponse getSettingsResponse = settingsStore.GetSetting(new GetSettingRequest
            {
                Key = existingSettings.key
            });

            Assert.IsFalse(getSettingsResponse.Success);
            Assert.IsNotEmpty(getSettingsResponse.ErrorCode);
            Assert.AreSame(getSettingsResponse.ErrorCode, ErrorCode.NoSettingsKeyFound);
        }

        [Test]
        public void ClearSetting_WhenPassedKeyDoesNotExists_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();


            ClearSettingResponse clearResponse = settingsStore.ClearSetting(new ClearSettingRequest()
            {
                Key = Guid.NewGuid().ToString()
            });

            Assert.IsTrue(clearResponse.Success);
        }

        [Test]
        public void ClearSetting_WhenEmptyKeyWasPassed_IsNotSuccessful()
        {
            (string key, string value) existingSettings = EnsureValidSettingsAreCreated();

            SettingsStore settingsStore = Factory.CreateSettingsStore();


            ClearSettingResponse clearResponse = settingsStore.ClearSetting(new ClearSettingRequest()
            {
                Key = string.Empty
            });

            Assert.IsFalse(clearResponse.Success);
            Assert.IsNotEmpty(clearResponse.ErrorCode);
            Assert.AreSame(clearResponse.ErrorCode, ErrorCode.InvalidParameters);
        }

        [Test]
        public void ClearSetting_WhenFileDoesNotExist_IsNotSuccessful()
        {
            SettingsStore settingsStore = Factory.CreateSettingsStore();

            ClearSettingResponse clearResponse = settingsStore.ClearSetting(new ClearSettingRequest()
            {
                Key = Guid.NewGuid().ToString()
            });

            Assert.IsFalse(clearResponse.Success);
            Assert.IsNotEmpty(clearResponse.ErrorCode);
            Assert.AreSame(clearResponse.ErrorCode, ErrorCode.NoSettingsFileFound);
        }

        private (string key, string value) EnsureValidSettingsAreCreated(string key = default, string value = default)
        {
            (bool isSucceed, string key, string value) createdSettings = Factory.GetCreatedSettings(key, value);
            Assert.IsTrue(createdSettings.isSucceed);

            return (createdSettings.key, createdSettings.value);
        }
    }
}
