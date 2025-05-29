// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models
{
    public class StartRequest
    {
        public string GameLiftLocalFilePath { get; set; }

        public int Port { get; set; }

        public LocalOperatingSystem LocalOperatingSystem { get; set; }
    }

    public class StartResponse : Response
    {
        public int ProcessId { get; set; }
    }
}
