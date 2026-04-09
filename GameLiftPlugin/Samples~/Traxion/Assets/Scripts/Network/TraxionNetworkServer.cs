// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// TCP server that drives the Traxion simulation.
/// Accepts up to <see cref="TraxionConfig.MaxPlayers"/> clients, collects
/// their direction inputs every frame, advances the simulation, and broadcasts
/// the resulting <see cref="TraxionGameState"/> snapshot back to all clients.
/// </summary>
public class TraxionNetworkServer
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly TraxionManager  _manager;
    private readonly TraxionSimulation _sim;
    private readonly TcpListener       _listener;
    private readonly TcpClient[]       _clients        = new TcpClient[TraxionConfig.MaxPlayers];
    private readonly bool[]            _ready          = new bool[TraxionConfig.MaxPlayers];
    private readonly string[]          _playerSessions = new string[TraxionConfig.MaxPlayers];

    private int ConnectedCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
                if (_clients[i] != null) n++;
            return n;
        }
    }

    // ── Construction ──────────────────────────────────────────────────────────

    public TraxionNetworkServer(TraxionManager manager, TraxionSimulation sim, int port)
    {
        _manager  = manager;
        _sim      = sim;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Debug.Log($"[Traxion Server] Listening on port {port}");
    }

    // ── Per-frame update ──────────────────────────────────────────────────────

    /// <summary>Call from MonoBehaviour.Update.</summary>
    public void Update(float deltaTime)
    {
        AcceptPending();
        ReceiveMessages();
        bool stateChanged = _sim.Update(deltaTime);
        if (stateChanged) BroadcastState();
    }

    // ── Accept new connections ────────────────────────────────────────────────

    private void AcceptPending()
    {
        if (!_listener.Pending()) return;

        TcpClient incoming = _listener.AcceptTcpClient();

        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
        {
            if (_clients[i] != null) continue;

            _clients[i] = incoming;
            Debug.Log($"[Traxion Server] Player {i} connected");
            return;
        }

        // Game is full — reject
        try   { TraxionProtocol.Send(incoming, "REJECT:game full"); }
        catch { /* best-effort */ }
        incoming.Close();
    }

    // ── Receive & dispatch ────────────────────────────────────────────────────

    private void ReceiveMessages()
    {
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
        {
            if (_clients[i] == null) continue;

            string[] msgs = TraxionProtocol.Receive(_clients[i]);
            foreach (string msg in msgs)
                Dispatch(i, msg);
        }
    }

    private void Dispatch(int playerIdx, string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;
        int colon    = msg.IndexOf(':');
        string verb  = colon < 0 ? msg : msg.Substring(0, colon);
        string body  = colon < 0 ? "" : msg.Substring(colon + 1);

        switch (verb)
        {
            case "CONNECT":    HandleConnect(playerIdx, body);    break;
            case "READY":      HandleReady(playerIdx);            break;
            case "INPUT":      HandleInput(playerIdx, body);      break;
            case "END":        HandleEnd();                       break;
            case "DISCONNECT": HandleDisconnect(playerIdx);       break;
            default:
                Debug.LogWarning($"[Traxion Server] Unknown message: {verb}");
                break;
        }
    }

    // ── Message handlers ──────────────────────────────────────────────────────

    private void HandleConnect(int playerIdx, string json)
    {
        Debug.Log($"[Traxion Server] CONNECT player {playerIdx}");

        // Validate player session via GameLift
        var info = TraxionConnectionInfo.FromJson(json);
        if (!_manager.AcceptPlayerSession(info.playerSessionId))
        {
            Send(playerIdx, "DISCONNECT:session rejected");
            Disconnect(playerIdx);
            return;
        }

        _playerSessions[playerIdx] = info.playerSessionId;
        BroadcastState();
    }

    private void HandleReady(int playerIdx)
    {
        _ready[playerIdx] = true;
        Debug.Log($"[Traxion Server] Player {playerIdx} ready");

        // Start game once every connected + ready client has confirmed
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
            if (_clients[i] != null && !_ready[i]) return;

        int count = ConnectedCount;
        if (count >= TraxionConfig.MinPlayersToStart)
        {
            _sim.InitialiseGame(count);
            BroadcastState();
        }
    }

    private void HandleInput(int playerIdx, string json)
    {
        var input = JsonUtility.FromJson<NeonInputMessage>(json);
        _sim.QueueDirection(input.playerId, input.direction);
    }

    private void HandleEnd()
    {
        Debug.Log("[Traxion Server] END requested — terminating session");
        Broadcast("DISCONNECT:");
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
            Disconnect(i);
        _manager.TerminateSession();
    }

    private void HandleDisconnect(int playerIdx)
    {
        _sim.SetConnected(playerIdx, false);
        Disconnect(playerIdx);

        if (ConnectedCount == 0)
            HandleEnd();
        else
            BroadcastState();
    }

    // ── Broadcast helpers ─────────────────────────────────────────────────────

    private void BroadcastState()
    {
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
        {
            if (_clients[i] == null) continue;
            string json = _sim.Serialise(i);
            Send(i, $"STATE:{json}");
        }
    }

    private void Broadcast(string msg)
    {
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
            Send(i, msg);
    }

    private void Send(int playerIdx, string msg)
    {
        if (_clients[playerIdx] == null) return;
        try
        {
            TraxionProtocol.Send(_clients[playerIdx], msg);
        }
        catch (Exception e) when (e is SocketException || e is InvalidOperationException)
        {
            Debug.LogWarning($"[Traxion Server] Send to player {playerIdx} failed: {e.Message}");
            Disconnect(playerIdx);
        }
    }

    // ── Connection teardown ───────────────────────────────────────────────────

    private void Disconnect(int playerIdx)
    {
        var client = _clients[playerIdx];
        if (client == null) return;

        if (_playerSessions[playerIdx] != null)
        {
            _manager.RemovePlayerSession(_playerSessions[playerIdx]);
            _playerSessions[playerIdx] = null;
        }

        try   { client.GetStream()?.Close(); } catch { /* best-effort */ }
        try   { client.Close(); }              catch { /* best-effort */ }

        _clients[playerIdx] = null;
        _ready[playerIdx]   = false;
    }

    /// <summary>Graceful shutdown called from OnApplicationQuit.</summary>
    public void Shutdown()
    {
        Broadcast("DISCONNECT:");
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
            Disconnect(i);
        try { _listener.Stop(); } catch { /* best-effort */ }
    }
}

#endif // UNITY_SERVER
