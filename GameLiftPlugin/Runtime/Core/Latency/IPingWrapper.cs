// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency.Models;

namespace AmazonGameLiftPlugin.Core.Latency
{
    public interface IPingWrapper
    {
        Task<PingResult> SendPingAsync(string hostNameOrAddress);
    }
}
