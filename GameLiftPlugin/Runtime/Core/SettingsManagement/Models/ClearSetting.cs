// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.SettingsManagement.Models
{
    public class ClearSettingRequest
    {
        public string Key { get; set; }
    }

    public class ClearSettingResponse : Response
    {
    }
}
