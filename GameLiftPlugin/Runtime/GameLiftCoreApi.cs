// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#if !UNITY_SERVER

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.GameLift;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Latency;
using AmazonGameLiftPlugin.Core.Latency.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;

namespace AmazonGameLift.Runtime
{
    public class GameLiftCoreApi
    {
        private readonly GameLiftConfiguration _configuration;
        private readonly bool _isAnywhereMode;
        private readonly IGameServerAdapter _gameServerAdapter;

        protected GameLiftCoreApi(GameLiftConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration.IsGameLiftAnywhere)
            {
                var fleetId = configuration.FleetId;
                var fleetLocation = configuration.FleetLocation;
                var credentials = new CredentialsStore(new FileWrapper());
                var credentialsResponse =
                    credentials.RetriveAwsCredentials(new RetriveAwsCredentialsRequest(){ProfileName = configuration.ProfileName});
                var region = configuration.AwsRegion;
                var gameLiftClient = new AmazonGameLiftClient(credentialsResponse.AccessKey, credentialsResponse.SecretKey, RegionEndpoint.GetBySystemName(region));
                var gameLiftClientWrapper = new AmazonGameLiftWrapper(gameLiftClient); 
                _gameServerAdapter = new AnywhereGameServerAdapter(gameLiftClientWrapper, fleetId, fleetLocation);
                _isAnywhereMode = true;
            }
            else
            {
                _userIdentity = new UserIdentity(new AmazonCognitoIdentityWrapper(configuration.AwsRegion));
                _apiGateway = new ApiGateway(_userIdentity, new JwtTokenExpirationCheck(), new HttpClientWrapper());
                _gameServerAdapter = new ApiGateway(_userIdentity, new JwtTokenExpirationCheck(), new HttpClientWrapper());
            }
        }

        #region User Accounts

        private readonly IUserIdentity _userIdentity;

        public virtual SignUpResponse SignUp(string email, string password)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignUpResponse());
            }

            var request = new SignUpRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                Password = password,
            };
            return _userIdentity.SignUp(request);
        }

        public virtual ConfirmSignUpResponse ConfirmSignUp(string email, string confirmationCode)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new ConfirmSignUpResponse());
            }

            var request = new ConfirmSignUpRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
            };
            return _userIdentity.ConfirmSignUp(request);
        }

        public virtual SignInResponse SignIn(string email, string password)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignInResponse()
                {
                    AccessToken = string.Empty,
                    IdToken = string.Empty,
                    RefreshToken = string.Empty,
                });
            }

            var request = new SignInRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                Username = email,
                Password = password,
            };
            return _userIdentity.SignIn(request);
        }

        public virtual SignOutResponse SignOut(string accessToken)
        {
            if (_isAnywhereMode)
            {
                return Response.Ok(new SignOutResponse());
            }

            var request = new SignOutRequest()
            {
                AccessToken = accessToken,
            };
            return _userIdentity.SignOut(request);
        }

        #endregion

        #region Matchmaking

        private readonly ApiGateway _apiGateway;
        private readonly LatencyService _latencyService = new LatencyService(new PingWrapper());

        public virtual string[] ListAvailableRegions()
        {
            return AwsRegionMapper.AvailableRegions().ToArray();
        }

        public virtual Task<GetLatenciesResponse> GetLatencies(string[] regions)
        {
            var request = new GetLatenciesRequest()
            {
                Regions = regions
            };
            return _latencyService.GetLatencies(request);
        }

        public virtual Task<GetGameConnectionResponse> GetGameConnection(string idToken, string refreshToken)
        {
            var request = new GetGameConnectionRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                ApiGatewayEndpoint = _configuration.ApiGatewayEndpoint,
                IdToken = idToken,
                RefreshToken = refreshToken,
            };
            return _gameServerAdapter.GetGameConnection(request);
        }

        public virtual Task<StartGameResponse> StartGame(string idToken, string refreshToken,
            Dictionary<string, long> latencies)
        {
            var request = new StartGameRequest()
            {
                ClientId = _configuration.UserPoolClientId,
                ApiGatewayEndpoint = _configuration.ApiGatewayEndpoint,
                IdToken = idToken,
                RefreshToken = refreshToken,
                RegionLatencies = latencies
            };
            return _gameServerAdapter.StartGame(request);
        }

        #endregion
    }
}

#endif