// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency.Models;

namespace AmazonGameLiftPlugin.Core.LatencyMeasurement
{
    public interface ILatencyService
    {
        Task<GetLatenciesResponse> GetLatencies(GetLatenciesRequest request);
    }
}
