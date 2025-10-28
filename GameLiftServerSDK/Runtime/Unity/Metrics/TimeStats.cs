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
using System;
using System.Collections.Generic;
using UnityEngine;
using Aws.GameLift.Server.Model.Metrics;
using Aws.GameLift.Server.Model.Metrics.DerivedMetrics;
using Aws.GameLift.Server;

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Utility class for collecting Unity performance timing metrics.
    /// </summary>
    public class TimeStats
    {
        private static readonly HashSet<IDerivedMetric> DefaultPercentiles = new HashSet<IDerivedMetric>
        {
            new Percentile(50),
            new Percentile(90),
            new Percentile(95)
        };

        // Measures actual time spent processing each server update cycle (ms)
        private ITimer _serverTickTimeMetric;
        // Measures time elapsed between server ticks/updates using Unity's Time.deltaTime (ms)
        private ITimer _serverDeltaTimeMetric;
        private ITimer _serverFixedUpdateTimeMetric;

        private float _fixedUpdateStartTime;
        private float _updateStartTime;
        private bool _isInitialized;
        private GameLiftLogger _logger;

        /// <summary>
        /// Initialize the TimeStats metrics.
        /// </summary>
        /// <param name="metricsManager">The GameLift metrics manager instance</param>
        public void Initialize(Aws.GameLift.Server.Metrics metricsManager)
        {
            if (_isInitialized || metricsManager == null)
                return;

            _logger = GameLiftLogger.Instance;

            try
            {
                // Server tick processing time, calculated from FixedUpdate start to end of frame (ms)
                _serverTickTimeMetric = metricsManager
                    .NewTimer("tick_time")
                    .SetDerivedMetrics(DefaultPercentiles)
                    .Build();

                // Delta time between ticks/updates using Unity's Time.deltaTime (ms)
                _serverDeltaTimeMetric = metricsManager
                    .NewTimer("delta_time")
                    .SetDerivedMetrics(DefaultPercentiles)
                    .Build();

                // Server fixed update time, calculated from FixedUpdate start to Awaitable.FixedUpdateAsync
                _serverFixedUpdateTimeMetric = metricsManager.NewTimer("fixed_update_time")
                    .SetDerivedMetrics(DefaultPercentiles)
                    .Build();

                _isInitialized = true;
                _logger?.LogInfo("TimeStats initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to initialize TimeStats: {ex.Message}");
            }
        }

        /// <summary>
        /// Collect frame timing metrics. Call this from FixedUpdate() to ensure we capture the entire frame time.
        /// </summary>
        public void CollectFrameMetrics()
        {
            if (!_isInitialized) return;
            // Start timing the frame processing
            _updateStartTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Complete frame timing measurement. Call this from LateUpdate().
        /// </summary>
        public void CompleteFrameMetrics()
        {
            if (!_isInitialized) return;

            try
            {
                float endTime = Time.realtimeSinceStartup;
                // Actual processing time for the server update (tick) in ms
                double frameProcessingTimeMs = (endTime - _updateStartTime) * 1000.0;
                _serverTickTimeMetric?.Set(frameProcessingTimeMs);

                // Unity's deltaTime in ms
                _serverDeltaTimeMetric?.Set(Time.deltaTime * 1000.0);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to record frame processing time: {ex.Message}");
            }
        }

        /// <summary>
        /// Start timing a FixedUpdate cycle. Call at the beginning of FixedUpdate().
        /// </summary>
        public void StartFixedUpdateTiming()
        {
            if (!_isInitialized) return;

            _fixedUpdateStartTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// End timing a FixedUpdate cycle and record the metric. Call at the end of FixedUpdate().
        /// </summary>
        public void EndFixedUpdateTiming()
        {
            if (!_isInitialized) return;

            try
            {
                float endTime = Time.realtimeSinceStartup;
                double fixedUpdateTimeMs = (endTime - _fixedUpdateStartTime) * 1000.0;
                _serverFixedUpdateTimeMetric?.Set(fixedUpdateTimeMs);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to record FixedUpdate timing: {ex.Message}");
            }
        }

        /// <summary>
        /// Shutdown and cleanup the TimeStats.
        /// </summary>
        public void Shutdown()
        {
            if (_isInitialized)
            {
                try
                {
                    GameLiftMetrics.Manager.FlushAllMetrics();
                    _logger?.LogInfo("TimeStats: server_up set to 0 for shutdown");
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Failed to set server_up to 0 during shutdown: {ex.Message}");
                }
            }

            _isInitialized = false;
            _serverTickTimeMetric = null;
            _serverDeltaTimeMetric = null;
            _serverFixedUpdateTimeMetric = null;
        }

        /// <summary>
        /// Check if TimeStats is properly initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
    }
}
#endif
