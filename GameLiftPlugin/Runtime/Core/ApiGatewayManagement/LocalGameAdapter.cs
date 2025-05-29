// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class LocalGameAdapter : IGameServerAdapter
    {
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private readonly string _fleetId = "fleet-123";
        private readonly string _playerIdPrefix = "playerId-";

        public LocalGameAdapter(IAmazonGameLiftWrapper amazonGameLiftWrapper)
        {
            _amazonGameLiftWrapper = amazonGameLiftWrapper;
        }

        public async Task<GetGameConnectionResponse> GetGameConnection(GetGameConnectionRequest request)
        {
            try
            {
                Amazon.GameLift.Model.DescribeGameSessionsResponse describeGameSessionsResponse = await _amazonGameLiftWrapper.DescribeGameSessions(new Amazon.GameLift.Model.DescribeGameSessionsRequest
                {
                    FleetId = _fleetId,
                });

                if (describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.GameSession oldestGameSession = describeGameSessionsResponse.GameSessions.First();

                    Amazon.GameLift.Model.CreatePlayerSessionResponse createPlayerSessionResponse = await _amazonGameLiftWrapper.CreatePlayerSession(new Amazon.GameLift.Model.CreatePlayerSessionRequest
                    {
                        GameSessionId = oldestGameSession.GameSessionId,
                        PlayerId = _playerIdPrefix + Guid.NewGuid().ToString()
                    });
                    Amazon.GameLift.Model.PlayerSession playerSession = createPlayerSessionResponse.PlayerSession;

                    var response = new GetGameConnectionResponse
                    {
                        IpAddress = playerSession.IpAddress,
                        DnsName = playerSession.DnsName,
                        Port = playerSession.Port.ToString(),
                        PlayerSessionId = playerSession.PlayerSessionId,
                        Ready = true
                    };

                    return Response.Ok(response);
                }

                return Response.Fail(new GetGameConnectionResponse
                {
                    ErrorCode = ErrorCode.NoGameSessionWasFound
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new GetGameConnectionResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<StartGameResponse> StartGame(StartGameRequest request)
        {
            try
            {
                Amazon.GameLift.Model.DescribeGameSessionsResponse describeGameSessionsResponse = await _amazonGameLiftWrapper.DescribeGameSessions(new Amazon.GameLift.Model.DescribeGameSessionsRequest
                {
                    FleetId = _fleetId,
                });

                if (!describeGameSessionsResponse.GameSessions.Any())
                {
                    Amazon.GameLift.Model.CreateGameSessionResponse createGameSessionResponse = await _amazonGameLiftWrapper.CreateGameSessionAsync(new Amazon.GameLift.Model.CreateGameSessionRequest
                    {
                        MaximumPlayerSessionCount = 4,
                        FleetId = _fleetId
                    });
                }

                return Response.Ok(new StartGameResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StartGameResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
