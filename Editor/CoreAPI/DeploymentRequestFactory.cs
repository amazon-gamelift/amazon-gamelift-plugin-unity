// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class DeploymentRequestFactory
    {
        private CoreApi GameLiftCoreApi { get; }

        private bool IsContainersDeployment { get; }

        internal DeploymentRequestFactory(CoreApi coreApi, bool isContainersDeployment)
        {
            GameLiftCoreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            IsContainersDeployment = isContainersDeployment;
        }

        /// <exception cref="ArgumentNullException">For all parameters.</exception>
        internal virtual (DeploymentRequest request, bool success, Response failedResponse) CreateRequest(
            string scenarioFolderPath, string gameName, bool isDevelopmentBuild)
        {
            if (scenarioFolderPath is null)
            {
                throw new ArgumentNullException(nameof(scenarioFolderPath));
            }

            if (gameName is null)
            {
                throw new ArgumentNullException(nameof(gameName));
            }
 
            string stackName = IsContainersDeployment ? GameLiftCoreApi.GetStackNameContainers(gameName) : GameLiftCoreApi.GetStackName(gameName);
            string cfnTemplatePath = Path.Combine(scenarioFolderPath, Paths.CfnTemplateFileName);
            string parametersPath = Path.Combine(scenarioFolderPath, Paths.ParametersFileName);
            string lambdaFolderPath = Path.Combine(scenarioFolderPath, Paths.LambdaFolderPathInScenario);

            GetSettingResponse currentProfileResponse = GameLiftCoreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!currentProfileResponse.Success)
            {
                return (null, false, currentProfileResponse);
            }

            GetSettingResponse currentRegionResponse = GameLiftCoreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!currentRegionResponse.Success)
            {
                return (null, false, currentRegionResponse);
            }

            GetSettingResponse bucketResponse = GameLiftCoreApi.GetSetting(SettingsKeys.CurrentBucketName);

            if (!bucketResponse.Success)
            {
                return (null, false, bucketResponse);
            }

            var request = new DeploymentRequest()
            {
                Profile = currentProfileResponse.Value,
                Region = currentRegionResponse.Value,
                BucketName = bucketResponse.Value,
                StackName = stackName,
                CfnTemplatePath = cfnTemplatePath,
                ParametersPath = parametersPath,
                IsDevelopmentBuild = isDevelopmentBuild,
                GameName = gameName,
                LambdaFolderPath = lambdaFolderPath,
            };
            return (request, true, null);
        }

        /// <exception cref="ArgumentNullException"></exception>
        internal virtual DeploymentRequest WithServerBuild(DeploymentRequest request, string buildFolderPath)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (buildFolderPath is null)
            {
                throw new ArgumentNullException(nameof(buildFolderPath));
            }

            request.BuildFolderPath = buildFolderPath;
            request.BuildS3Key = GameLiftCoreApi.GetBuildS3Key();
            return request;
        }
    }
}
