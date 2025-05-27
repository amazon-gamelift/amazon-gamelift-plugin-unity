// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public interface IDeploymentManager
    {
        DescribeStackResponse DescribeStack(DescribeStackRequest request);

        DescribeStackResourceResponse DescribeStackResource(DescribeStackResourceRequest request);

        ValidateCfnTemplateResponse ValidateCfnTemplate(ValidateCfnTemplateRequest request);

        CreateChangeSetResponse CreateChangeSet(CreateChangeSetRequest request);

        DescribeChangeSetResponse DescribeChangeSet(DescribeChangeSetRequest request);

        StackExistsResponse StackExists(StackExistsRequest request);

        ExecuteChangeSetResponse ExecuteChangeSet(ExecuteChangeSetRequest request);

        UploadServerBuildResponse UploadServerBuild(UploadServerBuildRequest request);

        CancelDeploymentResponse CancelDeployment(CancelDeploymentRequest request);

        DeleteChangeSetResponse DeleteChangeSet(DeleteChangeSetRequest request);

        DeleteStackResponse DeleteStack(DeleteStackRequest request);
    }
}
