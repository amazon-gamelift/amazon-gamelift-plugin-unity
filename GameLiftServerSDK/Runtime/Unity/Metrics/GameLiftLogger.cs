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

using System;
using log4net;
using log4net.Config;
using UnityEngine;

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// A logger that utilizes the DefaultLoggingConfiguration and provides both Unity Debug and log4net logging.
    /// Safe to use from MonoBehaviours and other Unity contexts.
    /// </summary>
    public class GameLiftLogger
    {
        private const string LogPrefix = "GameLiftMetrics";
        private static GameLiftLogger s_instance;
        private static readonly ILog s_log = LogManager.GetLogger(typeof(GameLiftLogger));
        public enum LogLevel { Error = 0, Warning = 1, Info = 2, Debug = 3 }
        private static LogLevel s_currentLevel = LogLevel.Error; // default quiet

        public static void SetLogLevel(LogLevel level)
        {
            s_currentLevel = level;
        }

        public static LogLevel GetLogLevel() => s_currentLevel;

        public static GameLiftLogger Instance => s_instance ??= new GameLiftLogger();

        private GameLiftLogger()
        {
            EnsureLog4NetConfigured();
        }

        private static void EnsureLog4NetConfigured()
        {
            if (!LogManager.GetRepository().Configured)
            {
                // Configure log4net to support the default console output
                // This mirrors the configuration in DefaultLoggingConfiguration
                BasicConfigurator.Configure();
            }
        }

        /// <summary>
        /// Log an informational message.
        /// </summary>
        public void LogInfo(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            if (s_currentLevel >= LogLevel.Info)
            {
                Debug.Log($"[{LogPrefix}] {message}");
                s_log.Info(message);
            }
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        public void LogWarning(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            if (s_currentLevel >= LogLevel.Warning)
            {
                Debug.LogWarning($"[{LogPrefix}] {message}");
                s_log.Warn(message);
            }
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        public void LogError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Debug.LogError($"[{LogPrefix}] {message}");
            s_log.Error(message);
        }

        /// <summary>
        /// Log an error message with exception.
        /// </summary>
        public void LogError(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string fullMessage = exception != null ? $"{message} - Exception: {exception}" : message;
            Debug.LogError($"[{LogPrefix}] {fullMessage}");

            if (exception != null)
            {
                s_log.Error(message, exception);
            }
            else
            {
                s_log.Error(message);
            }
        }

        /// <summary>
        /// Log a debug message (only in Unity Editor or when UNITY_DEBUG is defined).
        /// </summary>
        public void LogDebug(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            if (s_currentLevel >= LogLevel.Debug)
            {
                Debug.Log($"[{LogPrefix} Debug] {message}");
                s_log.Debug(message);
            }
        }

        /// <summary>
        /// Log a fatal error message.
        /// </summary>
        public void LogFatal(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Debug.LogError($"[{LogPrefix} FATAL] {message}");
            s_log.Fatal(message);
        }

        /// <summary>
        /// Log a fatal error message with exception.
        /// </summary>
        public void LogFatal(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string fullMessage = exception != null ? $"{message} - Exception: {exception}" : message;
            Debug.LogError($"[{LogPrefix} FATAL] {fullMessage}");

            if (exception != null)
            {
                s_log.Fatal(message, exception);
            }
            else
            {
                s_log.Fatal(message);
            }
        }
    }
}
