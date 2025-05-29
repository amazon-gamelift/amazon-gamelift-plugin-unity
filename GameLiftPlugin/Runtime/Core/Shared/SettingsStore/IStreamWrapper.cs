// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace AmazonGameLiftPlugin.Core.Shared.SettingsStore
{
    public interface IStreamWrapper
    {
        void Save(TextWriter output, bool assignAnchors);

        void Load(TextReader input);

        IList<YamlDocument> GetDocuments();
    }
}
