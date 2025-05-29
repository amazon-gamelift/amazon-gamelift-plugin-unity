// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.UserIdentityManagement;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public class JwtTokenExpirationCheck : IJwtTokenExpirationCheck
    {
        public (bool success, string errorCode, string token) RefreshTokenIfExpired(
            ApiGatewayRequest request, IUserIdentity userIdentity)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            bool isValid = jwtTokenHandler.CanReadToken(request.IdToken);
            if (!isValid)
            {
                return (false, ErrorCode.InvalidIdToken, request.IdToken);
            }

            JwtSecurityToken token = jwtTokenHandler.ReadJwtToken(request.IdToken);
            Claim expDate = token.Claims.First(x => x.Type == "exp");

            long expDateInSeconds = long.Parse(expDate.Value);
            var tokenExpirationDate = DateTimeOffset.FromUnixTimeSeconds(expDateInSeconds);
            if (DateTime.UtcNow > tokenExpirationDate)
            {
                UserIdentityManagement.Models.RefreshTokenResponse refreshTokenResponse =
                    userIdentity.RefreshToken(new UserIdentityManagement.Models.RefreshTokenRequest
                    {
                        ClientId = request.ClientId,
                        RefreshToken = request.RefreshToken
                    });

                return (true, string.Empty, refreshTokenResponse.IdToken);
            }
            else
            {
                return (true, string.Empty, request.IdToken);
            }
        }
    }
}
