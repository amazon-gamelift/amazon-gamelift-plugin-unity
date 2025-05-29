// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.CloudFormation.Model;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public interface IAmazonCloudFormationWrapper
    {
        ValidateTemplateResponse ValidateTemplate(ValidateTemplateRequest request);

        DescribeStacksResponse DescribeStacks(DescribeStacksRequest request);

        DescribeStackResourceResponse DescribeStackResource(DescribeStackResourceRequest request);

        CreateChangeSetResponse CreateChangeSet(CreateChangeSetRequest request);

        DescribeChangeSetResponse DescribeChangeSet(DescribeChangeSetRequest request);

        ExecuteChangeSetResponse ExecuteChangeSet(ExecuteChangeSetRequest request);

        CancelUpdateStackResponse CancelDeployment(CancelUpdateStackRequest request);

        DeleteChangeSetResponse DeleteChangeSet(DeleteChangeSetRequest request);

        DeleteStackResponse DeleteStack(DeleteStackRequest request);
    }
}
