// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Runtime;
using UnityEngine;

/// <summary>
/// Fetches a <see cref="NeonBlitzConnectionInfo"/> from Amazon GameLift on the
/// client so the player can connect to a game server.
///
/// Falls back to a local-server connection (localhost:<see cref="NeonBlitzConfig.DefaultPort"/>)
/// when GameLift is not configured — useful during development.
/// </summary>
public class NeonBlitzGameLiftClient : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Tooltip("Leave null to auto-locate the GameLiftCoreApi component in the scene.")]
    [SerializeField] private GameLiftCoreApi _gameLiftApi;

    // ── Properties ────────────────────────────────────────────────────────────
    public bool IsGameLiftAvailable => _gameLiftApi != null && _gameLiftApi.IsConnected;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (_gameLiftApi == null)
            _gameLiftApi = FindObjectOfType<GameLiftCoreApi>();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Asynchronously obtain connection details for a Neon Blitz session.
    /// Returns (true, info) on success; (false, null) on failure.
    /// </summary>
    public async Task<(bool success, NeonBlitzConnectionInfo info)> GetConnectionInfo(
        string playerName,
        CancellationToken ct = default)
    {
        if (!IsGameLiftAvailable)
        {
            // Local-testing fallback
            Debug.Log("[NeonBlitz Client] GameLift not available — using localhost");
            return (true, new NeonBlitzConnectionInfo
            {
                ipAddress       = "localhost",
                port            = NeonBlitzConfig.DefaultPort,
                playerSessionId = "",
                playerName      = playerName,
            });
        }

        try
        {
            var response = await _gameLiftApi.GetConnectionInfo(ct);
            if (!response.Success)
            {
                Debug.LogWarning($"[NeonBlitz Client] GetConnectionInfo failed: {response.ErrorMessage}");
                return (false, null);
            }

            return (true, new NeonBlitzConnectionInfo
            {
                ipAddress       = response.IpAddress,
                port            = response.Port,
                playerSessionId = response.PlayerSessionId,
                playerName      = playerName,
            });
        }
        catch (OperationCanceledException)
        {
            return (false, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"[NeonBlitz Client] GetConnectionInfo exception: {e}");
            return (false, null);
        }
    }
}

#endif // !UNITY_SERVER
