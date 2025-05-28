// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency.Models;

namespace AmazonGameLiftPlugin.Core.Latency
{
    public class PingWrapper : IPingWrapper
    {
        public async Task<PingResult> SendPingAsync(string hostNameOrAddress)
        {
            using (var pinger = new Ping())
            {
                PingReply pingReply = await pinger.SendPingAsync(hostNameOrAddress);

                return new PingResult
                {
                    RoundtripTime = pingReply.RoundtripTime
                };
            }
        }
    }
}
