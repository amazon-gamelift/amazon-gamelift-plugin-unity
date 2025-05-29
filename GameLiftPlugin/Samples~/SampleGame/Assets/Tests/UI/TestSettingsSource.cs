// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.IO;
using Newtonsoft.Json;

namespace SampleTests.UI
{
    internal sealed class TestSettingsSource
    {
        private const string DefaultPath = "UiTestSettings.json";

        public TestSettings Get(string filePath = DefaultPath)
        {
            string settingsText = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TestSettings>(settingsText);
        }
    }
}
