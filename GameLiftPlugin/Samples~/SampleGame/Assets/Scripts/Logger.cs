// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Logger
{
    private static Logger s_instance;
    private readonly int _processId;

    public static Logger SharedInstance => s_instance ?? (s_instance = new Logger());

    protected internal Logger()
    {
        _processId = Process.GetCurrentProcess().Id;
    }

    public virtual void Write(string message, LogType logType = LogType.Log)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        string formatted = Format(message, logType);
        switch (logType)
        {
            case LogType.Error:
                Debug.LogError(formatted);
                break;
            case LogType.Assert:
                Debug.LogAssertion(formatted);
                break;
            case LogType.Warning:
                Debug.LogWarning(formatted);
                break;
            case LogType.Log:
                Debug.Log(formatted);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logType));
        }
    }

    private string Format(string message, LogType logType)
    {
        DateTime utcNow = DateTime.UtcNow;
        return $"{utcNow:s}{utcNow:ff} PID:{_processId} {logType} {message}";
    }
}
