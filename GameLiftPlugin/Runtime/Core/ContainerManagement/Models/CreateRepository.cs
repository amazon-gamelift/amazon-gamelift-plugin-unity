// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.ContainerManagement.Models
{
    public class CreateECRRepositoryRequest
    {
        public string RepositoryName { get; set; }
    }

    public class CreateECRRepositoryResponse : Response
    {
        public string RepositoryUri { get; set; }
    }
}
