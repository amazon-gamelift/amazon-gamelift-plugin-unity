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
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Aws.GameLift.Server.Model;
using Aws.GameLift.Server.Model.Metrics;
using Aws.GameLift.Unity.Metrics;

namespace GameLiftServerSDK.Runtime.UnitTests
{
    /// <summary>
    /// Runtime smoke tests for GameLiftMetricsProcessor - focused on critical path validation
    /// </summary>
    public class GameLiftMetricsProcessorRuntimeTests
    {
        private GameObject testGameObject;
        private GameLiftMetricsProcessor processor;
        private GameLiftMetricsSettings currentSettings;

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
                testGameObject = null;
            }

            if (currentSettings != null)
            {
                Object.DestroyImmediate(currentSettings);
                currentSettings = null;
            }

            // Shutdown metrics last, after processor OnDestroy runs its own cleanup
            GameLiftMetrics.Shutdown();

            processor = null;
        }

        [UnityTest]
        public IEnumerator SmokeTest_MetricsProcessor_InitializesAndCleansUpProperly()
        {
            // Arrange - Create processor with minimal valid settings
            SetupProcessorWithEnabledMetrics();

            // Assert - Basic initialization worked
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "Metrics should initialize in runtime");
            Assert.IsTrue(processor.IsMetricsEnabled, "Processor should report enabled");

            // Act - Destroy GameObject (triggers cleanup)
            Object.DestroyImmediate(testGameObject);
            testGameObject = null;
            yield return null;

            // Assert - Cleanup worked
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "Metrics should shutdown when processor destroyed");
        }

        [Test]
        public void Test_NullSettings_ProcessorHandlesGracefully()
        {
            // Arrange
            SetupProcessorWithNullSettings();

            // Assert - Processor should handle null settings gracefully
            Assert.IsFalse(processor.IsMetricsEnabled, "Processor should report disabled with null settings");
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "Metrics should not initialize with null settings");
        }

        [UnityTest]
        public IEnumerator Test_ShutdownWithoutInitialization_NoNullReference()
        {
            // Arrange - Create processor but don't initialize metrics (simulate the error condition)
            SetupProcessorWithNullSettings();

            // Assert - Processor is not enabled (no metrics initialization)
            Assert.IsFalse(processor.IsMetricsEnabled, "Processor should not be enabled");
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "Metrics should not be initialized");

            // Act - Destroy GameObject to trigger OnDestroy and shutdown sequence
            // This should not throw a null reference exception
            Assert.DoesNotThrow(() => {
                Object.DestroyImmediate(testGameObject);
                testGameObject = null;
            }, "Shutdown should handle null metrics gracefully");

            yield return null;

            // Assert - Should still be safely shutdown
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "Metrics should remain uninitialized after cleanup");
        }

        [UnityTest]
        public IEnumerator Events_OnGameSessionStarted_SetsMaxPlayers_And_ResetsPlayers()
        {
            // Arrange
            SetupProcessorWithEnabledMetrics();
            yield return null; // allow Start() to run and gauges to be created

            var session = new GameSession
            {
                GameSessionId = "session-abc",
                MaximumPlayerSessionCount = 8
            };

            // Act
            processor.OnGameSessionStarted(session);

            // Assert
            var maxGauge = (IGauge)GetPrivateField(processor, "_maxPlayersGauge");
            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");

            Assert.IsNotNull(maxGauge, "_maxPlayersGauge should be initialized");
            Assert.IsNotNull(playersGauge, "_playersGauge should be initialized");
            Assert.AreEqual(8d, maxGauge.CurrentValue, "Max players gauge should match session maximum");
            Assert.AreEqual(0d, playersGauge.CurrentValue, "Players gauge should reset to 0 at session start");
        }

        [UnityTest]
        public IEnumerator Events_OnPlayerSessionAccepted_IncrementsPlayers()
        {
            // Arrange
            SetupProcessorWithEnabledMetrics();
            yield return null; // allow Start()
            processor.OnGameSessionStarted(new GameSession { GameSessionId = "s1", MaximumPlayerSessionCount = 4 });

            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assume.That(playersGauge, Is.Not.Null);

            // Act
            processor.OnPlayerSessionAccepted();
            processor.OnPlayerSessionAccepted();
            yield return null;

            // Assert
            Assert.AreEqual(2d, playersGauge.CurrentValue, "Players gauge should increment for accepted sessions");
        }

        [UnityTest]
        public IEnumerator Events_OnPlayerSessionRemoved_DecrementsPlayers()
        {
            // Arrange
            SetupProcessorWithEnabledMetrics();
            yield return null;
            processor.OnGameSessionStarted(new GameSession { GameSessionId = "s2", MaximumPlayerSessionCount = 4 });

            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assume.That(playersGauge, Is.Not.Null);

            // Bring to 1 then remove to 0
            processor.OnPlayerSessionAccepted();
            yield return null;

            // Act
            processor.OnPlayerSessionRemoved();
            yield return null;

            // Assert
            Assert.AreEqual(0d, playersGauge.CurrentValue, "Players gauge should decrement on removal");
        }

        [UnityTest]
        public IEnumerator Events_OnGameSessionEnded_SetsPlayersToZero()
        {
            // Arrange
            SetupProcessorWithEnabledMetrics();
            yield return null;
            processor.OnGameSessionStarted(new GameSession { GameSessionId = "s3", MaximumPlayerSessionCount = 10 });
            var playersGauge = (IGauge)GetPrivateField(processor, "_playersGauge");
            Assume.That(playersGauge, Is.Not.Null);

            processor.OnPlayerSessionAccepted();
            processor.OnPlayerSessionAccepted();
            Assume.That(playersGauge.CurrentValue, Is.EqualTo(2d));

            // Act
            processor.OnGameSessionEnded();
            yield return null;

            // Assert
            Assert.AreEqual(0d, playersGauge.CurrentValue, "Players gauge should be set to 0 on session end");
        }

        // Helper methods
        private GameLiftMetricsSettings CreateValidSettings(bool enableMetrics = true)
        {
            var settings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            settings.EnableMetrics = enableMetrics;
            settings.StatsDHost = "localhost";
            settings.StatsDPort = 8125;
            return settings;
        }

        private object GetPrivateField(object instance, string fieldName)
        {
            var fi = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return fi?.GetValue(instance);
        }

        private void SetupProcessorWithSettings(GameLiftMetricsSettings settings)
        {
            currentSettings = settings;
            testGameObject = new GameObject("TestProcessor");
            testGameObject.SetActive(false);

            processor = testGameObject.AddComponent<GameLiftMetricsProcessor>();

            var field = typeof(GameLiftMetricsProcessor).GetField("_metricsSettings",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(processor, settings);

            testGameObject.SetActive(true);
        }

        private void SetupProcessorWithEnabledMetrics()
        {
            var settings = CreateValidSettings(enableMetrics: true);
            SetupProcessorWithSettings(settings);
        }

        private void SetupProcessorWithDisabledMetrics()
        {
            var settings = CreateValidSettings(enableMetrics: false);
            SetupProcessorWithSettings(settings);
        }

        private void SetupProcessorWithNullSettings()
        {
            testGameObject = new GameObject("NullSettingsTest");
            testGameObject.SetActive(false);

            processor = testGameObject.AddComponent<GameLiftMetricsProcessor>();
            // Don't set any settings - leave them null

            testGameObject.SetActive(true);
        }
    }
}

#endif
