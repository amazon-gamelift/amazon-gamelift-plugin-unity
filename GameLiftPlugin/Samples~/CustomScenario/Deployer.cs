// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using JetBrains.Annotations;
using UnityEngine;

namespace AmazonGameLift.Custom
{
    [UsedImplicitly]
    public sealed class Deployer : DeployerBase
    {
        public override string DisplayName => "Custom Scenario";

        public override string Description =>
            "This is a custom deployment scenario for you to edit!";

        public override string HelpUrl => "";

        public override string ScenarioFolder => "Assets/Editor/Custom Scenario";

        public override bool HasGameServer => false;

        public override int PreferredUiOrder => 99;

        protected override Task<DeploymentResponse> Deploy(DeploymentRequest request)
        {
            Debug.Log($"Scenario lambdas path: {request.LambdaFolderPath}");
            return Task.FromResult(Response.Fail(new DeploymentResponse { ErrorCode = Editor.ErrorCode.OperationCancelled }));
        }
    }
}
