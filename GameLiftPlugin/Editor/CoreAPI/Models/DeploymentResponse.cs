// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public sealed class DeploymentResponse : Response
    {
        public DeploymentId DeploymentId { get; }

        public DeploymentResponse()
        {
        }

        public DeploymentResponse(Response failedResponse)
        {
            if (failedResponse is null)
            {
                throw new ArgumentNullException(nameof(failedResponse));
            }

            ErrorCode = failedResponse.ErrorCode;
            ErrorMessage = failedResponse.ErrorMessage;
        }

        public DeploymentResponse(DeploymentId deploymentId) => DeploymentId = deploymentId;

        public DeploymentResponse(string errorCode, string errorMessage = null)
        {
            if (errorCode is null)
            {
                throw new ArgumentNullException(nameof(errorCode));
            }

            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
