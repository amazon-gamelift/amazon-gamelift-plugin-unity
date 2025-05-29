// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER
using System;
using System.Net;
using System.Net.Sockets;

public class NetworkServer
{
    private readonly GameLogic _gl;
    private readonly TcpListener _listener;
    private readonly TcpClient[] _clients = { null, null, null, null };
    private readonly bool[] _ready = { false, false, false, false };
    private readonly string[] _playerSessionIds = { null, null, null, null };

    public NetworkServer(GameLogic gl, int port)
    {
        _gl = gl;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
    }

    public void Update()
    {
        // Are there any new connections pending?
        if (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();

            for (int x = 0; x < 4; x++)
            {
                if (_clients[x] == null)
                {
                    _clients[x] = client;
                    UpdateNumConnected();
                    _gl.Log.WriteLine("Connection accepted: playerIdx " + x + " joined");
                    return;
                }
            }

            // game already full, reject the connection
            _gl.Log.WriteLine("Connection rejected: game already full.");

            try
            {
                NetworkProtocol.Send(client, "REJECTED: game already full");
            }
            catch (SocketException) { }
        }

        // Have we received an input event message from any client?
        for (int x = 0; x < 4; x++)
        {
            if (_clients[x] == null)
            {
                continue;
            }

            string[] messages = NetworkProtocol.Receive(_clients[x]);

            foreach (string msgStr in messages)
            {
                _gl.Log.WriteLine("Msg rcvd from playerIdx " + x + " Msg: " + msgStr);
                HandleMessage(x, msgStr);
            }
        }
    }

    public void Disconnect()
    {
        // warn clients
        TransmitMessage("DISCONNECT:");

        // disconnect connections
        for (int x = 0; x < 4; x++)
        {
            HandleDisconnect(x);
        }

        // end listener
        _listener.Stop();

        // warn GameLift
        if (_gl.GameLift != null && _gl.GameLift.IsConnected)
        {
            _gl.GameLift.TerminateGameSession();
        }
        // process is terminating so no other state cleanup required
    }

    public void TransmitLog(string msgStr)
    {
        TransmitMessage("LOG:" + msgStr);
    }

    public void TransmitState()
    {
        // update the state of all players
        for (int x = 0; x < 4; x++)
        {
            string msgStr = "STATE:" + _gl.GetState(x);
            Send(msgStr, x, nameof(TransmitState));
        }
    }

    private void TransmitMessage(string msgStr)
    {
        // send the same message to all players
        for (int x = 0; x < 4; x++)
        {
            Send(msgStr, x, nameof(TransmitMessage));
        }
    }

    private void Send(string message, int playerIndex, string operationName)
    {
        try
        {
            NetworkProtocol.Send(_clients[playerIndex], message);
        }
        catch (Exception e) when (e is SocketException || e is InvalidOperationException)
        {
            HandleDisconnect(playerIndex);
            _gl.Log.WriteLine($"{operationName} failed: Disconnected. " + e);
        }
    }

    private void HandleMessage(int playerIdx, string msgStr)
    {
        // parse message and pass json string to relevant handler for deserialization
        _gl.Log.WriteLine("Msg rcvd from player " + playerIdx + ":" + msgStr);
        string delimiter = ":";
        string json = msgStr.Substring(msgStr.IndexOf(delimiter) + delimiter.Length);

        if (msgStr[0] == 'C')
        {
            HandleConnect(playerIdx, json);
        }

        if (msgStr[0] == 'R')
        {
            HandleReady(playerIdx);
        }

        if (msgStr[0] == 'I')
        {
            HandleInput(playerIdx, json);
        }

        if (msgStr[0] == 'E')
        {
            HandleEnd();
        }

        if (msgStr[0] == 'D')
        {
            HandleDisconnect(playerIdx);
        }
    }

    private void HandleConnect(int playerIdx, string json)
    {
        _gl.Log.WriteLine("CONNECT: player index " + playerIdx);
        _gl.ZeroScore(playerIdx);
        var connectionInfo = ConnectionInfo.CreateFromSerial(json);
        if (!ValidateConnectionRequest(connectionInfo))
        {
            HandleConnectionValidationFailure(playerIdx);
        } else
        {
            _playerSessionIds[playerIdx] = connectionInfo.PlayerSessionId;
            TransmitState();
        }
    }

    private void HandleReady(int playerIdx)
    {
        // start the game once all connected clients have requested to start (RETURN key)
        _gl.Log.WriteLine("READY:");
        _ready[playerIdx] = true;

        for (int x = 0; x < 4; x++)
        {
            if (_clients[x] != null && _ready[x] == false)
            {
                return; // a client is not ready
            }
        }

        _gl.StartGame();
    }

    private void HandleInput(int playerIdx, string json)
    {
        // simulate the input then respond to all players with the current state
        _gl.Log.WriteLine("INPUT:" + json);
        var inputChord = Chord.CreateFromSerial(json);
        _gl.InputEvent(playerIdx, inputChord);
    }

    private void HandleEnd()
    {
        // end the game at the request of any client (ESC key)
        _gl.Log.WriteLine("END:");

        for (int x = 0; x < 4; x++)
        {
            _ready[x] = false; // all clients end now.
        }

        _gl.EndGame();

        if (_gl.GameLift != null && _gl.GameLift.IsConnected)
        {
            _gl.GameLift.TerminateGameSession();
        }
    }

    private void HandleDisconnect(int playerIdx)
    {
        DisconnectPlayer(playerIdx);
        // if that was the last client to leave, then end the game.

        for (int x = 0; x < 4; x++)
        {
            if (_clients[x] != null)
            {
                return; // a client is still attached
            }
        }

        HandleEnd();
    }

    private void DisconnectPlayer(int playerIdx)
    {
        _gl.Log.WriteLine("DISCONNECT: Player " + playerIdx);

        // Tell GameLift the player has disconnected
        if (_playerSessionIds[playerIdx] != null)
        {
            _gl.GameLift.RemovePlayerSession(_playerSessionIds[playerIdx]);
            _playerSessionIds[playerIdx] = null;
        }

        // remove the client and close the connection
        TcpClient client = _clients[playerIdx];

        if (client != null)
        {
            NetworkStream stream = client.GetStream();
            stream.Close();
            client.Close();
            _clients[playerIdx] = null;
        }

        // clean up the game state
        _gl.ResetScore(playerIdx);
        UpdateNumConnected();
    }

    public bool IsConnected(int playerIdx)
    {
        return _clients[playerIdx] != null;
    }

    private void UpdateNumConnected()
    {
        int count = 0;

        for (int x = 0; x < 4; x++)
        {
            if (_clients[x] != null)
            {
                count++;
            }
        }

        _gl.Status.NumConnected = count;
    }

    private bool ValidateConnectionRequest(ConnectionInfo connectionInfo)
    {
        _gl.Log.WriteLine("Received Connection Request: " + connectionInfo);
        // Consider the Connection Validated if the GameLift AcceptPlayerSession call suceeds
        return _gl.GameLift.AcceptPlayerSession(connectionInfo.PlayerSessionId);
    }

    private void HandleConnectionValidationFailure(int playerIdx)
    {
        // Tell Client to Disconnect
        Send("DISCONNECT", playerIdx, nameof(HandleConnectionValidationFailure));

        _gl.Log.WriteLine("Connection Validation Failed for player " + playerIdx
               + ". Verify the provided PlayerSessionId is correct.");
        HandleDisconnect(playerIdx);
    }
}

#endif
