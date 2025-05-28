// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;
using Newtonsoft.Json;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class ApiGateway : IGameServerAdapter
    {
        private readonly IUserIdentity _userIdentity;
        private readonly IJwtTokenExpirationCheck _jwtTokenExpirationCheck;
        private readonly IHttpClientWrapper _httpWrapper;

        public ApiGateway(
                IUserIdentity userIdentity,
                IJwtTokenExpirationCheck jwtTokenExpirationCheck,
                IHttpClientWrapper httpWrapper
            )
        {
            _userIdentity = userIdentity;
            _jwtTokenExpirationCheck = jwtTokenExpirationCheck;
            _httpWrapper = httpWrapper;
        }

        private bool IsValid(ApiGatewayRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.ApiGatewayEndpoint)
                || string.IsNullOrWhiteSpace(request.ClientId)
                || string.IsNullOrWhiteSpace(request.IdToken)
                || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return false;
            }

            return true;
        }

        public async Task<StartGameResponse> StartGame(StartGameRequest request)
        {
            try
            {
                string latenciesJson = null;

                if (request.RegionLatencies != null)
                {
                    string regionToLatencyMappingJson =
                        JsonConvert.SerializeObject(request.RegionLatencies);

                    latenciesJson =
                        string.Format("{{\"regionToLatencyMapping\":{0}}}", regionToLatencyMappingJson);
                }

                if (!IsValid(request))
                {
                    return Response.Fail(new StartGameResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                (bool success, string errorCode, string token) =
                    _jwtTokenExpirationCheck.RefreshTokenIfExpired(request, _userIdentity);

                if (!success)
                {
                    return Response.Fail(new StartGameResponse
                    {
                        ErrorCode = errorCode
                    });
                }

                (HttpStatusCode statusCode, string body) response = await _httpWrapper.Post(
                        request.ApiGatewayEndpoint,
                        token,
                        "start_game",
                        latenciesJson
                    );

                if (response.statusCode == HttpStatusCode.Conflict)
                {
                    return Response.Fail(new StartGameResponse
                    {
                        ErrorCode = ErrorCode.ConflictError,
                        ErrorMessage = FormatRequestError(response)
                    });
                }

                if (response.statusCode != HttpStatusCode.Accepted)
                {
                    return Response.Fail(new StartGameResponse
                    {
                        ErrorCode = ErrorCode.ApiGatewayRequestError,
                        ErrorMessage = FormatRequestError(response)
                    });
                }

                return Response.Ok(new StartGameResponse
                {
                    IdToken = token
                });
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

        public async Task<GetGameConnectionResponse> GetGameConnection(GetGameConnectionRequest request)
        {
            try
            {
                if (!IsValid(request))
                {
                    return Response.Fail(new GetGameConnectionResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                (bool success, string errorCode, string token) =
                    _jwtTokenExpirationCheck.RefreshTokenIfExpired(request, _userIdentity);

                if (!success)
                {
                    return Response.Fail(new GetGameConnectionResponse
                    {
                        ErrorCode = errorCode
                    });
                }

                (HttpStatusCode statusCode, string body) response = await _httpWrapper.Post(
                        request.ApiGatewayEndpoint,
                        token,
                        "get_game_connection"
                    );

                if (response.statusCode == HttpStatusCode.OK)
                {
                    GetGameConnectionResult result = JsonConvert.DeserializeObject<GetGameConnectionResult>
                        (response.body);

                    return Response.Ok(new GetGameConnectionResponse
                    {
                        Ready = true,
                        IpAddress = result.IpAddress,
                        DnsName = result.DnsName,
                        Port = result.Port,
                        PlayerSessionId = result.PlayerSessionId,
                        IdToken = token
                    });
                }

                if (response.statusCode == HttpStatusCode.NoContent)
                {
                    return Response.Ok(new GetGameConnectionResponse { IdToken = token });
                }

                return Response.Fail(new GetGameConnectionResponse
                {
                    ErrorCode = ErrorCode.ApiGatewayRequestError,
                    ErrorMessage = FormatRequestError(response)
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

        private static string FormatRequestError((HttpStatusCode statusCode, string body) response) =>
            string.Format("Request failed with status {0}. Request body {1}.", (int)response.statusCode, response.body);
    }
}
