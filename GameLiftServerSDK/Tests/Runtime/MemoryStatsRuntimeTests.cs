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


/*
* Runtime focused smoke tests for MemoryStats collection cycles.
*/
#if (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && UNITY_SERVER
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Aws.GameLift.Unity.Metrics;

namespace GameLiftServerSDK.Runtime.UnitTests
{
    public class MemoryStatsRuntimeTests
    {
        private GameObject _go;
        private GameLiftMetricsProcessor _processor;
        private GameLiftMetricsSettings _settings;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_settings != null) Object.DestroyImmediate(_settings);
        }

        [UnityTest]
        public IEnumerator MemoryMetrics_Collected_OnInterval()
        {
            // Create inactive so Awake sees the injected settings
            _go = new GameObject("MemoryProcessorRuntime");
            _go.SetActive(false);
            _processor = _go.AddComponent<GameLiftMetricsProcessor>();
            _settings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
            _settings.EnableMetrics = true;
            _settings.EnableMemoryMetrics = true;
            _settings.MemoryMetricsIntervalSeconds = 0.1f; // fast
            _settings.StatsDHost = "localhost";
            _settings.StatsDPort = 8125;
            var field = typeof(GameLiftMetricsProcessor).GetField("_metricsSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_processor, _settings);
            // Activate to trigger Awake/Start lifecycle with proper settings
            _go.SetActive(true);

            // Allow Awake + Start
            yield return null;
            Assert.IsTrue(_processor.IsMetricsEnabled, "Processor should be enabled after activation with settings");

            // Run several FixedUpdate cycles
            float end = Time.realtimeSinceStartup + 0.35f;
            var fixedUpdate = typeof(GameLiftMetricsProcessor).GetMethod("FixedUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            while (Time.realtimeSinceStartup < end)
            {
                fixedUpdate.Invoke(_processor, null);
                yield return null;
            }

            // Assert processor still enabled & memory stats exist
            Assert.IsTrue(_processor.IsMetricsEnabled, "Processor should remain enabled");
            var memoryStats = typeof(GameLiftMetricsProcessor).GetField("_memoryStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_processor);
            Assert.IsNotNull(memoryStats, "MemoryStats should exist");
            var isInitialized = (bool)memoryStats.GetType().GetMethod("get_IsInitialized").Invoke(memoryStats, null);
            Assert.IsTrue(isInitialized, "MemoryStats should be initialized");
        }
    }
}
#endif
