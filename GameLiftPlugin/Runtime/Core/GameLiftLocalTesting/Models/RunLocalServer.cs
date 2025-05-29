// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models
{
    public class RunLocalServerRequest
    {
        public string FilePath { get; set; }
        public bool ShowWindow { get; set; }
        public string ApplicationProductName { get; set; }
        public LocalOperatingSystem LocalOperatingSystem { get; set; }
    }

    public class RunLocalServerResponse : Response
    {
        public int ProcessId { get; set; }
    }
}
