// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime.Internal;
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class GameLiftFleetManager
    {
        
        private const string FleetLocation = "custom-location-1";
        private const string FleetDescription = "Deployed by the Amazon GameLift Plug-in for Unity.";
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _fleetName;
        private string _fleetId;
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftFleetManager(IAmazonGameLiftWrapper amazonGameLiftWrapper)
        {
            _amazonGameLiftWrapper = amazonGameLiftWrapper;
        }

        public async Task<CreateFleetResponse> CreateFleet(string fleetName, string fleetLocation)
        {
            if (_amazonGameLiftWrapper == null)
            {
                return Response.Fail(new CreateFleetResponse { ErrorCode = ErrorCode.AccountProfileMissing });
            }
            
            if (string.IsNullOrWhiteSpace(fleetName))
            {
                return Response.Fail(new CreateFleetResponse { ErrorCode = ErrorCode.InvalidFleetName });
            }

            try
            {
                var createFleetRequest = new CreateFleetRequest
                {
                    Name = fleetName,
                    ComputeType = ComputeType.ANYWHERE,
                    Description = $"{FleetDescription} Using Unity Engine Version {UnityEngineVersionUtil.CurrentVersion}",
                    Locations = new List<LocationConfiguration>
                    {
                        new()
                        {
                            Location = fleetLocation,
                        }
                    },
                    Tags = new List<Tag>
                    {
                        { new() { Key = "CreatedBy", Value = "AmazonGameLiftUnityPlugin" } }
                    }
                };
                
                var createFleetResponse = await _amazonGameLiftWrapper.CreateFleet(createFleetRequest);
                
                return Response.Ok(new CreateFleetResponse()
                {
                    FleetId = createFleetResponse.FleetAttributes.FleetId,
                    FleetName = fleetName
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new CreateFleetResponse()
                {
                    ErrorCode = ErrorCode.CreateFleetFailed,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task<CreateCustomLocationResponse> CreateCustomLocationIfNotExists()
        {
            
            if (_amazonGameLiftWrapper == null)
            {
                return Response.Fail(new CreateCustomLocationResponse { ErrorCode = ErrorCode.AccountProfileMissing });
            }
            
            try
            {
                var listLocationsResponse = await _amazonGameLiftWrapper.ListLocations(new ListLocationsRequest
                {
                    Filters = new List<string> { "CUSTOM" },
                });

                var foundLocation = listLocationsResponse.Locations
                    .FirstOrDefault(l => l.LocationName.ToString() == FleetLocation);

                if (foundLocation == null)
                {
                    await _amazonGameLiftWrapper.CreateLocation(new CreateLocationRequest()
                    {
                        LocationName = FleetLocation
                    });
                }

                return Response.Ok(new CreateCustomLocationResponse
                {
                    Location = FleetLocation
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new CreateCustomLocationResponse()
                { 
                    ErrorCode = ErrorCode.CustomLocationCreationFailed, 
                    ErrorMessage = ex.Message 
                });
            }
        }

        public async Task<List<FleetAttributes>> DescribeFleetAttributes(ComputeType computeType)
        {
            try
            {
                var listFleetRequest = new ListFleetsRequest();
                var listFleetResponse = await _amazonGameLiftWrapper.ListFleets(listFleetRequest);

                var describeFleetRequest = new DescribeFleetAttributesRequest()
                {
                    FleetIds = listFleetResponse.FleetIds
                };

                var describeFleetResponse = await _amazonGameLiftWrapper.DescribeFleetAttributes(describeFleetRequest);
                return describeFleetResponse.FleetAttributes.Where(fleet => fleet.ComputeType == computeType)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }
        
        public async Task<FindFleetLocationResponse> FindFirstFleetLocation(string fleetId)
        {
            try
            {
                var describeFleetLocationAttributesRequest = new DescribeFleetLocationAttributesRequest()
                {
                    FleetId = fleetId
                };

                var describeFleetUtilizationResponse =
                    await _amazonGameLiftWrapper.DescribeFleetLocationAttributes(describeFleetLocationAttributesRequest);

                return Response.Ok(new FindFleetLocationResponse
                {
                    Location = describeFleetUtilizationResponse.LocationAttributes[0].LocationState.Location
                });
            }
            catch (Exception ex)
            {
                return Response.Fail(new FindFleetLocationResponse()
                { 
                    ErrorCode = ErrorCode.CustomLocationCreationFailed, 
                    ErrorMessage = ex.Message 
                });
            }
        }
    }
}