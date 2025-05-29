// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class UploadServerBuildRequest
    {
        public string BucketName { get; set; }

        public string BuildS3Key { get; set; }

        public string FilePath { get; set; }
    }

    public class UploadServerBuildResponse : Response
    {
    }
}
