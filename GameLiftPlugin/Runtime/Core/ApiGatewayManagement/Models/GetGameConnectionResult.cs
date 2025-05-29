// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models
{
    public class GetGameConnectionResult
    {
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string DnsName { get; set; }
        public string PlayerSessionId { get; set; }
    }
}
