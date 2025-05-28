// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.SettingsManagement.Models
{
    public class GetSettingRequest
    {
        public string Key { get; set; }
    }

    public class GetSettingResponse : Response
    {
        public string Value { get; set; }
    }
}
