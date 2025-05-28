// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace AmazonGameLiftPlugin.Core.Shared.FileSystem
{
    public interface IFileWrapper
    {
        bool FileExists(string path);

        string ReadAllText(string path);

        void WriteAllText(string path, string text);

        StreamWriter CreateText(string path);

        void Delete(string path);

        bool DirectoryExists(string path);

        void CreateDirectory(string directoryPath);

        string GetUniqueTempFilePath();
    }
}
