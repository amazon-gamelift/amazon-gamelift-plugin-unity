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
    public class GameLiftCGDManager
    {
      
        private readonly IAmazonGameLiftWrapper _amazonGameLiftWrapper;
        private string _containerGroupDefinitionName;
        private VisualElement _container;
        private ErrorResponse _logger;

        public GameLiftCGDManager(IAmazonGameLiftWrapper amazonGameLiftWrapper)
        {
            _amazonGameLiftWrapper = amazonGameLiftWrapper;
        }

        public async Task<ContainerGroupDefinition> GetContainerGroupDefinition(string _cgdName, bool enableLogging)
        {
            try
            {
                var describeContainerGroupDefinitionRequest = new DescribeContainerGroupDefinitionRequest()
                {
                    Name = _cgdName
                };

                var describeContainerGroupDefinitionResponse =
                    await _amazonGameLiftWrapper.DescribeContainerGroupDefinition(describeContainerGroupDefinitionRequest);

                return describeContainerGroupDefinitionResponse.ContainerGroupDefinition;
            }
            catch (Exception ex)
            {
                if (enableLogging)
                {
                    Debug.LogError(ex.Message);
                }
                return null;
            }
        }

        public async Task<ContainerGroupDefinition> GetContainerGroupDefinition(string _cgdName)
        {
            return await GetContainerGroupDefinition(_cgdName, true);
        }
    }


}