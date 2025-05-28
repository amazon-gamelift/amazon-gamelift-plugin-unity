// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.CredentialManagement.Models
{
    public class RetriveAwsCredentialsRequest
    {
        public string ProfileName { get; set; }
    }

    public class RetriveAwsCredentialsResponse : Response
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }
        
        public string Region { get; set; }
    }
}
