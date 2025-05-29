// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class CreateChangeSetRequest
    {
        public string StackName { get; set; }

        public string TemplateFilePath { get; set; }

        public string ParametersFilePath { get; set; }

        public string BootstrapBucketName { get; set; }

        public string LambdaSourcePath { get; set; }

        public string GameName { get; set; }

        public string BuildS3Key { get; set; }
    }

    public class CreateChangeSetResponse : Response
    {
        public string CreatedChangeSetName { get; set; }
    }
}
