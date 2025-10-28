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

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using Aws.GameLift.Server.Model.Metrics;
using UnityEngine;

#if UNITY_SERVER || UNITY_EDITOR

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Unity component that manages the global GameLift metrics lifecycle.
    /// Initializes and manages a global Metrics instance that can be accessed
    /// thread-safely from anywhere via GameLiftMetrics.Manager.
    /// Also provides automatic GameLift event tracking and metadata collection.
    /// </summary>
    public class GameLiftMetricsProcessor : MonoBehaviour
    {
        [SerializeField]
        private GameLiftMetricsSettings _metricsSettings;
        private GameLiftLogger _logger;

        private MemoryStats _memoryStats;
        private TimeStats _timeStats;

        private IGauge _playersGauge;
        private IGauge _maxPlayersGauge;

        // Memory metrics timing
        private float _lastMemoryMetricsTime;

        // Track running coroutines
        private Coroutine _frameTimingCoroutine;
        private Coroutine _fixedUpdateTimingCoroutine;

        /// <summary>
        /// Check if metrics are currently enabled and running
        /// </summary>
        public bool IsMetricsEnabled => GameLiftMetrics.IsInitialized && _metricsSettings != null && _metricsSettings.EnableMetrics;

        /// <summary>
        /// Check if memory metrics are enabled
        /// </summary>
        public bool IsMemoryMetricsEnabled => IsMetricsEnabled && _metricsSettings.EnableMemoryMetrics;

        private void Awake()
        {
            _logger = GameLiftLogger.Instance;
            if (_metricsSettings == null || !_metricsSettings.EnableMetrics)
            {
                _logger?.LogWarning("GameLiftMetricsProcessor: Metrics are not enabled. ");
            }
            else
            {
                GameLiftMetrics.Initialize(_metricsSettings);
            }
        }

        private void Start()
        {
            if (!IsMetricsEnabled) return;

            // Initialize player count gauges
            _playersGauge = GameLiftMetrics.Manager.NewGauge("server_players").Build();
            _maxPlayersGauge = GameLiftMetrics.Manager.NewGauge("server_max_players").Build();

            // Initialize TimeStats module
            _timeStats = new TimeStats();
            _timeStats.Initialize(GameLiftMetrics.Manager);

            // Initialize MemoryStats module
            if (IsMemoryMetricsEnabled)
            {
                _memoryStats = new MemoryStats();
                _memoryStats.Initialize(GameLiftMetrics.Manager);
                _lastMemoryMetricsTime = Time.realtimeSinceStartup;
            }
        }

        private void FixedUpdate()
        {
            if (!IsMetricsEnabled) return;

            // Start timing the FixedUpdate cycle and wait for completion
            _timeStats?.StartFixedUpdateTiming();
            _fixedUpdateTimingCoroutine = StartCoroutine(MeasureFixedUpdateTimeCoroutine());

            // Collect memory metrics at specified interval
            if (IsMemoryMetricsEnabled && ShouldCollectMemoryMetrics())
            {
                _memoryStats?.CollectMemoryMetrics();
                _lastMemoryMetricsTime = Time.realtimeSinceStartup;
            }

            // Start frame timing measurement and wait for end of frame (only if not already running)
            // Fixed update can be run multiple times per frame, so we only start frame timing if not already running
            if (_frameTimingCoroutine == null)
            {
                _timeStats?.CollectFrameMetrics();
                _frameTimingCoroutine = StartCoroutine(MeasureFrameTimeCoroutine());
            }
        }

        private IEnumerator MeasureFrameTimeCoroutine()
        {
            if (!IsMetricsEnabled) yield break;

            yield return new WaitForEndOfFrame();

            try
            {
                _timeStats?.CompleteFrameMetrics();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to complete frame timing measurement: {ex.Message}");
            }
            finally
            {
                // Clean up the coroutine reference
                _frameTimingCoroutine = null;
            }
        }

        private IEnumerator MeasureFixedUpdateTimeCoroutine()
        {
            if (!IsMetricsEnabled) yield break;

            yield return new WaitForFixedUpdate();

            try
            {
                _timeStats?.EndFixedUpdateTiming();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to complete FixedUpdate timing measurement: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            // Stop any running coroutines
            if (_frameTimingCoroutine != null)
            {
                StopCoroutine(_frameTimingCoroutine);
                _frameTimingCoroutine = null;
            }
            if (_fixedUpdateTimingCoroutine != null)
            {
                StopCoroutine(_fixedUpdateTimingCoroutine);
                _fixedUpdateTimingCoroutine = null;
            }

            _memoryStats?.Shutdown();
            _timeStats?.Shutdown();
            GameLiftMetrics.Shutdown();
        }

        /// <summary>
        /// Call when a game session starts
        /// </summary>
        /// <param name="gameSession">The GameLift game session</param>
        public void OnGameSessionStarted(GameSession gameSession)
        {
            if (!IsMetricsEnabled) return;

            // Flush all metrics to ensure we capture the latest state before starting the session
            GameLiftMetrics.Manager.FlushAllMetrics();

            try
            {
                // Tag metrics with session id if provided
                if (!string.IsNullOrEmpty(gameSession?.GameSessionId))
                {
                    GameLiftMetrics.Manager.AddGlobalTag($"session_id:{gameSession.GameSessionId}");
                }

                // Set configured maximum players
                if (gameSession != null && _maxPlayersGauge != null)
                {
                    _maxPlayersGauge.Set(gameSession.MaximumPlayerSessionCount);
                }

                // Reset active players to 0 at session start
                _playersGauge?.Reset();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"OnGameSessionStarted error: {ex.Message}");
            }
        }

        /// <summary>
        /// Call when a player session is accepted
        /// </summary>
        public void OnPlayerSessionAccepted()
        {
            if (!IsMetricsEnabled) return;

            try
            {
                _playersGauge?.Increment();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"OnPlayerSessionAccepted error: {ex.Message}");
            }
        }

        /// <summary>
        /// Call when a player session is removed
        /// </summary>
        public void OnPlayerSessionRemoved()
        {
            if (!IsMetricsEnabled) return;

            try
            {
                _playersGauge?.Decrement();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"OnPlayerSessionRemoved error: {ex.Message}");
            }
        }

        /// <summary>
        /// Call when the game session ends
        /// </summary>
        public void OnGameSessionEnded()
        {
            if (!IsMetricsEnabled) return;

            try
            {
                // Set players to 0 and flush remaining metrics
                _playersGauge?.Reset();
                GameLiftMetrics.Manager.FlushAllMetrics();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"OnGameSessionEnded error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if it's time to collect memory metrics based on the configured interval
        /// </summary>
        private bool ShouldCollectMemoryMetrics()
        {
            return Time.realtimeSinceStartup - _lastMemoryMetricsTime >= _metricsSettings.MemoryMetricsIntervalSeconds;
        }
    }
}
#else

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Unity component that manages the global GameLift metrics lifecycle.
    /// Initializes and manages a global Metrics instance that can be accessed
    /// thread-safely from anywhere via GameLiftMetrics.Manager.
    /// Also provides automatic GameLift event tracking and metadata collection.
    /// </summary>
    public class GameLiftMetricsProcessor : MonoBehaviour
    {
        [SerializeField]
        private GameLiftMetricsSettings _metricsSettings;

        /// <summary>
        /// Check if metrics are currently enabled and running
        /// </summary>
        public bool IsMetricsEnabled => false;

        /// <summary>
        /// Check if memory metrics are enabled
        /// </summary>
        public bool IsMemoryMetricsEnabled => false;


        /// <summary>
        /// Call when a game session starts
        /// </summary>
        /// <param name="gameSession">The GameLift game session</param>
        public void OnGameSessionStarted(GameSession gameSession) { }

        /// <summary>
        /// Call when a player session is accepted
        /// </summary>
        public void OnPlayerSessionAccepted() { }

        /// <summary>
        /// Call when a player session is removed
        /// </summary>
        public void OnPlayerSessionRemoved() { }

        /// <summary>
        /// Call when the game session ends
        /// </summary>
        public void OnGameSessionEnded() { }
    }
}
#endif
