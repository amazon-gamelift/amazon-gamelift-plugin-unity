// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.CredentialManagement.Models
{
    public class GetCredentialsFileRequest
    {
    }

    public class GetCredentialsFileResponse : Response
    {
        public string FilePath { get; set; }
    }
}
