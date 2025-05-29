// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace AmazonGameLift.Editor
{
    public class ScriptingDefineSymbolEditor
    {
        private readonly NamedBuildTarget _namedBuildTarget;

        public ScriptingDefineSymbolEditor(NamedBuildTarget namedBuildTarget)
        {
            _namedBuildTarget = namedBuildTarget;
        }

        public void Add(string value)
        {
            IEnumerable<string> defines =
                PlayerSettings.GetScriptingDefineSymbols(_namedBuildTarget).Split(';');
            if (defines.Contains(value))
            {
                return;
            }

            defines = defines.Append(value);
            PlayerSettings.SetScriptingDefineSymbols(_namedBuildTarget, string.Join(';', defines));
        }

        public void Remove(string value)
        {
            IEnumerable<string> defines =
                PlayerSettings.GetScriptingDefineSymbols(_namedBuildTarget).Split(';');
            if (!defines.Contains(value))
            {
                return;
            }

            defines = defines.Where(item => item != value);
            PlayerSettings.SetScriptingDefineSymbols(_namedBuildTarget, string.Join(';', defines));
        }
    }
}