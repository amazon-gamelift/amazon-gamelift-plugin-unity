// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal sealed class UnityLogger : ILogger
    {
        private readonly TextProvider _textProvider;

        public UnityLogger(TextProvider textProvider) => _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));

        public void Log(string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType));
            }
        }

        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }

        public void LogResponseError(Response response, LogType logType)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Log($"{_textProvider.GetError(response.ErrorCode)} {response.ErrorMessage}", logType);
        }
    }
}
