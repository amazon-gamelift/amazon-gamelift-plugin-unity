/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

#if UNITY_SERVER || UNITY_EDITOR
// FakeProcReader
// ----------------
// Test double for IProcReader used in editor/unit tests. Allows supplying synthetic
// /proc style file contents (e.g. /proc/meminfo, /proc/self/status, /proc/stat, etc.)
// so that metric extraction logic can be validated against controlled inputs without
// depending on the host machine's actual runtime values. This enables deterministic
// tests of parsing logic and edge cases (missing files, malformed lines, specific
// numeric values) by injecting arbitrary file bodies and iterating line-by-line
// exactly as the production reader would.
using System.Collections.Generic;
using Aws.GameLift.Unity.Metrics;

namespace GameLiftServerSDK.Editor.UnitTests.Helpers
{
    internal class FakeProcReader : IProcReader
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> _lines = new Dictionary<string, List<string>>();

        public FakeProcReader AddFile(string path, string content)
        {
            _files[path] = content;
            _lines[path] = new List<string>(content.Replace("\r", "\n").Split('\n'));
            return this;
        }

        public bool Exists(string path) => _files.ContainsKey(path);
        public string ReadAllText(string path) => _files.TryGetValue(path, out var c) ? c : string.Empty;
        public IEnumerable<string> ReadLines(string path)
        {
            if (_lines.TryGetValue(path, out var list))
            {
                foreach (var l in list) yield return l;
            }
        }
    }
}
#endif
