// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal interface IDeploymentIdContainer
    {
        bool HasValue { get; }

        DeploymentId Get();

        void Set(DeploymentId value);

        void Clear();
    }
}
