// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AmazonGameLiftPlugin.Core.JavaCheck.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;

namespace AmazonGameLiftPlugin.Core.JavaCheck
{
    public class InstalledJavaVersionProvider : IInstalledJavaVersionProvider
    {
        private readonly IProcessWrapper _process;

        public InstalledJavaVersionProvider(IProcessWrapper process)
        {
            _process = process;
        }

        private CheckInstalledJavaVersionResponse CreateCheckInstalledJavaVersionResponse(bool installed) {
            return Response.Ok(new CheckInstalledJavaVersionResponse
                {
                    IsInstalled = installed
                });
        }

        public CheckInstalledJavaVersionResponse CheckInstalledJavaVersion(CheckInstalledJavaVersionRequest request)
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "java";
            processStartInfo.Arguments = " -version";
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;

            string processOutput = null;
            try
            {
                processOutput = _process.GetProcessOutput(processStartInfo);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return CreateCheckInstalledJavaVersionResponse(false);
            }

            if (processOutput == null) {
                return CreateCheckInstalledJavaVersionResponse(false);
            }

            // Expected output format:
            //  (java|openjdk) version "<majorVersion>.<minorVersion>.<build>"
            //  ex: java version "1.8.0_291"
            //      openjdk version "1.8.0_322"
            //      openjdk version "19"
            //  majorVersion and minorVersion are integers. build can be anything
            var outputPattern = new Regex("(?:openjdk|java) version \"(?<majorVersion>\\d+)(?:\\.(?<minorVersion>\\d+)(?:\\.[^\"]+))?\"");

            Match outputMatch = outputPattern.Match(processOutput);

            if (!outputMatch.Success) {
                return CreateCheckInstalledJavaVersionResponse(false);
            }

            //  if majorVersion is 1, we use minorVersion as the majorVersion, since java version had the format 1.?? until java 8
            var majorVersion = outputMatch.Groups["majorVersion"].ToString();
            var minorVersion = outputMatch.Groups["minorVersion"].ToString();
            var actualMajorVersion = majorVersion.Equals("1") && !String.IsNullOrEmpty(minorVersion) ? minorVersion : majorVersion;

            int.TryParse(actualMajorVersion, out int majorVersionAsNumber);
            bool isInstalled = majorVersionAsNumber >= request.ExpectedMinimumJavaMajorVersion;

            return CreateCheckInstalledJavaVersionResponse(isInstalled);
        }
    }
}
