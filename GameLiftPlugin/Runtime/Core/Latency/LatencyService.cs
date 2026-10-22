// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly int _pingIntervalMs;
        private const int DefaultPingCount = 3;

        // Amazon GameLift Servers exposes a UDP ping beacon per hosting location. See:
        // https://docs.aws.amazon.com/gameliftservers/latest/developerguide/reference-udp-ping-beacons.html
        private const int PingBeaconPort = 7770;

        // Ping beacons rate-limit to a few requests per second per location. Space the
        // samples out so a burst is not throttled and reported as lost.
        private const int DefaultPingIntervalMs = 1000;

        public LatencyService(
            IPingWrapper pingWrapper,
            int pingCount = DefaultPingCount,
            int pingIntervalMs = DefaultPingIntervalMs)
        {
            _pingWrapper = pingWrapper;
            _pingCount = pingCount <= 0 ? DefaultPingCount : pingCount;
            _pingIntervalMs = pingIntervalMs < 0 ? DefaultPingIntervalMs : pingIntervalMs;
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

                IEnumerable<Task<KeyValuePair<string, long>?>> regionLatencyCalculationTasks =
                    request.Regions.Select(CalculateLatencyForRegion);

                KeyValuePair<string, long>?[] regionLatencies =
                    await Task.WhenAll(regionLatencyCalculationTasks);

                var regionLatencyMap = regionLatencies
                    .Where(entry => entry.HasValue)
                    .ToDictionary(entry => entry.Value.Key, entry => entry.Value.Value);

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

        // UDP ping beacon host for a region. China partition regions use a different DNS
        // suffix from the standard partition.
        private static string GetPingBeaconEndpointFromRegion(string region) =>
            region != null && region.StartsWith("cn-")
                ? $"gamelift-ping.{region}.api.amazonwebservices.com.cn"
                : $"gamelift-ping.{region}.api.aws";

        // Returns the region and its average round-trip latency, or null when the beacon
        // could not be reached so the region is omitted from the result.
        private async Task<KeyValuePair<string, long>?> CalculateLatencyForRegion(string region)
        {
            try
            {
                string host = GetPingBeaconEndpointFromRegion(region);

                var roundtripTimes = new List<long>();

                for (int counter = 0; counter < _pingCount; counter++)
                {
                    if (counter > 0 && _pingIntervalMs > 0)
                    {
                        await Task.Delay(_pingIntervalMs);
                    }

                    PingResult pingResult = await _pingWrapper.SendPingAsync(host, PingBeaconPort);

                    if (pingResult.Success)
                    {
                        roundtripTimes.Add(pingResult.RoundtripTime);
                    }
                }

                if (roundtripTimes.Count == 0)
                {
                    // No samples succeeded for this region; omit it so matchmaking is not
                    // given a fabricated latency for an unreachable location.
                    Logger.LogError($"No ping beacon responses received for region {region}; skipping.");
                    return null;
                }

                long averagePingInMs = roundtripTimes.Sum() / roundtripTimes.Count;

                return new KeyValuePair<string, long>(region, averagePingInMs);
            }
            catch (Exception ex)
            {
                // Log and skip this region if measuring it fails unexpectedly.
                Logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
