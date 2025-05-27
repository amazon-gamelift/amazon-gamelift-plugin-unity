// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.ECR.Model;
using AmazonGameLiftPlugin.Core.Shared;
using System.Collections.Generic;

namespace AmazonGameLiftPlugin.Core.ContainerManagement.Models
{
    public class DescribeECRRepositoriesRequest
    {
    }

    public class DescribeECRRepositoriesResponse : Response
    {
        public IEnumerable<Repository> ECRRepositories { get; set; }
    }
}

