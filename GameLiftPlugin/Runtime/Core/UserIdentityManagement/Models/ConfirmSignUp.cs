// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement.Models
{
    public class ConfirmSignUpRequest
    {
        public string ClientId { get; set; }
        public string Username { get; set; }
        public string ConfirmationCode { get; set; }
    }

    public class ConfirmSignUpResponse : Response
    {

    }
}
