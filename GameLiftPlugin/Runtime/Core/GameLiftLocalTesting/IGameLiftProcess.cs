// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting
{
    public interface IGameLiftProcess
    {
        StartResponse Start(StartRequest request);

        StopResponse Stop(StopRequest request);

        RunLocalServerResponse RunLocalServer(RunLocalServerRequest request);
    }
}
