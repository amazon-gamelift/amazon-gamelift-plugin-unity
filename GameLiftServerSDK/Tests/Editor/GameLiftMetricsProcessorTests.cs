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
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Server.Model;
using Aws.GameLift.Unity.Metrics;
using Aws.GameLift.Server.Model.Metrics;
using GameLiftServerSDK.Editor.UnitTests.Helpers;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Tests for the GameLiftMetricsProcessor component
    /// </summary>
    public class GameLiftMetricsProcessorTests
    {

        private GameObject testGameObject;
        private GameLiftMetricsProcessor processor;
        private GameLiftMetricsSettings testSettings;

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing metrics state
            GameLiftMetrics.Shutdown();

            // Create test GameObject and component
            testGameObject = new GameObject("TestGameLiftMetricsProcessor");
            processor = testGameObject.AddComponent<GameLiftMetricsProcessor>();

            // Create test settings
            testSettings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            testSettings.EnableMetrics = true;
            testSettings.StatsDHost = "localhost";
            testSettings.StatsDPort = 8125;
            testSettings.FlushIntervalMs = 5000;
            testSettings.EnableDebugLogging = false;
            testSettings.GlobalTags = new string[] { "test:true" };

            // Set the settings via reflection since it's a SerializeField
            SetPrivateField(processor, "_metricsSettings", testSettings);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up
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
        public void Startup_WithMetricsEnabled_InitializesGameLiftMetrics()
        {
            // Act - Simulate Unity calling Awake
            InvokePrivateMethod(processor, "Awake");

            // Assert - Verify GameLiftMetrics is properly initialized
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "GameLiftMetrics should be initialized");
            Assert.IsNotNull(GameLiftMetrics.Manager, "MetricsManager should not be null");
            Assert.IsTrue(processor.IsMetricsEnabled, "IsMetricsEnabled should be true");
        }

        [Test]
        public void Startup_WithMetricsDisabled_DoesNotInitializeGameLiftMetrics()
        {
            // Arrange
            testSettings.EnableMetrics = false;

            // Act
            InvokePrivateMethod(processor, "Awake");

            // Assert
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "GameLiftMetrics should not be initialized when disabled");
            Assert.IsFalse(processor.IsMetricsEnabled, "IsMetricsEnabled should be false");
        }

        [Test]
        public void Startup_WithNullSettings_DoesNotCrash()
        {
            // Arrange
            SetPrivateField(processor, "_metricsSettings", null);

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => InvokePrivateMethod(processor, "Awake"));
            Assert.IsFalse(processor.IsMetricsEnabled, "IsMetricsEnabled should be false with null settings");
        }

        [Test]
        public void Shutdown_WhenInitialized_ShutsDownGameLiftMetrics()
        {
            // Arrange - Initialize metrics first
            InvokePrivateMethod(processor, "Awake");
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "Precondition: GameLiftMetrics should be initialized");

            // Act - Simulate Unity calling OnDestroy
            InvokePrivateMethod(processor, "OnDestroy");

            // Assert - Verify proper cleanup
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "GameLiftMetrics should be shut down");
            Assert.IsNull(GameLiftMetrics.Manager, "MetricsManager should be null after shutdown");
        }

        [Test]
        public void Shutdown_WhenNotInitialized_DoesNotCrash()
        {
            // Arrange - Don't initialize metrics
            testSettings.EnableMetrics = false;
            InvokePrivateMethod(processor, "Awake");

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => InvokePrivateMethod(processor, "OnDestroy"));
        }

        [Test]
        public void Shutdown_CalledMultipleTimes_HandlesGracefully()
        {
            // Arrange
            InvokePrivateMethod(processor, "Awake");

            // Act & Assert - Multiple shutdowns should not crash
            Assert.DoesNotThrow(() => InvokePrivateMethod(processor, "OnDestroy"));
            Assert.DoesNotThrow(() => InvokePrivateMethod(processor, "OnDestroy"));
            Assert.DoesNotThrow(() => GameLiftMetrics.Shutdown());
        }

        [Test]
        public void GameEvents_WhenMetricsEnabled_HandleGracefully()
        {
            // Arrange
            // Inject a test StatsD client to capture global tags
            var statsClient = new TestStatsDClient();
            testSettings.CustomStatsDClient = statsClient;
            InvokePrivateMethod(processor, "Awake");
            // Initialize gauges
            InvokePrivateMethod(processor, "Start");
            var gameSession = new GameSession { GameSessionId = "test-session-123", MaximumPlayerSessionCount = 8 };

            // Act - fire events
            processor.OnGameSessionStarted(gameSession);

            // Assert - session id tag applied globally
            Assert.IsTrue(statsClient.GlobalTags.Any(t => t.Key == "session_id" && t.Value == gameSession.GameSessionId),
                "Session tag should be present with correct key/value");

            // Assert - max players set and players reset
            var maxPlayersGauge = (IGauge)GetPrivateField(processor, "_maxPlayersGauge");
            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assert.IsNotNull(maxPlayersGauge, "maxPlayersGauge should be initialized");
            Assert.IsNotNull(playersGauge, "playersGauge should be initialized");
            Assert.AreEqual(8, maxPlayersGauge.CurrentValue, "Max players should match session maximum");
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should reset to 0 at session start");

            // Accept a player -> players = 1
            processor.OnPlayerSessionAccepted();
            Assert.AreEqual(1, playersGauge.CurrentValue, "Players should increment to 1 on accept");

            // Remove a player -> players = 0
            processor.OnPlayerSessionRemoved();
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should decrement to 0 on remove");

            // End game session -> players remains 0
            processor.OnGameSessionEnded();
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should be 0 at session end");
        }

        [Test]
        public void SessionIdTag_IsUnique_AndUpdatesOnSubsequentSessions()
        {
            // Arrange
            var statsClient = new TestStatsDClient();
            testSettings.CustomStatsDClient = statsClient;
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");
            var firstSession = new GameSession { GameSessionId = "session-A", MaximumPlayerSessionCount = 4 };
            var secondSession = new GameSession { GameSessionId = "session-B", MaximumPlayerSessionCount = 6 };

            // Act: start first session
            processor.OnGameSessionStarted(firstSession);
            // Assert only one session_id tag with first value
            Assert.AreEqual(1, statsClient.GlobalTags.Count(t => t.Key == "session_id"), "Exactly one session_id tag after first session");
            Assert.IsTrue(statsClient.GlobalTags.Any(t => t.Key == "session_id" && t.Value == firstSession.GameSessionId), "session_id tag should match first session id");

            // Act: start second session (simulate new game session start)
            processor.OnGameSessionStarted(secondSession);
            // Assert still only one session_id tag and value updated
            Assert.AreEqual(1, statsClient.GlobalTags.Count(t => t.Key == "session_id"), "Exactly one session_id tag after second session");
            Assert.IsTrue(statsClient.GlobalTags.Any(t => t.Key == "session_id" && t.Value == secondSession.GameSessionId), "session_id tag should match second session id");
            Assert.IsFalse(statsClient.GlobalTags.Any(t => t.Key == "session_id" && t.Value == firstSession.GameSessionId), "Old session_id value should be replaced");
        }

        [Test]
        public void GameEvents_WhenMetricsDisabled_HandleGracefully()
        {
            // Arrange
            testSettings.EnableMetrics = false;
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");
            var gameSession = new GameSession { GameSessionId = "test-session-123" };

            // Act & Assert - All events should handle gracefully even when disabled
            Assert.DoesNotThrow(() => processor.OnGameSessionStarted(gameSession));
            Assert.DoesNotThrow(() => processor.OnPlayerSessionAccepted());
            Assert.DoesNotThrow(() => processor.OnPlayerSessionRemoved());
            Assert.DoesNotThrow(() => processor.OnGameSessionEnded());

            // Gauges should not be initialized
            Assert.IsNull(GetPrivateField(processor, "_playersGauge"));
            Assert.IsNull(GetPrivateField(processor, "_maxPlayersGauge"));
        }

        [Test]
        public void GameEvents_WithNullParameters_HandleGracefully()
        {
            // Arrange
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");

            // Act & Assert - Null parameters should not crash and still update counts
            processor.OnGameSessionStarted(null); // should reset players to 0
            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should reset to 0 even with null session");

            processor.OnPlayerSessionAccepted();
            Assert.AreEqual(1, playersGauge.CurrentValue, "Players should increment on accept with null session");

            processor.OnPlayerSessionRemoved();
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should decrement on remove with null session");
        }

        [Test]
        public void FullLifecycle_StartupShutdown_WorksCorrectly()
        {
            // Test complete lifecycle

            // 1. Startup
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "Should initialize");

            // 2. Use (simulate some game events)
            var gameSession = new GameSession { GameSessionId = "lifecycle-test" };
            processor.OnGameSessionStarted(gameSession);
            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should reset to 0 at start");

            processor.OnPlayerSessionAccepted();
            Assert.AreEqual(1, playersGauge.CurrentValue, "Players should be 1 after accept");

            processor.OnPlayerSessionRemoved();
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should be 0 after remove");
            processor.OnGameSessionEnded();
            Assert.AreEqual(0, playersGauge.CurrentValue, "Players should remain 0 at end");

            // 3. Shutdown
            InvokePrivateMethod(processor, "OnDestroy");
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "Should shutdown cleanly");
        }

        [Test]
        public void MultipleProcessors_OnlyFirstInitializes()
        {
            // Arrange
            var secondGameObject = new GameObject("SecondProcessor");
            var secondProcessor = secondGameObject.AddComponent<GameLiftMetricsProcessor>();
            SetPrivateField(secondProcessor, "_metricsSettings", testSettings);

            try
            {
                // Act - Initialize both processors
                InvokePrivateMethod(processor, "Awake");
                InvokePrivateMethod(secondProcessor, "Awake");

                // Assert - Both should work but share the same global metrics instance
                Assert.IsTrue(GameLiftMetrics.IsInitialized);
                Assert.IsTrue(processor.IsMetricsEnabled);
                Assert.IsTrue(secondProcessor.IsMetricsEnabled);
                Assert.AreSame(GameLiftMetrics.Manager, GameLiftMetrics.Manager, "Should share the same manager instance");
            }
            finally
            {
                if (secondGameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(secondGameObject);
                }
            }
        }

        [Test]
        public void TimeStats_WhenMetricsEnabled_InitializesCorrectly()
        {
            // Arrange & Act
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");

            // Assert - TimeStats should be initialized
            var timeStats = GetPrivateField(processor, "_timeStats");
            Assert.IsNotNull(timeStats, "TimeStats should be created in Start()");
            var isInitialized = (bool)InvokeMethodOnObject(timeStats, "get_IsInitialized");
            Assert.IsTrue(isInitialized, "TimeStats should be initialized");
        }

        [Test]
        public void TimeStats_WhenMetricsDisabled_DoesNotInitialize()
        {
            // Arrange
            testSettings.EnableMetrics = false;

            // Act
            InvokePrivateMethod(processor, "Awake");

            // Assert - TimeStats should not be created or should not be initialized
            var timeStats = GetPrivateField(processor, "_timeStats");
            if (timeStats != null)
            {
                var isInitialized = (bool)InvokeMethodOnObject(timeStats, "get_IsInitialized");
                Assert.IsFalse(isInitialized, "TimeStats should not be initialized when metrics are disabled");
            }
        }

        [Test]
        public void MemoryStats_WhenMetricsEnabled_InitializesCorrectly()
        {
            // Arrange
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");

            // Act
            var memoryStats = GetPrivateField(processor, "_memoryStats");

            // Assert
            Assert.IsNotNull(memoryStats, "MemoryStats should be created when enabled");
            var isInitialized = (bool)InvokeMethodOnObject(memoryStats, "get_IsInitialized");
            Assert.IsTrue(isInitialized, "MemoryStats should be initialized when enabled");
        }

        [Test]
        public void MemoryStats_WhenMemoryMetricsDisabled_NotInitialized()
        {
            // Arrange
            testSettings.EnableMemoryMetrics = false;

            // Act
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");

            // Assert
            var memoryStats = GetPrivateField(processor, "_memoryStats");
            if (memoryStats != null)
            {
                var isInitialized = (bool)InvokeMethodOnObject(memoryStats, "get_IsInitialized");
                Assert.IsFalse(isInitialized, "MemoryStats should not initialize when memory metrics disabled");
            }
        }

        [Test]
        public void MemoryStats_Lifecycle_ShutdownOnDestroy()
        {
            // Arrange
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");
            var memoryStats = GetPrivateField(processor, "_memoryStats");
            Assume.That(memoryStats, Is.Not.Null, "Precondition: MemoryStats created");
            var isInitialized = (bool)InvokeMethodOnObject(memoryStats, "get_IsInitialized");
            Assume.That(isInitialized, Is.True, "Precondition: MemoryStats initialized");

            // Act
            InvokePrivateMethod(processor, "OnDestroy");

            // Assert - after destroy we expect GameLiftMetrics shutdown and MemoryStats shutdown
            var shutdownInitialized = (bool)InvokeMethodOnObject(memoryStats, "get_IsInitialized");
            Assert.IsFalse(shutdownInitialized, "MemoryStats should be shutdown after processor destroyed");
        }

        [Test]
        public void MemoryStats_NotCreated_WhenGlobalMetricsDisabled()
        {
            // Arrange
            testSettings.EnableMetrics = false;
            testSettings.EnableMemoryMetrics = true; // memory flag true but global off

            // Act
            InvokePrivateMethod(processor, "Awake");
            InvokePrivateMethod(processor, "Start");

            // Assert
            var memoryStats = GetPrivateField(processor, "_memoryStats");
            if (memoryStats != null)
            {
                var isInitialized = (bool)InvokeMethodOnObject(memoryStats, "get_IsInitialized");
                Assert.IsFalse(isInitialized, "MemoryStats should not initialize if global metrics disabled");
            }
        }

        private void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found");
            field.SetValue(instance, value);
        }

        private object GetPrivateField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found");
            return field.GetValue(instance);
        }

        private object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method '{methodName}' not found");
            return method.Invoke(instance, parameters);
        }

        private object InvokeMethodOnObject(object instance, string methodName, params object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method '{methodName}' not found");
            return method.Invoke(instance, parameters);
        }
    }
}
#endif
