// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;

namespace AmazonGameLift.ContainerSingleFleet
{
    [UsedImplicitly]
    public sealed class Deployer : ContainersDeployerBase
    {
        public override string DisplayName => "Single-Region Container Fleet";

        public override string Description => "TBD";

        public override string HelpUrl => "https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in-scenario.html";

        public override string ScenarioFolder => "scenario3_containers_single_fleet";

        public override int PreferredUiOrder => 10;

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
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
