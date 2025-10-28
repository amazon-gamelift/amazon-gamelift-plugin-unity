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
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model.Metrics;

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Singleton used by Unity projects to record network-related metrics in a transport-agnostic way
    /// so users can instrument their own networking layer (NGO, Netcode for Entities, custom, etc.).
    ///
    /// Metrics exposed:
    /// - Counters: bytes_in, bytes_out, packets_in, packets_out, packets_in_lost, packets_out_lost
    /// - Gauges: server_connections
    ///
    /// Usage:
    ///   GameLiftMetrics.Network.IncrementBytesIn(deltaBytes);
    /// </summary>
    /// 
    public sealed class NetworkStats
    {
        private static readonly object s_lock = new object();
        private static NetworkStats s_instance;

        public static NetworkStats Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new NetworkStats();
                    }
                    return s_instance;
                }
            }
        }

        private ICounter _bytesIn;
        private ICounter _bytesOut;
        private ICounter _packetsIn;
        private ICounter _packetsOut;
        private ICounter _packetsInLost;
        private ICounter _packetsOutLost;
        private IGauge _serverConnections;

        private bool _initialized;
        private GameLiftLogger _logger;

        public NetworkStats() { }

        /// <summary>
        /// Initialize metric handles. Called automatically by GameLiftMetrics.Initialize.
        /// </summary>
        public void Initialize(Aws.GameLift.Server.Metrics manager)
        {
            if (_initialized || manager == null) return;

            _logger = GameLiftLogger.Instance;
            try
            {
                _bytesIn = manager.NewCounter("bytes_in").Build();
                _bytesOut = manager.NewCounter("bytes_out").Build();
                _packetsIn = manager.NewCounter("packets_in").Build();
                _packetsOut = manager.NewCounter("packets_out").Build();
                _packetsInLost = manager.NewCounter("packets_in_lost").Build();
                _packetsOutLost = manager.NewCounter("packets_out_lost").Build();
                _serverConnections = manager.NewGauge("connections").Build();

                _initialized = true;
                _logger?.LogInfo("NetworkStats initialized");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"NetworkStats initialization failed: {ex.Message}", ex);
            }
        }

        public bool IsInitialized => _initialized;

        // Counter helpers
        public void IncrementBytesIn(int delta) => AddSafe(_bytesIn, delta, nameof(IncrementBytesIn));
        public void IncrementBytesOut(int delta) => AddSafe(_bytesOut, delta, nameof(IncrementBytesOut));
        public void IncrementPacketsIn(int delta) => AddSafe(_packetsIn, delta, nameof(IncrementPacketsIn));
        public void IncrementPacketsOut(int delta) => AddSafe(_packetsOut, delta, nameof(IncrementPacketsOut));
        public void IncrementPacketsInLost(int delta) => AddSafe(_packetsInLost, delta, nameof(IncrementPacketsInLost));
        public void IncrementPacketsOutLost(int delta) => AddSafe(_packetsOutLost, delta, nameof(IncrementPacketsOutLost));

        // Gauge helpers
        public void SetServerConnections(int value) => SetSafe(_serverConnections, value, nameof(SetServerConnections));
        public void IncrementServerConnections() => IncSafe(_serverConnections, nameof(IncrementServerConnections));
        public void DecrementServerConnections() => DecSafe(_serverConnections, nameof(DecrementServerConnections));

        private void AddSafe(ICounter counter, int delta, string op)
        {
            if (!_initialized || counter == null) return;
            if (delta <= 0)
            {
                _logger?.LogWarning($"{op} called with non-positive delta: {delta}. Ignoring.");
                return;
            }
            try { counter.Add(delta); }
            catch (Exception ex) { _logger?.LogWarning($"{op} failed: {ex.Message}"); }
        }

        private void SetSafe(IGauge gauge, int value, string op)
        {
            if (!_initialized || gauge == null) return;
            try { gauge.Set(value); }
            catch (Exception ex) { _logger?.LogWarning($"{op} failed: {ex.Message}"); }
        }

        private void IncSafe(IGauge gauge, string op)
        {
            if (!_initialized || gauge == null) return;
            try { gauge.Increment(); }
            catch (Exception ex) { _logger?.LogWarning($"{op} failed: {ex.Message}"); }
        }

        private void DecSafe(IGauge gauge, string op)
        {
            if (!_initialized || gauge == null) return;
            try { gauge.Decrement(); }
            catch (Exception ex) { _logger?.LogWarning($"{op} failed: {ex.Message}"); }
        }

        public void Shutdown()
        {
            _initialized = false;
            _bytesIn = null;
            _bytesOut = null;
            _packetsIn = null;
            _packetsOut = null;
            _packetsInLost = null;
            _packetsOutLost = null;
            _serverConnections = null;
        }
    }
}
#endif
