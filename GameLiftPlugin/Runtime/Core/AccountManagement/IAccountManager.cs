// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.AccountManagement.Models;

namespace AmazonGameLiftPlugin.Core.AccountManagement
{
    public interface IAccountManager
    {
        RetrieveAccountIdByCredentialsResponse RetrieveAccountIdByCredentials(RetrieveAccountIdByCredentialsRequest request);
    }
}
