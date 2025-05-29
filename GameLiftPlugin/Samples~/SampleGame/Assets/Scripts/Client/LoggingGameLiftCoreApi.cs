// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Collections.Generic;
using System.Threading.Tasks;
using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;

public sealed class LoggingGameLiftCoreApi : GameLiftCoreApi
{
    private readonly Logger _logger = Logger.SharedInstance;

    public LoggingGameLiftCoreApi(GameLiftConfiguration configuration) : base(configuration)
    {
    }

    public override async Task<GetLatenciesResponse> GetLatencies(string[] regions)
    {
        GetLatenciesResponse response = await base.GetLatencies(regions);
        _logger.Write($"{nameof(GameLiftCoreApi)}.{nameof(GetLatencies)} {FormatResponse(response)}");
        return response;
    }

    public override async Task<StartGameResponse> StartGame(string idToken, string refreshToken, Dictionary<string, long> latencies)
    {
        StartGameResponse startGameResponse = await base.StartGame(idToken, refreshToken, latencies);
        _logger.Write($"{nameof(GameLiftCoreApi)}.{nameof(StartGame)} {FormatResponse(startGameResponse)}");
        return startGameResponse;
    }

    public override async Task<GetGameConnectionResponse> GetGameConnection(string idToken, string refreshToken)
    {
        GetGameConnectionResponse getGameConnectionResponse = await base.GetGameConnection(idToken, refreshToken);
        _logger.Write($"{nameof(GameLiftCoreApi)}.{nameof(GetGameConnection)} ready={getGameConnectionResponse.Ready} {FormatResponse(getGameConnectionResponse)}");
        return getGameConnectionResponse;
    }

    public override SignOutResponse SignOut(string accessToken)
    {
        SignOutResponse signOutResponse = base.SignOut(accessToken);
        _logger.Write($"{nameof(GameLiftCoreApi)}.{nameof(SignOut)} {FormatResponse(signOutResponse)}");
        return signOutResponse;
    }

    private string FormatResponse(Response response)
    {
        return $"success={response.Success}, error={response.ErrorCode}, message={response.ErrorMessage}";
    }
}

#endif
