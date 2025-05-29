// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift.Model;

namespace AmazonGameLiftPlugin.Core
{
    public interface IAmazonGameLiftWrapper
    {
        Task<CreateGameSessionResponse> CreateGameSessionAsync(
            CreateGameSessionRequest request,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Editor region is code dedicated to Amazon GameLift SDK calls made by the Unity Editor Plugin. 
        /// </summary>
        #region Editor
        Task<CreatePlayerSessionResponse> CreatePlayerSession(CreatePlayerSessionRequest request);

        Task<SearchGameSessionsResponse> SearchGameSessions(SearchGameSessionsRequest request);

        Task<DescribeGameSessionsResponse> DescribeGameSessions(DescribeGameSessionsRequest request);

        Task<DescribeContainerGroupDefinitionResponse> DescribeContainerGroupDefinition(DescribeContainerGroupDefinitionRequest request);
        #endregion

        /// <summary>
        /// Server region is code dedicated to Amazon GameLift SDK and AWS SDK calls made by the game server. All of these calls will be done via UI Elements or on Startup. 
        /// </summary>
        #region Server
        Task<ListLocationsResponse> ListLocations(ListLocationsRequest request);
        
        Task<CreateLocationResponse> CreateLocation(CreateLocationRequest request);
        
        Task<RegisterComputeResponse> RegisterCompute(RegisterComputeRequest request);
        
        Task<GetComputeAuthTokenResponse> GetComputeAuthToken(GetComputeAuthTokenRequest request);
        
        Task<CreateFleetResponse> CreateFleet(CreateFleetRequest request);
        
        Task<DescribeFleetAttributesResponse> DescribeFleetAttributes(DescribeFleetAttributesRequest request);
        
        Task<DeregisterComputeResponse> DeregisterCompute(DeregisterComputeRequest request);
        
        Task<ListComputeResponse> ListCompute(ListComputeRequest request);
        
        Task<ListFleetsResponse> ListFleets(ListFleetsRequest request);
        
        Task<DescribeFleetLocationAttributesResponse> DescribeFleetLocationAttributes(DescribeFleetLocationAttributesRequest request);
        #endregion
    }
}