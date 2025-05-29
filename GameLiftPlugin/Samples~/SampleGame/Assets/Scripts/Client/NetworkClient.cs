// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class NetworkClient
{
    public const string LocalHost = "localhost";
    private readonly GameLogic _gl;
    private TcpClient _client = null;

    public bool Authoritative => _client == null;

    public NetworkClient(GameLogic gl)
    {
        _gl = gl;
    }

    public void Update()
    {
        if (_client == null)
        {
            return;
        }

        string[] messages = NetworkProtocol.Receive(_client);

        foreach (string msgStr in messages)
        {
            _gl.Log.WriteLine("Msg rcvd: " + msgStr);
            HandleMessage(msgStr);
        }
    }

    public bool TryConnect(ConnectionInfo connectionInfo)
    {
        try
        {
            _client = new TcpClient(connectionInfo.IpAddress, connectionInfo.Port);
            string msgStr = "CONNECT:" + connectionInfo.Serialize();
            NetworkProtocol.Send(_client, msgStr);
            return true;
        }
        catch (ArgumentNullException e)
        {
            _client = null;
            _gl.Log.WriteLine(":( CONNECT TO SERVER " + connectionInfo.IpAddress + " FAILED: " + e);
            return false;
        }
        catch (SocketException e) // server not available
        {
            _client = null;

            if (connectionInfo.IpAddress == LocalHost)
            {
                _gl.Log.WriteLine(":) CONNECT TO LOCAL SERVER FAILED: PROBABLY NO LOCAL SERVER RUNNING, TRYING GAMELIFT");
            }
            else
            {
                _gl.Log.WriteLine(":( CONNECT TO SERVER " + connectionInfo.IpAddress + "FAILED: " + e + " (ARE YOU ON THE *AMAZON*INTERNAL*NETWORK*?)");
            }

            return false;
        }
    }

    public void Ready()
    {
        if (_client == null)
        {
            return;
        }

        string msgStr = "READY:";

        try
        {
            NetworkProtocol.Send(_client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            _gl.Log.WriteLine("Ready failed: Disconnected" + e);
        }
    }

    public void TransmitInput(int playerIdx, Chord chord)
    {
        if (_client == null)
        {
            return;
        }

        string msgStr = "INPUT:" + chord.Serialize();

        try
        {
            NetworkProtocol.Send(_client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            _gl.Log.WriteLine("TransmitInput failed: Disconnected" + e);
        }
    }

    public void End()
    {
        if (_client == null)
        {
            return;
        }

        string msgStr = "END:";

        try
        {
            NetworkProtocol.Send(_client, msgStr);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
            _gl.Log.WriteLine("End failed: Disconnected" + e);
        }
    }

    public void Disconnect()
    {
        if (_client == null)
        {
            return;
        }

        string msgStr = "DISCONNECT:";

        try
        {
            NetworkProtocol.Send(_client, msgStr);
        }

        finally
        {
            HandleDisconnect();
        }
    }

    private void HandleMessage(string msgStr)
    {
        // parse message and pass json string to relevant handler for deserialization
        //gl.log.WriteLine("Msg rcvd:" + msgStr);
        string delimiter = ":";
        string json = msgStr.Substring(msgStr.IndexOf(delimiter) + delimiter.Length);

        if (msgStr[0] == 'S')
        {
            HandleState(json);
        }

        if (msgStr[0] == 'L')
        {
            HandleLog(json);
        }

        if (msgStr[0] == 'R')
        {
            HandleReject();
        }

        if (msgStr[0] == 'D')
        {
            HandleDisconnect();
        }
    }

    private void HandleState(string msgStr)
    {
        _gl.SetState(msgStr);
    }

    private void HandleLog(string msgStr)
    {
        _gl.Log.WriteLine(msgStr);
    }

    private void HandleReject()
    {
        _gl.Log.WriteLine(":( CONNECT TO SERVER REJECTED: game already full");
        NetworkStream stream = _client.GetStream();
        stream.Close();
        _client.Close();
        _client = null;
        _gl.ClientConnected = false;
    }

    private void HandleDisconnect()
    {
        _gl.EndGame();

        if (_client.Connected)
        {
            NetworkStream stream = _client.GetStream();
            stream.Close();
        }

        _client.Close();
        _client = null;
        _gl.ClientConnected = false;
    }
}

#endif
