// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.SettingsManagement.Models
{
    public class PutSettingRequest
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class PutSettingResponse : Response
    {
    }
}
