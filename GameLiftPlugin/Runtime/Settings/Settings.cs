// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.SettingsManagement;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;

namespace AmazonGameLift.Runtime
{
    public class Settings<TEnum> where TEnum : struct, Enum
    {
        private readonly ISettingsStore _settingsStore;

        private static string GetKeyName(TEnum value) => Enum.GetName(typeof(TEnum), value);

        public Settings(string configFilePath)
        {
            _settingsStore = new SettingsStore(new FileWrapper(), settingsFilePath: configFilePath);
        }

        public GetSettingResponse GetSetting(TEnum key)
        {
            var request = new GetSettingRequest() { Key = GetKeyName(key) };
            return _settingsStore.GetSetting(request);
        }

        public virtual PutSettingResponse PutSetting(TEnum key, string value)
        {
            var request = new PutSettingRequest() { Key = GetKeyName(key), Value = value };
            return _settingsStore.PutSetting(request);
        }

        public virtual ClearSettingResponse ClearSetting(TEnum key)
        {
            var request = new ClearSettingRequest() { Key = GetKeyName(key) };
            return _settingsStore.ClearSetting(request);
        }

        public virtual Response PutSettingOrClear(TEnum key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return _settingsStore.ClearSetting(new ClearSettingRequest() { Key = GetKeyName(key) });
            }

            return _settingsStore.PutSetting(new PutSettingRequest() { Key = GetKeyName(key), Value = value });
        }
    }
}