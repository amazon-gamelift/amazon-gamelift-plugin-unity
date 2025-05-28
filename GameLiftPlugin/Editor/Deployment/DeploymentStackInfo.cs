// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    internal readonly struct DeploymentStackInfo : IEquatable<DeploymentStackInfo>
    {
        public string StackId { get;  }
        public string Details { get; }
        public string Status { get; }
        public string StackStatus { get; }
        public string ApiGatewayEndpoint { get; }
        public string UserPoolClientId { get; }

        public DeploymentStackInfo(string formatterdStatus, string stackStatus = null, string details = null, string apiGatewayEndpoint = null, string userPoolClientId = null, string stackId = null)
        {
            Status = formatterdStatus ?? throw new ArgumentNullException(nameof(formatterdStatus));
            StackId = stackId;
            StackStatus = stackStatus;
            Details = details;
            ApiGatewayEndpoint = apiGatewayEndpoint;
            UserPoolClientId = userPoolClientId;
        }

        public override bool Equals(object obj)
        {
            return obj is DeploymentStackInfo info && Equals(info);
        }

        public bool Equals(DeploymentStackInfo other)
        {
            return Details == other.Details
                && Status == other.Status
                && ApiGatewayEndpoint == other.ApiGatewayEndpoint
                && UserPoolClientId == other.UserPoolClientId;
        }

        public override int GetHashCode()
        {
            EqualityComparer<string> comparer = EqualityComparer<string>.Default;
            int hashCode = 85359248;
            const int c = -1521134295;
            hashCode = hashCode * c + comparer.GetHashCode(Details);
            hashCode = hashCode * c + comparer.GetHashCode(Status);
            hashCode = hashCode * c + comparer.GetHashCode(ApiGatewayEndpoint);
            hashCode = hashCode * c + comparer.GetHashCode(UserPoolClientId);
            return hashCode;
        }
    }
}
