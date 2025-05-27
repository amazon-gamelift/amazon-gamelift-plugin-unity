// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public class AmazonCloudFormationWrapper : IAmazonCloudFormationWrapper
    {
        private readonly IAmazonCloudFormation _amazonCloudFormation;

        public AmazonCloudFormationWrapper(string accessKey, string secretKey, string region)
        {
            _amazonCloudFormation = new AmazonCloudFormationClient(
                    accessKey,
                    secretKey,
                    AwsRegionMapper.GetRegionEndpoint(region)
                );
        }

        public CreateChangeSetResponse CreateChangeSet(CreateChangeSetRequest request)
        {
            return _amazonCloudFormation.CreateChangeSet(request);
        }

        public DescribeChangeSetResponse DescribeChangeSet(DescribeChangeSetRequest request)
        {
            return _amazonCloudFormation.DescribeChangeSet(request);
        }

        public DescribeStacksResponse DescribeStacks(DescribeStacksRequest request)
        {
            return _amazonCloudFormation.DescribeStacks(request);
        }

        public DescribeStackResourceResponse DescribeStackResource(DescribeStackResourceRequest request)
        {
            return _amazonCloudFormation.DescribeStackResource(request);
        }

        public ValidateTemplateResponse ValidateTemplate(ValidateTemplateRequest request)
        {
            return _amazonCloudFormation.ValidateTemplate(request);
        }

        public ExecuteChangeSetResponse ExecuteChangeSet(ExecuteChangeSetRequest request)
        {
            return _amazonCloudFormation.ExecuteChangeSet(request);
        }

        public CancelUpdateStackResponse CancelDeployment(CancelUpdateStackRequest request)
        {
            return _amazonCloudFormation.CancelUpdateStack(request);
        }

        public DeleteChangeSetResponse DeleteChangeSet(DeleteChangeSetRequest request)
        {
            return _amazonCloudFormation.DeleteChangeSet(request);
        }

        public DeleteStackResponse DeleteStack(DeleteStackRequest request)
        {
            return _amazonCloudFormation.DeleteStack(request);
        }
    }
}
