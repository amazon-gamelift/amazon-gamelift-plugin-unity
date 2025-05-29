// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;

[Serializable]
public struct ClientCredentials
{
    public string AccessToken;
    public string IdToken;
    public string RefreshToken;
}
