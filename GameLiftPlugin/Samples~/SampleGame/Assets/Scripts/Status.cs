// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

public class Status
{
    private readonly GameLogic _gl;
    private bool _gameliftStatus = false;
    private int _numConnected = 0;
    private int _playerIdx = 0;
    private bool _connected = false;

    public Status(GameLogic gl)
    {
        _gl = gl;
    }

    public void Start()
    {
        SetStatusText();
    }

    public bool GameliftStatus
    {
        set
        {
            if (_gameliftStatus == value)
            {
                return;
            }

            _gameliftStatus = value;
            SetStatusText();
        }

        get => _gameliftStatus;
    }

    public int NumConnected
    {
        set
        {
            if (_numConnected == value)
            {
                return;
            }

            _numConnected = value;
            SetStatusText();
        }
    }

    public int PlayerIdx
    {
        set
        {
            if (_playerIdx == value)
            {
                return;
            }

            _playerIdx = value;
            SetStatusText();
        }
    }

    public bool Connected
    {
        set
        {
            if (_connected == value)
            {
                return;
            }

            _connected = value;
            SetStatusText();
        }
    }

    public void SetStatusText()
    {
        string glt = _gameliftStatus ? "GAMELIFT | " : "LOCAL | ";
#if UNITY_SERVER
        string svr = "SERVER | ";
        string con = _numConnected + " CONNECTED";
#else
        string svr = "CLIENT | ";
        string con = _connected ? "CONNECTED " + (_playerIdx + 1) + "UP" : "DISCONNECTED";
#endif
        _gl.Render.SetStatusText(svr + glt + con);
    }
}
