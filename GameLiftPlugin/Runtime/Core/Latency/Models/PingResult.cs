// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.Latency.Models
{
    public class PingResult
    {
        public long RoundtripTime { get; set; }

        // Whether the ping beacon answered within the timeout. UDP has no guaranteed
        // delivery, so unanswered samples are excluded from the latency average.
        public bool Success { get; set; } = true;
    }
}
