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
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Unity.Metrics;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Tests for the GameLiftMetrics class
    /// </summary>
    public class GameLiftMetricsTests
    {
        private GameLiftMetricsSettings testSettings;

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing metrics state
            GameLiftMetrics.Shutdown();
            
            // Create test settings
            testSettings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            testSettings.EnableMetrics = true;
            testSettings.StatsDHost = "localhost";
            testSettings.StatsDPort = 8125;
            testSettings.FlushIntervalMs = 5000;
        }

        [TearDown]
        public void TearDown()
        {
            GameLiftMetrics.Shutdown();
            
            if (testSettings != null)
            {
                UnityEngine.Object.DestroyImmediate(testSettings);
            }
        }

        [Test]
        public void Initialize_WithValidSettings_InitializesCorrectly()
        {
            // Act
            GameLiftMetrics.Initialize(testSettings);

            // Assert
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "GameLiftMetrics should be initialized");
            Assert.IsNotNull(GameLiftMetrics.Manager, "Manager should not be null");
        }

        [Test]
        public void Initialize_WithNullSettings_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => GameLiftMetrics.Initialize(null));
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "GameLiftMetrics should not be initialized");
        }

        [Test]
        public void Shutdown_WhenInitialized_ShutsDownCleanly()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "Precondition: should be initialized");

            // Act
            GameLiftMetrics.Shutdown();

            // Assert
            Assert.IsFalse(GameLiftMetrics.IsInitialized, "GameLiftMetrics should be shut down");
            Assert.IsNull(GameLiftMetrics.Manager, "Manager should be null after shutdown");
        }

        [Test]
        public void Manager_AccessedConcurrently_IsThreadSafe()
        {
            // Arrange
            GameLiftMetrics.Initialize(testSettings);
            bool[] results = new bool[10];
            System.Threading.Tasks.Task[] tasks = new System.Threading.Tasks.Task[10];

            // Act - Access Manager property concurrently
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var manager = GameLiftMetrics.Manager;
                        results[index] = manager != null;
                    }
                    catch
                    {
                        results[index] = false;
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert - All accesses should succeed
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(results[i], $"Concurrent access {i} should succeed");
            }
        }

        [Test]
        public void Initialize_CalledTwice_ReplacesExistingManager()
        {
            // Arrange - Initialize first time
            GameLiftMetrics.Initialize(testSettings);
            Assert.IsTrue(GameLiftMetrics.IsInitialized, "First initialization should work");
            var firstManager = GameLiftMetrics.Manager;
            Assert.IsNotNull(firstManager, "First manager should be created");

            // Create different settings
            var secondSettings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            secondSettings.EnableMetrics = true;
            secondSettings.StatsDHost = "0.0.0.0";
            secondSettings.StatsDPort = 9999;

            try
            {
                // Act - Initialize second time (should replace first)
                GameLiftMetrics.Initialize(secondSettings);
                
                // Assert
                Assert.IsTrue(GameLiftMetrics.IsInitialized, "GameLiftMetrics should still be initialized");
                var secondManager = GameLiftMetrics.Manager;
                Assert.IsNotNull(secondManager, "New manager should be created");
                Assert.AreNotSame(firstManager, secondManager, "Second manager should be different from first");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(secondSettings);
            }
        }
    }
}

#endif
