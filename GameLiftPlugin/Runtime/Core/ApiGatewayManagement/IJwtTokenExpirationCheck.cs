// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public interface IJwtTokenExpirationCheck
    {
        (bool success, string errorCode, string token) RefreshTokenIfExpired(
            ApiGatewayRequest request, IUserIdentity userIdentity);
    }
}
