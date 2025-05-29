// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Threading.Tasks;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public interface IHttpClientWrapper
    {
        Task<(HttpStatusCode statusCode, string body)> Post(string endpoint, string token, string path, string body = null);
    }
}
