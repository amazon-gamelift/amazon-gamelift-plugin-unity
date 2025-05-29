// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.Shared.FileZip
{
    public interface IFileZip
    {
        void Zip(string sourcePathDirectory, string zipFilePath);
    }
}
