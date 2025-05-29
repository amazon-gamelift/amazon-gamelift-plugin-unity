// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public sealed class SaveParametersResponse : Response
    {
        public string SerializedParameters { get; }

        internal SaveParametersResponse(string serializedParameters) => SerializedParameters = serializedParameters
                ?? throw new ArgumentNullException(nameof(serializedParameters));

        internal SaveParametersResponse()
        {
        }
    }
}
