// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using UnityEngine;

public class GameLog
{
    private readonly GameLogic _logic;
    private readonly Logger _logger;

    public GameLog(GameLogic logic, Logger logger)
    {
        _logic = logic ?? throw new ArgumentNullException(nameof(logic));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void WriteLine(string line, LogType logType = LogType.Log)
    {
#if UNITY_SERVER
        string me = "SERVER";
#else
        string me = "CLIENT " + (_logic.PlayerIdx);
#endif
        _logger.Write($"{me}: Frame: {_logic.Frame} {line}", logType);
    }
}
