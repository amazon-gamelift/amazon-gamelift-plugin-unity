// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal static class DeploymentIdContainerFactory
    {
        private static IDeploymentIdContainer s_cachedContainer;

        public static IDeploymentIdContainer Create()
        {
            return s_cachedContainer ?? (s_cachedContainer = new DeploymentIdContainer());
        }
    }
}
