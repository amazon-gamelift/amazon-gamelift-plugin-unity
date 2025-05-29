// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.Shared
{
    /// <summary>
    /// Response Base Class
    /// </summary>
    public class Response
    {
        public bool Success { get; private set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public static T Ok<T>(T response) where T : Response
        {
            response.Success = true;
            response.ErrorCode = default;
            response.ErrorMessage = default;
            return response;
        }

        public static T Fail<T>(T response) where T : Response
        {
            response.Success = false;
            return response;
        }

        public static Response Fail(string errorCode, string errorMessage = null)
        {
            var response = new Response()
            {
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Success = false
            };
            return response;
        }
    }
}
