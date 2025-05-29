// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.SettingsStore;

namespace AmazonGameLiftPlugin.Core.Tests.Factories
{
    public class SettingsStoreFactory
    {
        private readonly IFileWrapper _fileWrapper;
        private readonly IStreamWrapper _yamlStreamWrapper;
        private readonly string _settingsFilePath;

        public SettingsStoreFactory(IFileWrapper fileWrapper, IStreamWrapper yamlStreamWrapper = default, string settingsFilePath = default)
        {
            _fileWrapper = fileWrapper;
            _yamlStreamWrapper = yamlStreamWrapper;
            _settingsFilePath = settingsFilePath;
        }

        public SettingsStore CreateSettingsStore(IFileWrapper fileWrapper = default, IStreamWrapper yamlStreamWrapper = default, string settingsFilePath = default)
        {
            return new SettingsStore(fileWrapper ?? _fileWrapper, yamlStreamWrapper ?? _yamlStreamWrapper, settingsFilePath ?? _settingsFilePath);
        }

        public (bool isSucceed, string key, string value) GetCreatedSettings(string key = default, string value = default)
        {
            SettingsStore settingsStore = CreateSettingsStore();

            key = key ?? Guid.NewGuid().ToString();
            value = value ?? Guid.NewGuid().ToString();

            PutSettingResponse writeResponse = settingsStore.PutSetting(new PutSettingRequest()
            {
                Key = key,
                Value = value,
            });

            return (writeResponse.Success, key, value);
        }
    }
}
