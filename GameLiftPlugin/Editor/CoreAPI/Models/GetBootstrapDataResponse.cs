// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public sealed class GetBootstrapDataResponse : Response
    {
        public string Profile { get; }

        public string Region { get; }

        internal GetBootstrapDataResponse(string profile, string region)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        internal GetBootstrapDataResponse()
        {
        }
    }
}
