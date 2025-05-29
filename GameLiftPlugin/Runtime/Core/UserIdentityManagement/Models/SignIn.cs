// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement.Models
{
    public class SignInRequest
    {
        public string ClientId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignInResponse : Response
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
