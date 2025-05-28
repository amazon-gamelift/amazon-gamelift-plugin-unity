// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER
using System;
using UnityEngine.SceneManagement;
#endif
using UnityEngine;

public sealed class ServerBootstrap : MonoBehaviour
{
#pragma warning disable CS0414
    [SerializeField]
    private GameLift _gameLift;

    [SerializeField]
    private string _gameSceneName = "GameScene";
#pragma warning restore CS0414

#if UNITY_SERVER
    private void Awake()
    {
        // prevent the game going to sleep when the window loses focus
        Application.runInBackground = true;
        // Just 60 frames per second is enough
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        string logFilePath = ReadLogFilePathFromCmd();
        string authToken = ReadAuthTokenFromCmd();
        int? port = ReadPortFromCmd();
        _gameLift.StartServer(port ?? 33430, authToken, logFilePath);
        StartGame();
    }

    private void StartGame()
    {
        SceneManager.LoadSceneAsync(_gameSceneName);
    }

    private int? ReadPortFromCmd()
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "-port")
            {
                continue;
            }

            if (!int.TryParse(args[i + 1], out int value))
            {
                continue;
            }

            if (value < 1000 || value >= 65536)
            {
                continue;
            }

            return value;
        }

        return null;
    }

    private string ReadAuthTokenFromCmd()
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "-authToken")
            {
                continue;
            }

            return args[i + 1];
        }

        return null;
    }

    private string ReadLogFilePathFromCmd()
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 2; i++)
        {
            if (args[i] != "-logFile")
            {
                continue;
            }

            return args[i + 1];
        }

        return null;
    }
#endif
}
