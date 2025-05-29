// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.SingleFleet
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "Single-Region Fleet";

        public override string Description =>
            "This CloudFormation template sets up a game backend service with a single Amazon GameLift fleet. "
            + "After player authenticates and start a game via POST /start_game, a lambda handler searches for an existing viable game session with open player slot on the fleet, and if not found, creates a new game session. "
            + "The game client is then expected to poll POST /get_game_connection to receive a viable game session.";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario1_single_fleet";

        public override bool HasGameServer => true;

        public override int PreferredUiOrder => 2;

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
            string zipPath = GameLiftCoreApi.GetUniqueTempFilePath();
            GameLiftCoreApi.Zip(request.BuildFolderPath, zipPath);

            UploadServerBuildResponse uploadBuildResponse = GameLiftCoreApi.UploadServerBuild(
                request.Profile, request.Region, request.BucketName, request.BuildS3Key, zipPath);

            if (GameLiftCoreApi.FileExists(zipPath))
            {
                GameLiftCoreApi.FileDelete(zipPath);
            }

            if (!uploadBuildResponse.Success)
            {
                return Task.FromResult(Response.Fail(new DeploymentResponse(uploadBuildResponse)));
            }

            ExecuteChangeSetResponse executeResponse = GameLiftCoreApi.ExecuteChangeSet(
                request.Profile, request.Region, request.StackName, request.ChangeSetName);

            if (!executeResponse.Success)
            {
                return Task.FromResult(Response.Fail(new DeploymentResponse(executeResponse)));
            }

            return Task.FromResult(Response.Ok(new DeploymentResponse()));
        }
    }
}
