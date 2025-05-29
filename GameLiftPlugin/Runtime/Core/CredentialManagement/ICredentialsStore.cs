// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.CredentialManagement.Models;

namespace AmazonGameLiftPlugin.Core.CredentialManagement
{
    public interface ICredentialsStore
    {
        SaveAwsCredentialsResponse SaveAwsCredentials(SaveAwsCredentialsRequest request);

        RetriveAwsCredentialsResponse RetriveAwsCredentials(RetriveAwsCredentialsRequest request);

        UpdateAwsCredentialsResponse UpdateAwsCredentials(UpdateAwsCredentialsRequest request);

        GetProfilesResponse GetProfiles(GetProfilesRequest request);

        GetCredentialsFileResponse GetCredentialsFile(GetCredentialsFileRequest request);
    }
}
