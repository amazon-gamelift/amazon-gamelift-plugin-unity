// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace AmazonGameLiftPlugin.Core.Shared.SettingsStore
{
    public class YamlStreamWrapper : IStreamWrapper
    {
        private readonly YamlStream _yamlStream;

        public YamlStreamWrapper(YamlStream yamlStream = default)
        {
            _yamlStream = yamlStream ?? new YamlStream();
        }

        public void Save(TextWriter output, bool assignAnchors) => _yamlStream.Save(output, assignAnchors);

        public void Load(TextReader input) => _yamlStream.Load(input);

        public IList<YamlDocument> GetDocuments() => _yamlStream.Documents;
    }
}
