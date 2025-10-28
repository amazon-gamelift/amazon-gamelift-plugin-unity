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

#if UNITY_SERVER
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Unity.Metrics;
using Aws.GameLift.Server;
using GameLiftServerSDK.Editor.UnitTests.Helpers;
using System.Linq;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Tests for MemoryStats class using mock IMemoryStatsSource interface
    /// </summary>
    public class MemoryStatsTests
    {
        private GameLiftMetricsSettings _settings;
        private TestStatsDClient _statsClient;

        [SetUp]
        public void SetUp()
        {
            GameLiftMetrics.Shutdown();
            _settings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            _settings.EnableMetrics = true;
            _settings.EnableMemoryMetrics = true;
            _settings.StatsDHost = "localhost";
            _settings.StatsDPort = 8125;
            _statsClient = new TestStatsDClient();
            _settings.CustomStatsDClient = _statsClient; // inject test statsd client for asserting metric calls
            GameLiftMetrics.Initialize(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            GameLiftMetrics.Shutdown();
            if (_settings != null) Object.DestroyImmediate(_settings);
        }

        // Helper to wait for gauge emission in tests
        private void WaitForGaugeEmission(int expectedMin = 1, int maxSpins = 100, int sleepMs = 10)
        {
            int spins = 0;
            while (_statsClient.GaugeCalls.Count < expectedMin && spins < maxSpins)
            {
                System.Threading.Thread.Sleep(sleepMs);
                spins++;
            }
        }

        [Test]
        public void Initialize_WithValidManager_SetsInitialized()
        {
            var mockMemoryInfoSource = new MockMemoryInfoSource();
            var ms = new MemoryStats(mockMemoryInfoSource);
            ms.Initialize(GameLiftMetrics.Manager);
            Assert.IsTrue(ms.IsInitialized);
        }

        [Test]
        public void CollectMemoryMetrics_PublishesExpectedGauges()
        {
            var mockMemoryInfoSource = new MockMemoryInfoSource
            {
                PhysicalTotal = 2000000 * 1024,
                PhysicalAvailable = 1000000 * 1024,
                PhysicalUsed = 500000 * 1024,
                VirtualTotal = 2500000L * 1024,
                VirtualAvailable = 750000 * 1024,
                VirtualUsed = 300000 * 1024,
                CommitLimit = 3000000L * 1024,
                CommittedAS = 250000 * 1024,
                CommitAvailable = 750000 * 1024
            };

            var ms = new MemoryStats(mockMemoryInfoSource);
            ms.Initialize(GameLiftMetrics.Manager);
            ms.CollectMemoryMetrics();
            GameLiftMetrics.Manager.FlushAllMetrics();
            WaitForGaugeEmission();

            var latest = _statsClient.GaugeCalls.Select(g => g.metric).ToList();
            Assert.Contains("mem_physical_total", latest);
            Assert.Contains("mem_physical_available", latest);
            Assert.Contains("mem_physical_used", latest);
            Assert.Contains("mem_virtual_total", latest);
            Assert.Contains("mem_virtual_available", latest);
            Assert.Contains("mem_virtual_used", latest);
            Assert.Contains("mem_commit_limit", latest);
            Assert.Contains("mem_committed_as", latest);
            Assert.Contains("mem_commit_available", latest);
            Assert.Contains("managed_gc_allocated_bytes", latest);

            // Numeric assertions - verify exact values are passed through
            double Get(string name) => _statsClient.GaugeCalls.Last(c => c.metric == name).value;
            const double KB = 1024.0;

            Assert.AreEqual(2000000 * KB, Get("mem_physical_total"), 0.1, "physical total bytes mismatch");
            Assert.AreEqual(1000000 * KB, Get("mem_physical_available"), 0.1, "physical available bytes mismatch");
            Assert.AreEqual(500000 * KB, Get("mem_physical_used"), 0.1, "physical used bytes mismatch");
            Assert.AreEqual(2500000 * KB, Get("mem_virtual_total"), 0.1, "virtual total bytes mismatch");
            Assert.AreEqual(750000 * KB, Get("mem_virtual_available"), 0.1, "virtual available bytes mismatch");
            Assert.AreEqual(300000 * KB, Get("mem_virtual_used"), 0.1, "virtual used bytes mismatch");
            Assert.AreEqual(3000000 * KB, Get("mem_commit_limit"), 0.1, "commit limit bytes mismatch");
            Assert.AreEqual(250000 * KB, Get("mem_committed_as"), 0.1, "committed AS bytes mismatch");
            Assert.AreEqual(750000 * KB, Get("mem_commit_available"), 0.1, "commit available bytes mismatch");
            Assert.GreaterOrEqual(Get("managed_gc_allocated_bytes"), 0.0, "managed GC allocated should be non-negative");
        }
    }

    public class MockMemoryInfoSource : IMemoryStatsSource
    {
        public long PhysicalTotal { get; set; }
        public long PhysicalAvailable { get; set; }
        public long PhysicalUsed { get; set; }
        public long VirtualTotal { get; set; }
        public long VirtualAvailable { get; set; }
        public long VirtualUsed { get; set; }
        public long CommitLimit { get; set; }
        public long CommittedAS { get; set; }
        public long CommitAvailable { get; set; }

        public MemoryValues? ReadMemoryInfo()
        {
            return new MemoryValues
            {
                PhysicalTotal = PhysicalTotal,
                PhysicalAvailable = PhysicalAvailable,
                PhysicalUsed = PhysicalUsed,
                VirtualTotal = VirtualTotal,
                VirtualAvailable = VirtualAvailable,
                VirtualUsed = VirtualUsed,
                CommitLimit = CommitLimit,
                CommittedAS = CommittedAS,
                CommitAvailable = CommitAvailable
            };
        }
    }
}
#endif // UNITY_SERVER
