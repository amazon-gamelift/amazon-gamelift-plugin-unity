// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.ECR.Model;
using AmazonGameLiftPlugin.Core.ContainerManagement.Models;
using System.Collections.Generic;

namespace AmazonGameLiftPlugin.Core.ContainerManagement
{
    public interface IAmazonECRWrapper
    {
        DescribeECRRepositoriesResponse DescribeECRRepositories(List<string> RepositoryNames = null);
        ListECRImagesResponse ListECRImages(string repositoryName);
        CreateECRRepositoryResponse CreateRepository(string repositoryName);
    }
}
