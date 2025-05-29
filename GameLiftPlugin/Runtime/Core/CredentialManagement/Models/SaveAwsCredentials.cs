// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.CredentialManagement.Models
{
    public class SaveAwsCredentialsRequest
    {
        public string ProfileName { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }
        
        public string Region { get; set; }
    }

    public class SaveAwsCredentialsResponse : Response
    {
    }
}
