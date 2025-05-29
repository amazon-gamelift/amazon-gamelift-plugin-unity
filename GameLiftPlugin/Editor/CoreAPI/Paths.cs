// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public static class Paths
    {
        public const string PluginSettingsFile = "GameLiftSettings.yaml";
        public const string PackageName = "com.amazonaws.gamelift";
        public const string SampleGameInPackage = "Samples~/SampleGame.unitypackage";
        public const string ScenariosRootInPackage = "Editor/Resources/CloudFormation";
        public const string LambdaFolderPathInScenario = "lambda";
        public const string CfnTemplateFileName = "cloudformation.yml";
        public const string ParametersFileName = "parameters.json";
        public const string ServerSdkDllInPackage = "Runtime/Plugins/GameLiftServerSDKNet45.dll";
        public const string ContainersRootInPackage = "Editor/Resources/Containers";
        public const string ContainersOutputFolderName = "Output~";
        public const string ContainerDockerfileFileName = "SampleDockerfile";
        public const string ContainerPushImageScriptFileName = "PushExistingImageToECRScriptTemplate.ps1";
    }
}
