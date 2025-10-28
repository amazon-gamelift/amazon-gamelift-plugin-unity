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
using System.Linq;
using System.Threading;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Unity.Metrics;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model.Metrics;
using Aws.GameLift;
using GameLiftServerSDK.Editor.UnitTests.Helpers;

namespace GameLiftServerSDK.Editor.UnitTests
{
    public class NetworkStatsTests
    {
        private GameLiftMetricsSettings _settings;
        private TestStatsDClient _statsClient;

        private static void WaitUntil(Func<bool> condition, int timeoutMs = 1000, int pollIntervalMs = 10)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (condition()) return;
                Thread.Sleep(pollIntervalMs);
            }
        }

        [SetUp]
        public void SetUp()
        {
            GameLiftMetrics.Shutdown();
            _settings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            _settings.EnableMetrics = true;
            _settings.StatsDHost = "localhost";
            _settings.StatsDPort = 8125;
            _statsClient = new TestStatsDClient();
            _settings.CustomStatsDClient = _statsClient; // inject test statsd client
            GameLiftMetrics.Initialize(_settings);

            // Ensure no leftover metrics from previous runs
            var mgr = GameLiftMetrics.Manager;
            mgr.DeleteMetric("bytes_in");
            mgr.DeleteMetric("bytes_out");
            mgr.DeleteMetric("packets_in");
            mgr.DeleteMetric("packets_out");
            mgr.DeleteMetric("packets_in_lost");
            mgr.DeleteMetric("packets_out_lost");
            mgr.DeleteMetric("connections");
        }

        [TearDown]
        public void TearDown()
        {
            GameLiftMetrics.Shutdown();
            if (_settings != null) UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Initialize_SetsUpCounters()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);
            Assert.IsTrue(ns.IsInitialized);
        }

        [Test]
        public void Increment_AddsToCounters()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);

            ns.IncrementBytesIn(100);
            ns.IncrementBytesOut(200);
            ns.IncrementPacketsIn(3);
            ns.IncrementPacketsOut(4);
            ns.IncrementPacketsInLost(1);
            ns.IncrementPacketsOutLost(2);

            GameLiftMetrics.Manager.FlushAllMetrics();

            // Wait for async flush to propagate to the test client
            WaitUntil(() => _statsClient.CounterCalls.Count >= 6);

            var metrics = _statsClient.CounterCalls.Select(c => (c.metric, c.value)).ToList();
            Assert.Contains(("bytes_in", 100), metrics);
            Assert.Contains(("bytes_out", 200), metrics);
            Assert.Contains(("packets_in", 3), metrics);
            Assert.Contains(("packets_out", 4), metrics);
            Assert.Contains(("packets_in_lost", 1), metrics);
            Assert.Contains(("packets_out_lost", 2), metrics);
        }

        [Test]
        public void Shutdown_ClearsStateAndStopsEmitting()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);
            ns.Shutdown();

            ns.IncrementBytesIn(50);
            GameLiftMetrics.Manager.FlushAllMetrics();

            Thread.Sleep(100);
            Assert.IsFalse(_statsClient.CounterCalls.Any(c => c.metric == "bytes_in"));
            Assert.IsFalse(ns.IsInitialized);
        }

        [Test]
        public void MultipleIncrements_AccumulateOverFlush()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);

            ns.IncrementBytesIn(10);
            ns.IncrementBytesIn(15);
            ns.IncrementPacketsOut(2);
            ns.IncrementPacketsOut(3);

            GameLiftMetrics.Manager.FlushAllMetrics();

            int Sum(string name) => _statsClient.CounterCalls.Where(c => c.metric == name).Sum(c => c.value);
            // Wait for async flush to complete and accumulate values
            WaitUntil(() => Sum("bytes_in") == 25 && Sum("packets_out") == 5);
            Assert.AreEqual(25, Sum("bytes_in"));
            Assert.AreEqual(5, Sum("packets_out"));
        }

        [Test]
        public void NonPositiveValues_DoNotEmitMetrics()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);

            // Zero and negative deltas should be ignored
            ns.IncrementBytesIn(0);
            ns.IncrementBytesOut(-5);
            ns.IncrementPacketsIn(0);
            ns.IncrementPacketsOut(-1);
            ns.IncrementPacketsInLost(0);
            ns.IncrementPacketsOutLost(-2);

            GameLiftMetrics.Manager.FlushAllMetrics();

            // Allow async flush to complete
            Thread.Sleep(100);

            Assert.IsFalse(_statsClient.CounterCalls.Any(), "No counter metrics should be emitted for non-positive deltas");
        }

        [Test]
        public void ThrowingCounter_Add_IsCaught_And_NoMetricsEmitted()
        {
            var ns = new NetworkStats();
            ns.Initialize(GameLiftMetrics.Manager);

            // Replace the bytes_in counter with a throwing stub via reflection
            var field = typeof(NetworkStats).GetField("_bytesIn", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Expected to find _bytesIn field via reflection");
            field.SetValue(ns, new ThrowingCounter("bytes_in"));

            // This call should not throw and should not emit metrics
            ns.IncrementBytesIn(10);

            GameLiftMetrics.Manager.FlushAllMetrics();
            Thread.Sleep(100);

            Assert.IsFalse(_statsClient.CounterCalls.Any(c => c.metric == "bytes_in"), "No bytes_in metrics should be emitted when Add throws");
        }

        // Minimal ICounter stub that throws from Add
        private sealed class ThrowingCounter : ICounter
        {
            public string Name { get; }
            public IDictionary<string, Tag> Tags { get; } = new Dictionary<string, Tag>();
            public SampleRate SampleRate => null; // not used
            public double CurrentValue => 0;

            public ThrowingCounter(string name)
            {
                Name = name;
            }

            public GenericOutcome Increment() => new GenericOutcome();
            public GenericOutcome Add(int value) => throw new InvalidOperationException("Test Add failure");
            public GenericOutcome AddTag(Tag tag) => new GenericOutcome();
            public GenericOutcome AddTag(string tagValue) => new GenericOutcome();
            public GenericOutcome RemoveTag(Tag tag) => new GenericOutcome();
            public GenericOutcome RemoveTag(string tagValue) => new GenericOutcome();
            public GenericOutcome Flush() => new GenericOutcome();
        }
    }
}
#endif
