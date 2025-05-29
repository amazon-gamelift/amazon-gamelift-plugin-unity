// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    internal class TestedDeployer : DeployerBase
    {
        private readonly bool _deployReturnsSuccess;

        public override string DisplayName => "Test";

        public override string Description => "TestDescription";

        public override string HelpUrl => "https://test.com/";

        public override string ScenarioFolder => "TestScenario";

        public override bool HasGameServer { get; }

        public TestedDeployer(Delay delay, CoreApi coreApi, bool deployReturnsSuccess = true, bool hasGameServer = false) : base(delay, coreApi)
        {
            _deployReturnsSuccess = deployReturnsSuccess;
            HasGameServer = hasGameServer;
        }

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
            DeploymentResponse result = _deployReturnsSuccess
                ? Response.Ok(new DeploymentResponse())
                : Response.Fail(new DeploymentResponse());
            return Task.FromResult(result);
        }
    }
}
