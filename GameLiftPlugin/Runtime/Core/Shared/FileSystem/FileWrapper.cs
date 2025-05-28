// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace AmazonGameLiftPlugin.Core.Shared.FileSystem
{
    public class FileWrapper : IFileWrapper
    {
        public bool FileExists(string path) => File.Exists(path);

        public string ReadAllText(string path) => File.ReadAllText(path);

        public void WriteAllText(string path, string text) => File.WriteAllText(path, text);

        public StreamWriter CreateText(string path) => File.CreateText(path);

        public void Delete(string path) => File.Delete(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public void CreateDirectory(string directoryPath)
            => Directory.CreateDirectory(directoryPath);

        public string GetUniqueTempFilePath() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
}
