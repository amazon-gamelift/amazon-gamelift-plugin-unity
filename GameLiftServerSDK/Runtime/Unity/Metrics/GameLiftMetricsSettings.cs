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

using UnityEngine;

namespace Aws.GameLift.Unity.Metrics
{
    [CreateAssetMenu(fileName = "GameLiftMetricsSettings", menuName = "GameLift/Metrics Settings")]
    public sealed class GameLiftMetricsSettings : ScriptableObject
    {
        [Header("Metrics Configuration")]
        [Tooltip("Enable GameLift metrics collection and reporting")]
        public bool EnableMetrics = true;

        [Header("Memory Metrics")]
        [Tooltip("Enable memory metrics collection (physical, virtual, managed memory)")]
        public bool EnableMemoryMetrics = true;

        [Tooltip("How often to collect memory metrics in seconds")]
        [Range(0.1f, 60.0f)]
        public float MemoryMetricsIntervalSeconds = 1.0f;

        [Header("Crash Reporter Configuration")]
        [Tooltip("CrashReporter server hostname")]
        public string CrashReporterHost = "localhost";

        [Tooltip("Crash Reporter server port")]
        public int CrashReporterPort = 8126;
        
        [Header("StatsD Configuration")]
        [Tooltip("StatsD server hostname")]
        public string StatsDHost = "localhost";

        [Tooltip("StatsD server port")]
        public int StatsDPort = 8125;

        [Header("Flush Settings")]
        [Tooltip("How often to flush metrics to StatsD in milliseconds")]
        [Range(1000, 60000)]
        public int FlushIntervalMs = 5000;

        [Header("Global Tags")]
        [Tooltip("Custom global tags to apply to all metrics")]
        public string[] GlobalTags = new string[0];

        [Header("Identifiers")]
        [Tooltip("Custom Build ID override. If empty, will use GAMELIFT_BUILD_ID environment variable or Application.version as fallback")]
        public string BuildIDOverride = string.Empty;

        [Tooltip("Optional user-defined Server ID for this server instance (env GAMELIFT_SERVER_ID overrides)")]
        public string ServerID = string.Empty;

        [Tooltip("Optional Fleet ID (env GAMELIFT_SDK_FLEET_ID overrides)")]
        public string FleetID = "UnknownFleetID";

        [Tooltip("Optional GameLift Process ID (env GAMELIFT_SDK_PROCESS_ID overrides)")]
        public string ProcessID = "UnknownProcessID";

        [Header("Advanced")]
        [Tooltip("Enable debug logging for metrics operations")]
        public bool EnableDebugLogging = false;

        // Non-serialized custom StatsD client injection point (used mainly for tests or advanced users)
        [System.NonSerialized]
        public Aws.GameLift.Server.IStatsDClient CustomStatsDClient;

        // <summary>
        // Returns the custom StatsD client if provided (non-serialized) otherwise null.
        // </summary>
        public Aws.GameLift.Server.IStatsDClient GetCustomClient() => CustomStatsDClient;

        // <summary>
        // Get the StatsD host from environment variable or configured value
        // </summary>
        public string GetStatsDHost()
        {
            string envHost = System.Environment.GetEnvironmentVariable("GAMELIFT_STATSD_HOST");
            return !string.IsNullOrEmpty(envHost) ? envHost : StatsDHost;
        }

        // <summary>
        // Get the StatsD port from environment variable or configured value
        // </summary>
        public int GetStatsDPort()
        {
            string envPort = System.Environment.GetEnvironmentVariable("GAMELIFT_STATSD_PORT");
            if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int port))
            {
                return port;
            }
            return StatsDPort;
        }

        // <summary>
        // Get the Crash Reporter host from environment variable or configured value
        // </summary>
        public string GetCrashReporterHost()
        {
            string envHost = System.Environment.GetEnvironmentVariable("GAMELIFT_CRASH_REPORTER_HOST");
            return !string.IsNullOrEmpty(envHost) ? envHost : CrashReporterHost;
        }

        // <summary>
        // Get the Crash Reporter port from environment variable or configured value
        // </summary>
        public int GetCrashReporterPort()
        {
            string envPort = System.Environment.GetEnvironmentVariable("GAMELIFT_CRASH_REPORTER_PORT");
            if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int port))
            {
                return port;
            }
            return CrashReporterPort;
        }

        // <summary>
        // Get the Build ID from environment variable or configured value.
        // Defaulting to configured value in Edit>Project Settings>Player>Version
        // </summary>
        public string GetBuildID()
        {
            string envBuildId = System.Environment.GetEnvironmentVariable("GAMELIFT_BUILD_ID");
            if (!string.IsNullOrEmpty(envBuildId))
                return envBuildId;
            if (!string.IsNullOrEmpty(BuildIDOverride))
                return BuildIDOverride;
            return Application.version;
        }

        // <summary>
        // Get the Server ID from environment variable or configured value
        // </summary>
        public string GetServerID()
        {
            string envServerId = System.Environment.GetEnvironmentVariable("GAMELIFT_SERVER_ID");
            return !string.IsNullOrEmpty(envServerId) ? envServerId : ServerID;
        }

        // <summary>
        // Get Fleet ID from environment variable or serialized value
        // </summary>
        public string GetFleetID()
        {
            string env = System.Environment.GetEnvironmentVariable(GameLiftConstants.EnvironmentVariableFleetId);
            return !string.IsNullOrEmpty(env) ? env : FleetID;
        }

        // <summary>
        // Get Process ID from environment variable or serialized value
        // </summary>
        public string GetProcessID()
        {
            string env = System.Environment.GetEnvironmentVariable(GameLiftConstants.EnvironmentVariableProcessId);
            return !string.IsNullOrEmpty(env) ? env : ProcessID;
        }
    }
}
