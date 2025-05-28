// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models
{
    public class ApiGatewayRequest
    {
        public string ApiGatewayEndpoint { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }
}
