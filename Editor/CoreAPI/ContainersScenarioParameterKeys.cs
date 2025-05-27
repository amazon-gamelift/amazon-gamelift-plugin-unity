// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public static class ContainersScenarioParameterKeys
    {
        public const string GameName = "GameNameParameter";
        public const string ContainerImageUri = "ContainerImageUriParameter";
        public const string ContainerGroupDefinitionName = "ContainerGroupDefinitionNameParameter";
        public const string ContainerGroupDefinitionTotalMemoryLimit = "TotalMemoryLimitParameter";
        public const string ContainerGroupDefinitionTotalVcpuLimit = "TotalVcpuLimitParameter";
        public const string ContainerGroupDefinitionFromPort = "FleetTcpFromPortParameter";
        public const string ContainerGroupDefinitionToPort = "FleetTcpToPortParameter";
        public const string UnityEngineVersion = "UnityEngineVersionParameter";
    }
}
