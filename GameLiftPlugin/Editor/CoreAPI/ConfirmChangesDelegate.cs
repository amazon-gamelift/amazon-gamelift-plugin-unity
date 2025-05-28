// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;

namespace AmazonGameLift.Editor
{
    public delegate Task<bool> ConfirmChangesDelegate(ConfirmChangesRequest changes);
}
