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
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Unity.Metrics;
using GameLiftServerSDK.Editor.UnitTests.Helpers;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Comprehensive tests for the TimeStats class
    /// </summary>
    public class TimeStatsTests
    {
        private TimeStats timeStats;
        private GameLiftMetricsSettings testSettings;
        private GameObject testGameObject;
        private TestStatsDClient statsClient;

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing metrics state
            GameLiftMetrics.Shutdown();

            // Create test GameObject for Unity context
            testGameObject = new GameObject("TestTimeStats");

            // Create test settings
            testSettings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            testSettings.EnableMetrics = true;
            testSettings.StatsDHost = "localhost";
            testSettings.StatsDPort = 8125;
            testSettings.FlushIntervalMs = 5000;
            statsClient = new TestStatsDClient();
            testSettings.CustomStatsDClient = statsClient; // capture metrics

            timeStats = new TimeStats();
        }

        [TearDown]
        public void TearDown()
        {
            timeStats?.Shutdown();
            GameLiftMetrics.Shutdown();

            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }

            if (testSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(testSettings);
            }
        }


        [Test]
        public void Initialize_WithValidMetricsManager_SetsInitializedFlag()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);

            // Act
            timeStats.Initialize(GameLiftMetrics.Manager);

            // Assert
            Assert.IsTrue(timeStats.IsInitialized, "TimeStats should be initialized");
        }

        [Test]
        public void Initialize_WithNullMetricsManager_DoesNotInitialize()
        {
            // Act
            timeStats.Initialize(null);

            // Assert
            Assert.IsFalse(timeStats.IsInitialized, "TimeStats should not initialize with null manager");
        }

        [Test]
        public void Initialize_CalledTwice_OnlyInitializesOnce()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);

            // Act
            timeStats.Initialize(GameLiftMetrics.Manager);
            bool firstInitResult = timeStats.IsInitialized;

            timeStats.Initialize(GameLiftMetrics.Manager);
            bool secondInitResult = timeStats.IsInitialized;

            // Assert
            Assert.IsTrue(firstInitResult, "Should initialize on first call");
            Assert.IsTrue(secondInitResult, "Should remain initialized on second call");
        }

        [Test]
        public void Initialize_WhenGameLiftMetricsNotReady_HandlesGracefully()
        {
            // Act
            timeStats.Initialize(null);

            // Assert
            Assert.IsFalse(timeStats.IsInitialized, "Should not initialize without valid metrics manager");
        }

        [Test]
        public void Shutdown_WhenInitialized_ClearsInitializedFlag()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            timeStats.Initialize(GameLiftMetrics.Manager);
            Assert.IsTrue(timeStats.IsInitialized, "Precondition: should be initialized");

            // Act
            timeStats.Shutdown();

            // Assert
            Assert.IsFalse(timeStats.IsInitialized, "Should clear initialized flag");
        }

        [Test]
        public void Shutdown_CalledMultipleTimes_RemainsShutdown()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            timeStats.Initialize(GameLiftMetrics.Manager);

            // Act
            timeStats.Shutdown();
            timeStats.Shutdown();
            timeStats.Shutdown();

            // Assert
            Assert.IsFalse(timeStats.IsInitialized, "Should remain shutdown after multiple calls");
        }

        [Test]
        public void Shutdown_WithNullServerUpMetric_ClearsState()
        {
            // This test specifically addresses the null reference issue reported
            // Test shutdown when metrics may not be properly initialized

            // Act
            timeStats.Shutdown();

            // Assert
            Assert.IsFalse(timeStats.IsInitialized, "Should remain uninitialized");
        }

        private static void WaitUntil(Func<bool> condition, int timeoutMs = 1000, int pollIntervalMs = 10)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (condition()) return;
                System.Threading.Thread.Sleep(pollIntervalMs);
            }
        }

        [Test]
        public void FullTimingCycle_EmitsTickAndDeltaTime_MaintainsInitializedState()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            timeStats.Initialize(GameLiftMetrics.Manager);

            // Act - Simulate a full timing cycle
            timeStats.CollectFrameMetrics();
            // Simulate some work time
            System.Threading.Thread.Sleep(1);
            timeStats.CompleteFrameMetrics();
            GameLiftMetrics.Manager.FlushAllMetrics();

            // Wait for timing metrics for this frame (tick_time and delta_time)
            WaitUntil(() => statsClient.TimingCalls.Count >= 2);

            // Assert
            Assert.IsTrue(timeStats.IsInitialized, "Should remain initialized after timing cycle");

            var snapshot = statsClient.TimingCalls.ToArray(); // atomic copy to avoid race condition
            var names = snapshot.Select(t => t.metric).ToList();
            CollectionAssert.Contains(names, "tick_time");
            CollectionAssert.Contains(names, "delta_time");
        }

        [Test]
        public void FullFixedUpdateCycle_EmitsFixedUpdateTime_MaintainsInitializedState()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            timeStats.Initialize(GameLiftMetrics.Manager);

            // Act - Simulate a full fixed update cycle
            timeStats.StartFixedUpdateTiming();
            // Simulate some work time
            System.Threading.Thread.Sleep(1);
            timeStats.EndFixedUpdateTiming();
            GameLiftMetrics.Manager.FlushAllMetrics();

            // Wait for at least one timing metric
            WaitUntil(() => statsClient.TimingCalls.Count >= 1);

            // Assert
            Assert.IsTrue(timeStats.IsInitialized, "Should remain initialized after fixed update cycle");

            var snapshot = statsClient.TimingCalls.ToArray(); // atomic copy to avoid race condition
            var names = snapshot.Select(t => t.metric).ToList();
            CollectionAssert.Contains(names, "fixed_update_time");
        }

        [Test]
        public void ConcurrentTimingCalls_MaintainState()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            timeStats.Initialize(GameLiftMetrics.Manager);

            // Act - Multiple overlapping timing calls
            timeStats.CollectFrameMetrics();
            timeStats.StartFixedUpdateTiming();
            timeStats.CollectFrameMetrics(); // Second call before completion
            timeStats.EndFixedUpdateTiming();
            timeStats.CompleteFrameMetrics();
            GameLiftMetrics.Manager.FlushAllMetrics();
            // Expect all three timers to emit: fixed_update_time, tick_time, and delta_time
            WaitUntil(() => statsClient.TimingCalls.Count >= 3);

            // Assert
            Assert.IsTrue(timeStats.IsInitialized, "Should remain initialized after overlapping calls");

            var names = statsClient.TimingCalls.Select(t => t.metric).ToList();

            CollectionAssert.Contains(names, "fixed_update_time");
            CollectionAssert.Contains(names, "tick_time");
            CollectionAssert.Contains(names, "delta_time");
        }
    }
}
#endif
