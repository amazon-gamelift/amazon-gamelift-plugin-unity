// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models
{
    public class StopRequest
    {
        public int ProcessId { get; set; }

        public LocalOperatingSystem LocalOperatingSystem { get; set; }
    }

    public class StopResponse : Response
    {

    }
}
