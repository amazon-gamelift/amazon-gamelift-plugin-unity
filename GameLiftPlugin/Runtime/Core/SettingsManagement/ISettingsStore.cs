// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.SettingsManagement.Models;

namespace AmazonGameLiftPlugin.Core.SettingsManagement
{
    public interface ISettingsStore
    {
        PutSettingResponse PutSetting(PutSettingRequest request);

        GetSettingResponse GetSetting(GetSettingRequest request);

        ClearSettingResponse ClearSetting(ClearSettingRequest request);
    }
}
