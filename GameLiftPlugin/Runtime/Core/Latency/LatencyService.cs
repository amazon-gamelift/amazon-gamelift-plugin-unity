// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.LatencyMeasurement;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;

namespace AmazonGameLiftPlugin.Core.Latency
{
    public class LatencyService : ILatencyService
    {
        private readonly IPingWrapper _pingWrapper;
        private readonly int _pingCount;
        private const int DefaultPingCount = 5;

        public LatencyService(IPingWrapper pingWrapper, int pingCount = DefaultPingCount)
        {
            _pingWrapper = pingWrapper;
            _pingCount = pingCount <= 0 ? DefaultPingCount : pingCount;
        }

        public async Task<GetLatenciesResponse> GetLatencies(GetLatenciesRequest request)
        {
            try
            {
                if (request == null || request.Regions == null || !request.Regions.Any())
                {
                    return Response.Fail(new GetLatenciesResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                var regionLatencyMap = new Dictionary<string, long>();

                var regionLatencyCalculationTasks = new List<Task>();

                foreach (string region in request.Regions)
                {
                    regionLatencyCalculationTasks.Add(
                            CalculateLatencyForRegion(regionLatencyMap, region)
                        );
                }

                await Task.WhenAll(regionLatencyCalculationTasks);

                return Response.Ok(new GetLatenciesResponse
                {
                    RegionLatencies = regionLatencyMap
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new GetLatenciesResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        private string GetEc2EndpointFromRegion(string region) => $"ec2.{region}.amazonaws.com";

        private async Task CalculateLatencyForRegion(
            Dictionary<string, long> regionLatencyMap,
            string region)
        {
            try
            {
                string address = GetEc2EndpointFromRegion(region);

                List<Task<PingResult>> pingTasks = PingAddress(address, _pingCount);

                PingResult[] pingResults = await Task.WhenAll(pingTasks.ToArray());

                long averagePingInMs = pingResults.Sum(x => x.RoundtripTime) / _pingCount;

                regionLatencyMap.Add(region, averagePingInMs);
            }
            catch (PingException ex)
            {
                Logger.LogError(ex, ex.Message);
            }
            catch (SocketException ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        private List<Task<PingResult>> PingAddress(string address, int pingCount)
        {
            var pingReplyTasks = new List<Task<PingResult>>();

            for (int counter = 0; counter < pingCount; counter++)
            {
                pingReplyTasks.Add(_pingWrapper.SendPingAsync(address));
            }

            return pingReplyTasks;
        }
    }
}
