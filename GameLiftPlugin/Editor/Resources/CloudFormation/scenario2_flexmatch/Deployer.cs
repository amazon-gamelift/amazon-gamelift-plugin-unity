// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.FlexMatch
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "FlexMatch";

        public override string Description =>
            "This CloudFormation template sets up a scenario to use FlexMatch -- a managed matchmaking service provided by GameLift. "
            + "The template demonstrates best practices in acquiring the matchmaking ticket status, "
            + "by listening to FlexMatch events in conjunction with a low frequency poller to ensure incomplete tickets are periodically pinged and therefore are not discarded by GameLift.";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario2_flexmatch";

        public override bool HasGameServer => true;

        public override int PreferredUiOrder => 5;

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
