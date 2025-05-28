// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement;

namespace AmazonGameLiftPlugin.Core
{
    public class AmazonGameLiftWrapper : IAmazonGameLiftWrapper
    {
        private readonly IAmazonGameLift _amazonGameLiftClient;

        public AmazonGameLiftWrapper(IAmazonGameLift amazonGameLiftClient)
        {
            _amazonGameLiftClient = amazonGameLiftClient;
        }
        /// <summary>
        /// Editor region is code dedicated to Amazon GameLift SDK calls made by the Unity Editor Plugin. 
        /// </summary>
        #region Editor
        public async Task<CreateGameSessionResponse> CreateGameSessionAsync(
                CreateGameSessionRequest request,
                CancellationToken cancellationToken = default
            )
        {
            return await _amazonGameLiftClient.CreateGameSessionAsync(request, cancellationToken);
        }

        public async Task<CreatePlayerSessionResponse> CreatePlayerSession(CreatePlayerSessionRequest request)
        {
            return await _amazonGameLiftClient.CreatePlayerSessionAsync(request);
        }

        public async Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request)
        {
            return await _amazonGameLiftClient.SearchGameSessionsAsync(request);
        }

        public async Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request)
        {
            return await _amazonGameLiftClient.DescribeGameSessionsAsync(request);
        }

        public async Task<DescribeContainerGroupDefinitionResponse> DescribeContainerGroupDefinition(DescribeContainerGroupDefinitionRequest request)
        {
            return await _amazonGameLiftClient.DescribeContainerGroupDefinitionAsync(request);
        }

        #endregion
        /// <summary>
        /// Server region is code dedicated to Amazon GameLift SDK and AWS SDK calls made by the game server. All of these calls will be done via UI Elements or on Startup. 
        /// </summary>
        #region Server
        public async Task<ListLocationsResponse> ListLocations(ListLocationsRequest request)
        {
            return await _amazonGameLiftClient.ListLocationsAsync(request);
        }

        public async Task<CreateLocationResponse> CreateLocation(CreateLocationRequest request)
        {
            return await _amazonGameLiftClient.CreateLocationAsync(request);
        }

        public async Task<RegisterComputeResponse> RegisterCompute(RegisterComputeRequest request)
        {
            return await _amazonGameLiftClient.RegisterComputeAsync(request);
        }

        public async Task<GetComputeAuthTokenResponse> GetComputeAuthToken(GetComputeAuthTokenRequest request)
        {
            return await _amazonGameLiftClient.GetComputeAuthTokenAsync(request);
        }

        public Task<CreateFleetResponse> CreateFleet(CreateFleetRequest request)
        {
            return _amazonGameLiftClient.CreateFleetAsync(request);
        }

        public Task<DescribeFleetAttributesResponse> DescribeFleetAttributes(DescribeFleetAttributesRequest request)
        {
            return _amazonGameLiftClient.DescribeFleetAttributesAsync(request);
        }

        public Task<DeregisterComputeResponse> DeregisterCompute(DeregisterComputeRequest request)
        {
            return _amazonGameLiftClient.DeregisterComputeAsync(request);
        }

        public Task<ListComputeResponse> ListCompute(ListComputeRequest request)
        {
            return _amazonGameLiftClient.ListComputeAsync(request);
        }

        public Task<ListFleetsResponse> ListFleets(ListFleetsRequest request)
        {
            return _amazonGameLiftClient.ListFleetsAsync(request);
        }
        
        public Task<DescribeFleetLocationAttributesResponse> DescribeFleetLocationAttributes(DescribeFleetLocationAttributesRequest request)
        {
            return _amazonGameLiftClient.DescribeFleetLocationAttributesAsync(request);
        }

        #endregion
    }
}