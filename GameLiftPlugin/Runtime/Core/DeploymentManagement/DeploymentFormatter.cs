// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public sealed class DeploymentFormatter
    {
        public string GetServerGamePath(string gameFilePathInBuild, string operatingSystem)
        {
            if (gameFilePathInBuild is null)
            {
                throw new ArgumentNullException(nameof(gameFilePathInBuild));
            }

            if (operatingSystem.Equals(OperatingSystem.WINDOWS_2016.ToString()))
                return $"C:\\game\\{gameFilePathInBuild}";
            return $"/local/game/{gameFilePathInBuild.Replace("\\", "/")}";
        }

        public string GetBuildS3Key()
        {
            string timeStamp = DateTime.Now.Ticks.ToString();
            return $"GameLift_Build_{timeStamp}.zip";
        }

        public string GetStackName(string gameName)
        {
            if (string.IsNullOrEmpty(gameName))
            {
                throw new ArgumentNullException(nameof(gameName));
            }

            return $"GameLiftPluginForUnity-{gameName}";
        }

        public string GetStackNameContainers(string gameName)
        {
            if (string.IsNullOrEmpty(gameName))
            {
                throw new ArgumentNullException(nameof(gameName));
            }

            return $"GameLiftPluginForUnity-{gameName}-Containers";

        }

        public string GetChangeSetName() => $"changeset-{Guid.NewGuid()}";

        public string GetCloudFormationFileKey(string fileName)
        {
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string timeStamp = DateTime.Now.Ticks.ToString();
            string[] parts = fileName.Split('.');
            string name = parts.First();
            string extension = parts.Last();
            return $"CloudFormation/{name}_{timeStamp}.{extension}";
        }

        public string GetS3Url(string bucketName, string region, string fileName)
        {
            if (bucketName is null)
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (region is null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return $"https://{bucketName}.s3.{region}.amazonaws.com/{fileName}";
        }

        public string GetLambdaS3Key(string gameName)
        {
            if (gameName is null)
            {
                throw new ArgumentNullException(nameof(gameName));
            }

            return $"functions/gamelift/GameLift_{gameName}_{DateTime.Now.Ticks}.zip";
        }
    }
}
