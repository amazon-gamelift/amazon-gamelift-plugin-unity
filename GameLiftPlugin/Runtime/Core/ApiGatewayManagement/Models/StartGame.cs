// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models
{
    public class StartGameRequest : ApiGatewayRequest
    {
        public Dictionary<string, long> RegionLatencies { get; set; }
    }

    public class StartGameResponse : Response
    {
        public string IdToken { get; set; }
    }
}
