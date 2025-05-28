// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public sealed class HttpClientWrapper : IHttpClientWrapper
    {
        private static readonly HttpClient s_httpClient = new HttpClient();

        public async Task<(HttpStatusCode statusCode, string body)> Post(
            string endpoint,
            string token,
            string path,
            string body = null)
        {
            s_httpClient.BaseAddress = new Uri(endpoint);
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.TryAddWithoutValidation("auth", token);

            if (!string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body);
            }

            HttpResponseMessage response = await s_httpClient.SendAsync(request);
            string responseBody = await response.Content?.ReadAsStringAsync();
            return (response.StatusCode, responseBody);
        }
    }
}
