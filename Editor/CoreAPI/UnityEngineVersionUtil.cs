// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Utility for getting major/minor version information about the running Unity Engine.
    /// </summary>
    public static class UnityEngineVersionUtil
    {
        private const string UnknownUnityEngineVersion = "UNKNOWN";
        private const string LogicalProjectVersionPath = "ProjectSettings/ProjectVersion.txt";
        private static readonly Regex EngineVersionRegex = new(@"m_EditorVersion: (\S+)");

        private static string _engineVersion;

        /**
         * Returns the current Unity engine's version identifier.
         */
        public static string CurrentVersion
        {
            get {
                _engineVersion ??= TryGetProjectEngineVersion();
                return _engineVersion ?? UnknownUnityEngineVersion;
            }
        }

        private static string TryGetProjectEngineVersion()
        {
            var projectVersionFile = FindProjectVersionFile();

            if (!File.Exists(projectVersionFile))
            {
                Debug.LogWarning($"Could not retrieve engine version as version file was not found: {projectVersionFile}");
                return null;
            }

            try
            {
                using var reader = new StreamReader(projectVersionFile);
                var engineVersion = ReadEngineVersion(reader);
                if (engineVersion != null)
                {
                    return engineVersion;
                }
                Debug.LogWarning($"Engine version was not found in project version file: {projectVersionFile}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not retrieve engine version: {e.Message}");
                Debug.LogException(e);
            }

            return null;
        }

        private static string FindProjectVersionFile()
        {
            #if UNITY_EDITOR
            // Application.dataPath only provides access to the version file when in the editor.
            return Path.GetFullPath($"../{LogicalProjectVersionPath}", Application.dataPath);
            #else
            throw new NotImplementedException("ProjectVersion file is only accessible within the Editor.");
            #endif
        }

        private static string ReadEngineVersion(StreamReader reader)
        {
            var line = reader.ReadLine();
            while (line != null)
            {
                var match = EngineVersionRegex.Match(line);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                line = reader.ReadLine();
            }

            return null;
        }
    }
}