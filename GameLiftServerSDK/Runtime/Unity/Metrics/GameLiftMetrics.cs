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
using UnityEngine;
using Aws.GameLift;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model.Metrics;
using System;


namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Global static accessor for GameLift metrics functionality.
    /// Provides Unity-friendly access to the underlying Metrics while maintaining
    /// Unity best practices for component lifecycle management.
    /// </summary>
    public static class GameLiftMetrics
    {
        private static Aws.GameLift.Server.Metrics _manager;
        private static GameLiftMetricsSettings _metricsSettings;
        private static GameLiftLogger _logger;
        private static readonly object _lock = new object();
        private static NetworkStats _networkStats;

        /// <summary>
        /// Gets the global Metrics instance.
        /// This is thread-safe and can be accessed from any thread.
        /// </summary>
        /// <returns>The Metrics instance, or null if not initialized</returns>
        public static Aws.GameLift.Server.Metrics Manager
        {
            get
            {
                lock (_lock)
                {
                    return _manager;
                }
            }
        }

        /// <summary>
        /// Gets the global NetworkStats singleton for recording network-related metrics.
        /// Returns null until Initialize() has completed.
        /// </summary>
        public static NetworkStats Network
        {
            get
            {
                lock (_lock)
                {
                    return _networkStats;
                }
            }
        }

        /// <summary>
        /// Checks if the metrics system is currently initialized and ready to use.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                lock (_lock)
                {
                    return _manager != null;
                }
            }
        }

        /// <summary>
        /// Public method to initialize the global metrics manager using <paramref name="settings"/>.
        /// This should be called once at the start of the application.
        /// </summary>
        /// <param name="settings">The GameLift metrics settings to configure the manager</param>
        public static void Initialize(GameLiftMetricsSettings settings)
        {
            _logger = GameLiftLogger.Instance;

            lock (_lock)
            {
                if (_manager != null)
                {
                    _logger?.LogWarning("GameLiftMetrics: Attempting to initialize when already initialized. Shutting down previous instance.");
                    _manager.Dispose();
                }

                _metricsSettings = settings;
            }

            // Configure logger level based on debug flag
            if (settings != null && settings.EnableDebugLogging)
            {
                GameLiftLogger.SetLogLevel(GameLiftLogger.LogLevel.Debug);
            }
            else
            {
                GameLiftLogger.SetLogLevel(GameLiftLogger.LogLevel.Error); // only errors/fatal
            }

            MetricsBuilder managerBuilder = Aws.GameLift.Server.Metrics.Create();
            managerBuilder
                .SetCrashReporterHost(_metricsSettings.GetCrashReporterHost())
                .SetCrashReporterPort(_metricsSettings.GetCrashReporterPort())
                .SetStatsdHost(_metricsSettings.GetStatsDHost())
                .SetStatsdPort(_metricsSettings.GetStatsDPort())
                .SetFlushInterval(_metricsSettings.FlushIntervalMs);

            var custom = _metricsSettings.GetCustomClient();
            if (custom != null)
            {
                managerBuilder.SetStatsDClient(custom);
            }
            _manager = managerBuilder.Build();

            SetGlobalTags();

            // Initialize built-in network stats helper
            try
            {
                var net = NetworkStats.Instance;
                net.Initialize(_manager);
                lock (_lock)
                {
                    _networkStats = net;
                }
            }
            catch (Exception ex)
            {
                if (_metricsSettings.EnableDebugLogging)
                {
                    _logger?.LogError($"GameLiftMetrics: Failed to initialize NetworkStats: {ex.Message}", ex);
                }
            }

            _logger?.LogInfo("GameLiftMetrics: Initialized with settings: " +
                $"Crash Reporter Host={_metricsSettings.GetCrashReporterHost()}:" +
                $"{_metricsSettings.GetCrashReporterPort()} " +
                $"Statsd Host={_metricsSettings.GetStatsDHost()}:" +
                $"{_metricsSettings.GetStatsDPort()} " +
                $"FlushInterval={_metricsSettings.FlushIntervalMs}ms");
        }

        private static void SetGlobalTags()
        {
            // Add global tags
            if (_metricsSettings.GlobalTags != null)
            {
                _manager.AddGlobalTags(_metricsSettings.GlobalTags);
            }

            // Add Unity-specific global tags
            _manager.AddGlobalTag("platform:unity");
            try
            {
                // Build configuration
                if (Debug.isDebugBuild)
                {
                    _manager.AddGlobalTag("build_configuration:Debug");
                }
                else
                {
                    _manager.AddGlobalTag("build_configuration:Release");
                }

                // Add GameLift identifiers from settings
                string fleetId = _metricsSettings.GetFleetID();
                if (!string.IsNullOrEmpty(fleetId))
                {
                    _manager.AddGlobalTag($"fleet_id:{fleetId}");
                }

                string gameLiftProcessId = _metricsSettings.GetProcessID();
                if (!string.IsNullOrEmpty(gameLiftProcessId))
                {
                    _manager.AddGlobalTag($"gamelift_process_id:{gameLiftProcessId}");
                }

                // User-configurable IDs
                var buildId = _metricsSettings.GetBuildID();
                if (!string.IsNullOrEmpty(buildId))
                {
                    _manager.AddGlobalTag($"build_id:{buildId}");
                }

                var serverId = _metricsSettings.GetServerID();
                if (!string.IsNullOrEmpty(serverId))
                {
                    _manager.AddGlobalTag($"server_id:{serverId}");
                }

                string engineVersion = Application.unityVersion;
                if (!string.IsNullOrEmpty(engineVersion))
                {
                    _manager.AddGlobalTag($"unity_version:{engineVersion}");
                }

                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                _manager.AddGlobalTag($"process_pid:{pid}");
            }
            catch (Exception ex)
            {
                // Don't throw exceptions to avoid impacting gameplay
                if (_metricsSettings.EnableDebugLogging == true)
                {
                    _logger?.LogError($"GameLiftMetrics: Error adding metadata tags: {ex.Message}", ex);
                }
            }

        }

        /// <summary>
        /// Shuts down the global metrics manager and releases resources.
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                _networkStats?.Shutdown();
                _networkStats = null;
                _manager?.Dispose();
                _manager = null;
            }
        }
    }
}
#endif
