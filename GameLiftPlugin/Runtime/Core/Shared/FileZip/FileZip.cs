// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.IO.Compression;

namespace AmazonGameLiftPlugin.Core.Shared.FileZip
{
    public class FileZip : IFileZip
    {
        public void Zip(string sourcePathDirectory, string zipFilePath)
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(sourcePathDirectory, zipFilePath);
        }
    }
}
