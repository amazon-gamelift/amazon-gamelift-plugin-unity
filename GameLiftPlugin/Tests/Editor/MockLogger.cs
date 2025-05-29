// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using AmazonGameLiftPlugin.Core.Shared;
using ILogger = AmazonGameLift.Editor.ILogger;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    internal sealed class MockLogger : ILogger
    {
        
        public void Log(string message, LogType logType)
        {    
        }

        public void LogResponseError(Response response, LogType logType = LogType.Error)
        {
            
        }

        public void LogException(Exception ex)
        {
        }
    }
}