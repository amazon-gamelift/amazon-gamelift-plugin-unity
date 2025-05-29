// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement.Models
{
    public class SignOutRequest
    {
        public string AccessToken { get; set; }
    }

    public class SignOutResponse : Response
    {        
    }
}
