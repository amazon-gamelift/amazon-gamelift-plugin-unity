// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal class DeploymentIdContainer : IDeploymentIdContainer
    {
        private DeploymentId? _deploymentId;

        public bool HasValue => _deploymentId.HasValue;

        public void Clear()
        {
            _deploymentId = null;
        }

        public DeploymentId Get()
        {
            return _deploymentId.Value;
        }

        public void Set(DeploymentId value)
        {
            _deploymentId = value;
        }
    }
}
