// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLiftPlugin.Core.Shared.ProcessManagement
{
    public class ExecutableNotFoundException : Exception
    {
        public ExecutableNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
