// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public sealed class DeploymentRequest
    {
        public string Profile { get; set; }
        public string Region { get; set; }
        public string BucketName { get; set; }
        public string CfnTemplatePath { get; set; }
        public string ParametersPath { get; set; }
        public string StackName { get; set; }
        public bool IsDevelopmentBuild { get; set; }
        public string GameName { get; set; }
        public string LambdaFolderPath { get; set; }
        public string ChangeSetName { get; set; }
        public string BuildFolderPath { get; set; }
        public string BuildS3Key { get; set; }
    }
}
