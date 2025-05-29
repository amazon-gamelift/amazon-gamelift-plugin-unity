// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Editor.Scripts
{
    public class AnywhereFleetSettingsBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var directory = Path.GetDirectoryName(report.summary.outputPath);
#if UNITY_SERVER
            AnywhereFleetSettingsWriter.WriteServerSettings(directory);
#else
            AnywhereFleetSettingsWriter.WriteClientSettings(directory);
#endif
        }
    }
}
