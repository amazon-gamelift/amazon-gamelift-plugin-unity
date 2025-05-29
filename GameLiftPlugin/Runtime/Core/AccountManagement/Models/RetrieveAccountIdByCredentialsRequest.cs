// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.AccountManagement.Models
{
    public class RetrieveAccountIdByCredentialsRequest
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }
    }

    public class RetrieveAccountIdByCredentialsResponse : Response
    {
        public string AccountId { get; set; }
    }
}
