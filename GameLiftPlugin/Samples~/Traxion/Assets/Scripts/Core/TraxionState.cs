// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;

// ── Enumerations ──────────────────────────────────────────────────────────────

public enum NeonDirection { Up = 0, Right = 1, Down = 2, Left = 3 }

public enum NeonPowerUpType { None = 0, Speed = 1, Shield = 2, Freeze = 3, Bomb = 4 }

public enum NeonGamePhase { Lobby = 0, Countdown = 1, Playing = 2, GameOver = 3 }

// ── Per-player state ──────────────────────────────────────────────────────────

[Serializable]
public class NeonPlayerState
{
    public int            playerId;
    public string         playerName       = "Player";
    public bool           connected;
    public bool           alive;

    // Position & movement
    public int            headX;
    public int            headY;
    public NeonDirection  direction        = NeonDirection.Right;
    public NeonDirection  queuedDirection  = NeonDirection.Right;

    // Score & trail
    public int            score;
    public int            trailCellCount;

    // Power-up
    public NeonPowerUpType activePowerUp   = NeonPowerUpType.None;
    public float          powerUpTimer;
    public bool           isShielded;

    public void Reset()
    {
        alive          = false;
        connected      = false;
        score          = 0;
        trailCellCount = 0;
        activePowerUp  = NeonPowerUpType.None;
        powerUpTimer   = 0f;
        isShielded     = false;
    }
}

// ── Power-up pickup state ────────────────────────────────────────────────────

[Serializable]
public class NeonPowerUpPickup
{
    public int            id;
    public int            x;
    public int            y;
    public NeonPowerUpType type;
    public bool           active;
}

// ── Full arena snapshot (serialised over network) ─────────────────────────────

[Serializable]
public class TraxionGameState
{
    // Match metadata
    public NeonGamePhase  phase            = NeonGamePhase.Lobby;
    public float          timeRemaining    = TraxionConfig.GameDuration;
    public float          countdownTimer   = TraxionConfig.CountdownDuration;
    public ulong          tick;
    public int            localPlayerId    = -1;
    public int            winnerId         = -1;

    // Players
    public NeonPlayerState[]    players;

    // Power-up pickups on the grid
    public NeonPowerUpPickup[]  powerUps;

    /// <summary>
    /// Flattened grid [y * GridWidth + x].
    ///   0          = empty
    ///   1..MaxPlayers = owned by that player (colour index = value - 1)
    /// </summary>
    public int[] grid;

    public TraxionGameState()
    {
        players  = new NeonPlayerState[TraxionConfig.MaxPlayers];
        for (int i = 0; i < TraxionConfig.MaxPlayers; i++)
            players[i] = new NeonPlayerState { playerId = i };

        powerUps = new NeonPowerUpPickup[TraxionConfig.MaxActivePowerUps];
        for (int i = 0; i < TraxionConfig.MaxActivePowerUps; i++)
            powerUps[i] = new NeonPowerUpPickup();

        grid = new int[TraxionConfig.GridWidth * TraxionConfig.GridHeight];
        winnerId = -1;
    }

    // ── Grid helpers ─────────────────────────────────────────────────────────

    public int  GetCell(int x, int y)           => grid[y * TraxionConfig.GridWidth + x];
    public void SetCell(int x, int y, int value) => grid[y * TraxionConfig.GridWidth + x] = value;
    public bool InBounds(int x, int y)           => x >= 0 && x < TraxionConfig.GridWidth
                                                         && y >= 0 && y < TraxionConfig.GridHeight;
}

// ── Wire messages ─────────────────────────────────────────────────────────────

[Serializable]
public class NeonInputMessage
{
    public int           playerId;
    public NeonDirection direction;
}
