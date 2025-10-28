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
using NUnit.Framework;
using UnityEngine;
using Aws.GameLift.Unity.Metrics;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Tests for env-var override and serialized fallback behavior in GameLiftMetricsSettings
    /// </summary>
    public class GameLiftMetricsSettingsTests
    {
        private GameLiftMetricsSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<GameLiftMetricsSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            if (settings != null)
            {
                UnityEngine.Object.DestroyImmediate(settings);
            }
        }

        // envVar, serializedFieldName, getterName, serializedValueToSet, expectedFallbackWhenEnvUnset
    [TestCase("GAMELIFT_BUILD_ID", "BuildIDOverride", "GetBuildID", "serialized-value", "serialized-value")]
    [TestCase("GAMELIFT_SERVER_ID", "ServerID", "GetServerID", "serialized-value", "serialized-value")]
    [TestCase("GAMELIFT_SDK_FLEET_ID", "FleetID", "GetFleetID", "serialized-value", "serialized-value")]
    [TestCase("GAMELIFT_SDK_PROCESS_ID", "ProcessID", "GetProcessID", "serialized-value", "serialized-value")]
        public void Getters_EnvOverridesSerialized_WhenSet(string envVar, string fieldName, string getterName, string serializedValue, string expectedFallback)
        {
            // Ensure clean env
            Environment.SetEnvironmentVariable(envVar, null);

            // 1) Fallback when env not set
            if (!string.IsNullOrEmpty(fieldName) && serializedValue != null)
            {
                SetSerializedField(settings, fieldName, serializedValue);
            }
            string value1 = InvokeGetter(settings, getterName);
            Assert.AreEqual(expectedFallback, value1, $"{getterName} should return expected fallback when env not set");

            // 2) Env override
            Environment.SetEnvironmentVariable(envVar, "env-value");
            try
            {
                string value2 = InvokeGetter(settings, getterName);
                Assert.AreEqual("env-value", value2, $"{getterName} should return env value when env set");
            }
            finally
            {
                // Cleanup env var
                Environment.SetEnvironmentVariable(envVar, null);
            }
        }

        private static void SetSerializedField(GameLiftMetricsSettings instance, string fieldName, string value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return; // no-op for env-only getters
            }
            var field = typeof(GameLiftMetricsSettings).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on settings");
            field.SetValue(instance, value);
        }

        private static string InvokeGetter(GameLiftMetricsSettings instance, string methodName)
        {
            var method = typeof(GameLiftMetricsSettings).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Getter '{methodName}' not found on settings");
            return (string)method.Invoke(instance, null);
        }
    }
}
#endif
