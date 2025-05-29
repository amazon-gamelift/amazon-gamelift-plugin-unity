// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.JavaCheck.Models
{
    public class CheckInstalledJavaVersionRequest
    {
        public int ExpectedMinimumJavaMajorVersion { get; set; }
    }

    public class CheckInstalledJavaVersionResponse : Response
    {
        public bool IsInstalled { get; set; }
    }
}
