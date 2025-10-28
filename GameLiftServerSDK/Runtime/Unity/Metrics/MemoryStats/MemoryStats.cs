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
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model.Metrics;
using Aws.GameLift.Unity.Metrics;
using System;

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Utility class for collecting Unity memory usage metrics.
    /// </summary>
    public class MemoryStats
    {
        // Physical Memory Metrics
        private IGauge _memoryPhysicalTotalGauge; // total physical memory in bytes
        private IGauge _memoryPhysicalAvailableGauge; // available physical memory in bytes
        private IGauge _memoryPhysicalUsedGauge; // resident set size (RSS) in bytes

        // Virtual / Commit Memory Metrics
        private IGauge _memoryVirtualTotalGauge; // total virtual memory (RAM + swap) in bytes
        private IGauge _memoryVirtualAvailableGauge; // available virtual memory (commitLimit - committedAS) in bytes
        private IGauge _memoryVirtualUsedGauge; // used virtual memory (VmSize) in bytes
        private IGauge _memoryCommitLimitGauge; // commit limit (total allocatable memory) in bytes
        private IGauge _memoryCommittedAsGauge; // committed AS (allocated memory) in bytes
        private IGauge _memoryCommitAvailableGauge; // available commit memory (MemAvailable) in bytes

        // Managed Memory
        private IGauge _gcAllocatedMemoryGauge; // managed memory allocated by the garbage collector in bytes

        private bool _isInitialized;
        private GameLiftLogger _logger;
        private Aws.GameLift.Server.Metrics _metricsManager;
        private readonly IMemoryStatsSource _memoryInfoSource;

        public MemoryStats() : this(PlatformMemoryInfoSource()) {}

        public MemoryStats(IMemoryStatsSource memoryInfoSource)
        {
            _memoryInfoSource = memoryInfoSource;
        }

        private static IMemoryStatsSource PlatformMemoryInfoSource()
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            return new LinuxMemoryStatsSource();
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return new WindowsMemoryStatsSource();
#else
            return new UnsupportedMemoryStatsSource();
#endif
        }


        /// <summary>
        /// Initialize memory metric gauges.
        /// </summary>
        /// <param name="metricsManager">Metrics manager instance.</param>
        public void Initialize(Aws.GameLift.Server.Metrics metricsManager)
        {
            if (_isInitialized || metricsManager == null)
                return;

            _logger = GameLiftLogger.Instance;

            if (_memoryInfoSource is UnsupportedMemoryStatsSource)
            {
                _logger?.LogError($"MemoryStats unsupported on this platform. Memory stats will not be logged.");
                return;
            }

            _metricsManager = metricsManager;

            try
            {
                _memoryPhysicalTotalGauge = metricsManager.NewGauge("mem_physical_total").Build();
                _memoryPhysicalAvailableGauge = metricsManager.NewGauge("mem_physical_available").Build();
                _memoryPhysicalUsedGauge = metricsManager.NewGauge("mem_physical_used").Build();

                _memoryVirtualTotalGauge = metricsManager.NewGauge("mem_virtual_total").Build();
                _memoryVirtualAvailableGauge = metricsManager.NewGauge("mem_virtual_available").Build();
                _memoryVirtualUsedGauge = metricsManager.NewGauge("mem_virtual_used").Build();

                _memoryCommitLimitGauge = metricsManager.NewGauge("mem_commit_limit").Build();
                _memoryCommittedAsGauge = metricsManager.NewGauge("mem_committed_as").Build();
                _memoryCommitAvailableGauge = metricsManager.NewGauge("mem_commit_available").Build();

                _gcAllocatedMemoryGauge = metricsManager.NewGauge("managed_gc_allocated_bytes").Build();

                _isInitialized = true;
                _logger?.LogInfo("MemoryStats initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to initialize MemoryStats: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Collect and publish current memory metrics.
        /// </summary>
        public void CollectMemoryMetrics()
        {
            if (!_isInitialized) return;

            try
            {
                var maybeMemoryInfo = _memoryInfoSource?.ReadMemoryInfo();
                if (!maybeMemoryInfo.HasValue)
                {
                    _logger?.LogWarning($"Failed to read memory info.");
                    return;
                }

                var memoryValues = maybeMemoryInfo.Value;

                _memoryPhysicalTotalGauge?.Set(memoryValues.PhysicalTotal);
                _memoryPhysicalAvailableGauge?.Set(memoryValues.PhysicalAvailable);
                _memoryPhysicalUsedGauge?.Set(memoryValues.PhysicalUsed);

                _memoryVirtualTotalGauge?.Set(memoryValues.VirtualTotal);
                _memoryVirtualAvailableGauge?.Set(memoryValues.VirtualAvailable);
                _memoryVirtualUsedGauge?.Set(memoryValues.VirtualUsed);

                _memoryCommitLimitGauge?.Set(memoryValues.CommitLimit);
                _memoryCommittedAsGauge?.Set(memoryValues.CommittedAS);
                _memoryCommitAvailableGauge?.Set(memoryValues.CommitAvailable);

                _gcAllocatedMemoryGauge?.Set(GC.GetTotalMemory(false));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to collect memory metrics: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset gauges and mark uninitialized.
        /// </summary>
        public void Shutdown()
        {
            _isInitialized = false;
            _memoryPhysicalTotalGauge = null;
            _memoryPhysicalAvailableGauge = null;
            _memoryPhysicalUsedGauge = null;
            _memoryVirtualTotalGauge = null;
            _memoryVirtualAvailableGauge = null;
            _memoryVirtualUsedGauge = null;
            _memoryCommitLimitGauge = null;
            _memoryCommittedAsGauge = null;
            _memoryCommitAvailableGauge = null;
            _gcAllocatedMemoryGauge = null;
            _logger?.LogInfo("MemoryStats shutdown completed");
        }

        public bool IsInitialized => _isInitialized;
    }
}

/// <summary>
/// A no-op memory info source used on unsupported platforms.
/// </summary>
public class UnsupportedMemoryStatsSource : IMemoryStatsSource
{
    public MemoryValues? ReadMemoryInfo()
    {
        return null;
    }
}

