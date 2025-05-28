// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;

public sealed class GameLiftClient : IDisposable
{
    private const int MaxConnectionRetryCount = 7;
    private const int InitialConnectionDelayMs = 15000;
    private const int ConnectionRetryDelayMs = 5000;
    private readonly Delay _delay;
    private readonly Logger _logger;
    private ClientCredentials _clientCredentials;

    public GameLiftCoreApi Core { get; }

    public ClientCredentials ClientCredentials { get => _clientCredentials; set => _clientCredentials = value; }

    public GameLiftClient(GameLiftCoreApi coreApi, Delay delay, Logger logger)
    {
        Core = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
        _delay = delay ?? throw new ArgumentNullException(nameof(delay));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SignOutResponse SignOut()
    {
        SignOutResponse signOutResponse = Core.SignOut(ClientCredentials.AccessToken);
        _logger.Write("Client signed out.");
        return signOutResponse;
    }

    public async Task<(bool success, ConnectionInfo connectionInfo)> GetConnectionInfo(CancellationToken cancellationToken = default)
    {
        string ip = null;
        int port = -1;
        string playerSessionId = null;

        GetLatenciesResponse latenciesResponse = await Core.GetLatencies(Core.ListAvailableRegions());
        StartGameResponse startGameResponse = await Core.StartGame(ClientCredentials.IdToken, ClientCredentials.RefreshToken, latenciesResponse.RegionLatencies);

        if (!startGameResponse.Success && startGameResponse.ErrorCode != ErrorCode.ConflictError)
        {
            return (success: false, new ConnectionInfo { IpAddress = ip, Port = port, PlayerSessionId = playerSessionId });
        }

        _clientCredentials.IdToken = startGameResponse.IdToken;
        await _delay.Wait(InitialConnectionDelayMs, cancellationToken);
        int retry = 0;
        int delay = ConnectionRetryDelayMs;

        while (retry < MaxConnectionRetryCount && !cancellationToken.IsCancellationRequested)
        {
            GetGameConnectionResponse connection = await Core.GetGameConnection(ClientCredentials.IdToken, ClientCredentials.RefreshToken);

            if (!connection.Success)
            {
                return (success: false, new ConnectionInfo { IpAddress = ip, Port = port, PlayerSessionId = playerSessionId });
            }

            _clientCredentials.IdToken = connection.IdToken;

            if (connection.Ready)
            {
                ip = connection.DnsName ?? connection.IpAddress;
                port = int.Parse(connection.Port);
                playerSessionId = connection.PlayerSessionId;
                return (success: true, new ConnectionInfo { IpAddress = ip, Port = port, PlayerSessionId = playerSessionId });
            }

            await _delay.Wait(delay, cancellationToken);
            retry++;
            delay *= 2;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return (success: false, new ConnectionInfo { IpAddress = ip, Port = port, PlayerSessionId = playerSessionId });
    }

    public void Dispose()
    {
        SignOut();
    }
}

#endif
