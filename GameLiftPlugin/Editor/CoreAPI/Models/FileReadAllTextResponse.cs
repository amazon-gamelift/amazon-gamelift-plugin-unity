// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public sealed class FileReadAllTextResponse : Response
    {
        public string Text { get; }

        internal FileReadAllTextResponse(string text) => Text = text
                ?? throw new ArgumentNullException(nameof(text));

        internal FileReadAllTextResponse()
        {
        }
    }
}
