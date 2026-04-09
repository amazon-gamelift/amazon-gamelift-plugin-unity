// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER

using System;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using UnityEngine;

/// <summary>
/// Wraps the Amazon GameLift Server SDK for Neon Blitz server builds.
///
/// Call <see cref="Initialise"/> once from <see cref="NeonBlitzBootstrap"/>.
/// The <see cref="NeonBlitzNetworkServer"/> uses <see cref="AcceptPlayerSession"/>
/// and <see cref="RemovePlayerSession"/> to gate connections, and calls
/// <see cref="TerminateGameSession"/> when the match ends.
/// </summary>
public class NeonBlitzGameLiftServer : MonoBehaviour
{
    // ── Properties ────────────────────────────────────────────────────────────
    public bool IsConnected { get; private set; }
    public int  ServerPort  { get; private set; } = NeonBlitzConfig.DefaultPort;

    // ── Initialise ────────────────────────────────────────────────────────────

    public bool Initialise()
    {
        var outcome = GameLiftServerAPI.InitSDK();
        if (!outcome.Success)
        {
            Debug.LogError($"[NeonBlitz GameLift] InitSDK failed: {outcome.Error}");
            return false;
        }

        var processParams = new ProcessParameters(
            onStartGameSession:   OnStartGameSession,
            onUpdateGameSession:  OnUpdateGameSession,
            onProcessTerminate:   OnProcessTerminate,
            onHealthCheck:        OnHealthCheck,
            port:                 ServerPort,
            logParameters:        new LogParameters(new[] { "/local/game/logs/NeonBlitz.log" })
        );

        var readyOutcome = GameLiftServerAPI.ProcessReady(processParams);
        if (!readyOutcome.Success)
        {
            Debug.LogError($"[NeonBlitz GameLift] ProcessReady failed: {readyOutcome.Error}");
            return false;
        }

        IsConnected = true;
        Debug.Log($"[NeonBlitz GameLift] ProcessReady — listening on port {ServerPort}");
        return true;
    }

    // ── Session management ────────────────────────────────────────────────────

    public bool AcceptPlayerSession(string playerSessionId)
    {
        if (!IsConnected || string.IsNullOrEmpty(playerSessionId)) return true; // local testing

        var outcome = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);
        if (!outcome.Success)
            Debug.LogWarning($"[NeonBlitz GameLift] AcceptPlayerSession failed: {outcome.Error}");
        return outcome.Success;
    }

    public void RemovePlayerSession(string playerSessionId)
    {
        if (!IsConnected || string.IsNullOrEmpty(playerSessionId)) return;

        var outcome = GameLiftServerAPI.RemovePlayerSession(playerSessionId);
        if (!outcome.Success)
            Debug.LogWarning($"[NeonBlitz GameLift] RemovePlayerSession failed: {outcome.Error}");
    }

    public void TerminateGameSession()
    {
        if (!IsConnected) return;
        var outcome = GameLiftServerAPI.TerminateGameSession();
        if (!outcome.Success)
            Debug.LogWarning($"[NeonBlitz GameLift] TerminateGameSession failed: {outcome.Error}");
        IsConnected = false;
    }

    // ── GameLift callbacks ────────────────────────────────────────────────────

    private void OnStartGameSession(GameSession session)
    {
        Debug.Log($"[NeonBlitz GameLift] StartGameSession — id:{session.GameSessionId}");

        // Extract custom port if supplied as a game-session property
        foreach (var prop in session.GameProperties)
        {
            if (prop.Key == "port" && int.TryParse(prop.Value, out int p))
                ServerPort = p;
        }

        GameLiftServerAPI.ActivateGameSession();
    }

    private void OnUpdateGameSession(UpdatedGameSession session)
    {
        Debug.Log("[NeonBlitz GameLift] UpdateGameSession");
    }

    private void OnProcessTerminate()
    {
        Debug.Log("[NeonBlitz GameLift] ProcessTerminate — shutting down");
        Application.Quit();
    }

    private bool OnHealthCheck() => true;

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void OnApplicationQuit()
    {
        if (IsConnected) GameLiftServerAPI.Destroy();
    }
}

#endif // UNITY_SERVER
