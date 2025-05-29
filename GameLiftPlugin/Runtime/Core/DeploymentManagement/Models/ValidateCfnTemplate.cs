// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement.Models
{
    public class ValidateCfnTemplateRequest
    {
        public string TemplateFilePath { get; set; }
    }

    public class ValidateCfnTemplateResponse : Response
    {
    }
}
