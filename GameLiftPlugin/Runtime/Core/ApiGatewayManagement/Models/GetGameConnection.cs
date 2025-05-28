// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models
{
    public class GetGameConnectionRequest : ApiGatewayRequest
    {

    }

    public class GetGameConnectionResponse : Response
    {
        public string IdToken { get; set; }
        public string IpAddress { get; set; }
        public string DnsName { get; set; }
        public string Port { get; set; }
        public string PlayerSessionId { get; set; }
        public bool Ready { get; set; }
    }
}
