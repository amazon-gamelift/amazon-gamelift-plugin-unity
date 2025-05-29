// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public interface ILogger
    {
        void Log(string message, LogType logType);
        void LogResponseError(Response response, LogType logType = LogType.Error);
        void LogException(Exception ex);
    }
}
