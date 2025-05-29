// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;

namespace AmazonGameLift.Editor
{
    // See available operating system types: https://docs.unity3d.com/ScriptReference/SystemInfo-operatingSystem.html
    internal class OperatingSystemUtility
    {
        public static LocalOperatingSystem GetLocalOperatingSystem()
        {
            if (isMacOs())
            {
                return LocalOperatingSystem.MAC_OS;
            }

            if (isWindows())
            {
                return LocalOperatingSystem.WINDOWS;
            }

            return LocalOperatingSystem.UNSUPPORTED;
        }

        public static bool isMacOs()
        {
            return SystemInfo.operatingSystem.StartsWith("Mac OS");
        }

        public static bool isWindows()
        {
            return SystemInfo.operatingSystem.StartsWith("Windows");
        }
    }
}
