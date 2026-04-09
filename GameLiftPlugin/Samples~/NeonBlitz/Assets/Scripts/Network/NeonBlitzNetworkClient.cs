// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Manages the client-side TCP connection to the Neon Blitz game server.
/// Sends player inputs and receives authoritative state snapshots.
/// </summary>
public class NeonBlitzNetworkClient
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly NeonBlitzManager _manager;
    private TcpClient                 _client;

    public bool IsConnected  => _client != null;

    // ── Construction ──────────────────────────────────────────────────────────

    public NeonBlitzNetworkClient(NeonBlitzManager manager)
    {
        _manager = manager;
    }

    // ── Connect / disconnect ──────────────────────────────────────────────────

    /// <summary>
    /// Attempt a TCP connection.  Returns <c>true</c> on success.
    /// </summary>
    public bool TryConnect(NeonBlitzConnectionInfo info)
    {
        try
        {
            _client = new TcpClient(info.ipAddress, info.port);
            string payload = JsonUtility.ToJson(info);
            NeonBlitzProtocol.Send(_client, $"CONNECT:{payload}");
            Debug.Log($"[NeonBlitz Client] Connected to {info.ipAddress}:{info.port}");
            return true;
        }
        catch (Exception e)
        {
            _client = null;
            Debug.LogWarning($"[NeonBlitz Client] Connect failed: {e.Message}");
            return false;
        }
    }

    public void SendReady()          => SafeSend("READY:");
    public void SendEnd()            => SafeSend("END:");

    public void SendInput(NeonInputMessage input)
    {
        string json = JsonUtility.ToJson(input);
        SafeSend($"INPUT:{json}");
    }

    public void Disconnect()
    {
        SafeSend("DISCONNECT:");
        CloseSocket();
    }

    // ── Per-frame receive ─────────────────────────────────────────────────────

    /// <summary>Call from MonoBehaviour.Update to drain inbound messages.</summary>
    public void Update()
    {
        if (_client == null) return;

        string[] msgs = NeonBlitzProtocol.Receive(_client);
        foreach (string msg in msgs)
            Dispatch(msg);
    }

    // ── Dispatch ──────────────────────────────────────────────────────────────

    private void Dispatch(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;
        int    colon = msg.IndexOf(':');
        string verb  = colon < 0 ? msg : msg.Substring(0, colon);
        string body  = colon < 0 ? ""  : msg.Substring(colon + 1);

        switch (verb)
        {
            case "STATE":      _manager.ApplyServerState(body);  break;
            case "REJECT":     HandleReject(body);                break;
            case "DISCONNECT": HandleServerDisconnect();          break;
            default:
                Debug.LogWarning($"[NeonBlitz Client] Unknown message: {verb}");
                break;
        }
    }

    private void HandleReject(string reason)
    {
        Debug.LogWarning($"[NeonBlitz Client] Connection rejected: {reason}");
        CloseSocket();
        _manager.OnConnectionRejected(reason);
    }

    private void HandleServerDisconnect()
    {
        Debug.Log("[NeonBlitz Client] Server disconnected.");
        CloseSocket();
        _manager.OnServerDisconnected();
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private void SafeSend(string msg)
    {
        if (_client == null) return;
        try
        {
            NeonBlitzProtocol.Send(_client, msg);
        }
        catch (Exception e) when (e is SocketException || e is InvalidOperationException)
        {
            Debug.LogWarning($"[NeonBlitz Client] Send failed: {e.Message}");
            CloseSocket();
            _manager.OnServerDisconnected();
        }
    }

    private void CloseSocket()
    {
        if (_client == null) return;
        try   { _client.GetStream()?.Close(); } catch { /* best-effort */ }
        try   { _client.Close(); }              catch { /* best-effort */ }
        _client = null;
    }
}

#endif // !UNITY_SERVER
