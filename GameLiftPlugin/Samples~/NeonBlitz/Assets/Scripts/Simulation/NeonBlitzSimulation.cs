// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

// The simulation is entirely authoritative — it runs only on the server.
// Clients receive serialised snapshots; they never run this code.
#if UNITY_SERVER

using System;
using UnityEngine;

/// <summary>
/// All game-logic for Neon Blitz lives here.
/// Call <see cref="Update"/> every frame; it returns <c>true</c> whenever
/// the state changed so the network layer knows to broadcast.
/// </summary>
public class NeonBlitzSimulation
{
    // ── State ─────────────────────────────────────────────────────────────────
    private NeonBlitzGameState _state;

    // Per-player move timers (modified by speed / freeze power-ups)
    private readonly float[] _moveInterval  = new float[NeonBlitzConfig.MaxPlayers];
    private readonly float[] _moveAccum     = new float[NeonBlitzConfig.MaxPlayers];

    private int  _powerUpSpawnCountdown;
    private int  _nextPowerUpId;
    private readonly System.Random _rng = new System.Random();

    // Snapshot of floor(timeRemaining) from the last frame — used to tick
    // per-second survival bonus without a coroutine.
    private int _lastSecond;

    public NeonBlitzGameState State => _state;

    // ── Initialisation ────────────────────────────────────────────────────────

    public NeonBlitzSimulation()
    {
        _state = new NeonBlitzGameState();
        ResetMoveIntervals();
    }

    private void ResetMoveIntervals()
    {
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
            _moveInterval[i] = NeonBlitzConfig.BaseMoveInterval;
    }

    /// <summary>
    /// Prepare a fresh match.  Call once all players have connected.
    /// </summary>
    public void InitialiseGame(int playerCount)
    {
        _state = new NeonBlitzGameState
        {
            phase          = NeonGamePhase.Countdown,
            timeRemaining  = NeonBlitzConfig.GameDuration,
            countdownTimer = NeonBlitzConfig.CountdownDuration,
            tick           = 0,
            winnerId       = -1,
        };

        _powerUpSpawnCountdown = NeonBlitzConfig.PowerUpSpawnEveryNTicks;
        _nextPowerUpId         = 0;
        _lastSecond            = Mathf.FloorToInt(NeonBlitzConfig.GameDuration);

        var startPos = StartPositions();
        var startDir = StartDirections();

        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            var p = _state.players[i];
            p.Reset();
            p.playerId = i;

            if (i < playerCount)
            {
                p.connected      = true;
                p.alive          = true;
                p.headX          = startPos[i].x;
                p.headY          = startPos[i].y;
                p.direction      = startDir[i];
                p.queuedDirection = startDir[i];
                _state.SetCell(p.headX, p.headY, i + 1);
                p.trailCellCount = 1;
            }
        }

        ResetMoveIntervals();
        Array.Clear(_moveAccum, 0, _moveAccum.Length);
    }

    // Corner starts pointing inward so players immediately converge.
    private static Vector2Int[] StartPositions()
    {
        const int M = 3;
        int W = NeonBlitzConfig.GridWidth  - 1;
        int H = NeonBlitzConfig.GridHeight - 1;
        return new[]
        {
            new Vector2Int(M,   M),
            new Vector2Int(W-M, H-M),
            new Vector2Int(W-M, M),
            new Vector2Int(M,   H-M),
        };
    }

    private static NeonDirection[] StartDirections() =>
        new[] { NeonDirection.Right, NeonDirection.Left,
                NeonDirection.Left,  NeonDirection.Right };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Queue a direction change.  Blocks 180° reversals.</summary>
    public void QueueDirection(int playerId, NeonDirection dir)
    {
        if ((uint)playerId >= NeonBlitzConfig.MaxPlayers) return;
        var p = _state.players[playerId];
        if (!p.alive) return;
        if (IsOpposite(p.direction, dir)) return;
        p.queuedDirection = dir;
    }

    /// <summary>Mark a player as disconnected mid-game.</summary>
    public void SetConnected(int playerId, bool connected)
    {
        if ((uint)playerId >= NeonBlitzConfig.MaxPlayers) return;
        var p = _state.players[playerId];
        p.connected = connected;
        if (!connected && p.alive)
        {
            p.alive = false;
            CheckWinCondition();
        }
    }

    /// <summary>
    /// Advance the simulation by <paramref name="dt"/> seconds.
    /// Returns <c>true</c> when the state has changed and should be broadcast.
    /// </summary>
    public bool Update(float dt)
    {
        switch (_state.phase)
        {
            case NeonGamePhase.Lobby:    return false;
            case NeonGamePhase.GameOver: return false;
            case NeonGamePhase.Countdown:
                return TickCountdown(dt);
            case NeonGamePhase.Playing:
                return TickPlaying(dt);
            default: return false;
        }
    }

    public string Serialise(int forPlayerId)
    {
        _state.localPlayerId = forPlayerId;
        return JsonUtility.ToJson(_state);
    }

    // ── Countdown phase ───────────────────────────────────────────────────────

    private bool TickCountdown(float dt)
    {
        _state.countdownTimer -= dt;
        if (_state.countdownTimer > 0f) return true;   // still counting down

        _state.countdownTimer = 0f;
        _state.phase = NeonGamePhase.Playing;
        return true;
    }

    // ── Playing phase ─────────────────────────────────────────────────────────

    private bool TickPlaying(float dt)
    {
        bool changed = false;

        // Decrement match clock
        _state.timeRemaining -= dt;
        if (_state.timeRemaining <= 0f)
        {
            _state.timeRemaining = 0f;
            EndGame();
            return true;
        }

        // Per-second survival bonus
        int curSecond = Mathf.FloorToInt(_state.timeRemaining);
        if (curSecond < _lastSecond)
        {
            _lastSecond = curSecond;
            for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
                if (_state.players[i].alive)
                    _state.players[i].score += NeonBlitzConfig.ScorePerSurvivalSecond;
            changed = true;
        }

        // Update power-up timers
        changed |= TickPowerUpTimers(dt);

        // Advance each player's trail
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            if (!_state.players[i].alive) continue;
            _moveAccum[i] += dt;
            if (_moveAccum[i] >= _moveInterval[i])
            {
                _moveAccum[i] = 0f;
                changed |= StepPlayer(i);
            }
        }

        if (changed)
        {
            _state.tick++;
            _powerUpSpawnCountdown--;
            if (_powerUpSpawnCountdown <= 0)
            {
                TrySpawnPowerUp();
                _powerUpSpawnCountdown = NeonBlitzConfig.PowerUpSpawnEveryNTicks;
            }
        }

        return changed;
    }

    // ── Player movement ───────────────────────────────────────────────────────

    private bool StepPlayer(int id)
    {
        var p = _state.players[id];
        p.direction = p.queuedDirection;

        var (dx, dy) = DirDelta(p.direction);
        int nx = p.headX + dx;
        int ny = p.headY + dy;

        // Out of bounds?
        if (!_state.InBounds(nx, ny))
        {
            Eliminate(id);
            return true;
        }

        // Power-up pickup?
        bool gotPowerUp = TryPickUp(id, nx, ny);

        // Trail collision?
        if (!gotPowerUp && _state.GetCell(nx, ny) != 0)
        {
            if (p.isShielded)
            {
                // Shield absorbs one hit
                p.isShielded     = false;
                p.activePowerUp  = NeonPowerUpType.None;
                p.powerUpTimer   = 0f;
                return true;
            }
            Eliminate(id);
            return true;
        }

        // Move forward
        p.headX = nx;
        p.headY = ny;
        _state.SetCell(nx, ny, id + 1);
        p.trailCellCount++;
        return true;
    }

    // ── Power-up pickup ───────────────────────────────────────────────────────

    private bool TryPickUp(int playerId, int x, int y)
    {
        for (int i = 0; i < NeonBlitzConfig.MaxActivePowerUps; i++)
        {
            var pu = _state.powerUps[i];
            if (!pu.active || pu.x != x || pu.y != y) continue;

            pu.active = false;
            ApplyPowerUp(playerId, pu.type);
            return true;
        }
        return false;
    }

    private void ApplyPowerUp(int id, NeonPowerUpType type)
    {
        var p = _state.players[id];
        p.activePowerUp = type;
        p.powerUpTimer  = NeonBlitzConfig.PowerUpDuration;

        switch (type)
        {
            case NeonPowerUpType.Speed:
                _moveInterval[id] = NeonBlitzConfig.BaseMoveInterval / NeonBlitzConfig.SpeedBoostFactor;
                break;

            case NeonPowerUpType.Shield:
                p.isShielded = true;
                break;

            case NeonPowerUpType.Freeze:
                for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
                    if (i != id && _state.players[i].alive)
                        _moveInterval[i] = NeonBlitzConfig.BaseMoveInterval / NeonBlitzConfig.FreezeFactor;
                break;

            case NeonPowerUpType.Bomb:
                DetonateBomb(id);
                p.activePowerUp = NeonPowerUpType.None;
                p.powerUpTimer  = 0f;
                break;
        }
    }

    private void DetonateBomb(int id)
    {
        var p  = _state.players[id];
        int r  = NeonBlitzConfig.BombBlastRadius;
        for (int dy = -r; dy <= r; dy++)
        for (int dx = -r; dx <= r; dx++)
        {
            int cx = p.headX + dx, cy = p.headY + dy;
            if (!_state.InBounds(cx, cy)) continue;
            int cell = _state.GetCell(cx, cy);
            if (cell > 0 && cell != id + 1)
            {
                _state.SetCell(cx, cy, 0);
                int victim = cell - 1;
                if ((uint)victim < NeonBlitzConfig.MaxPlayers)
                    _state.players[victim].trailCellCount =
                        Math.Max(0, _state.players[victim].trailCellCount - 1);
            }
        }
    }

    private bool TickPowerUpTimers(float dt)
    {
        bool changed = false;
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            var p = _state.players[i];
            if (p.activePowerUp == NeonPowerUpType.None || p.powerUpTimer <= 0f) continue;
            p.powerUpTimer -= dt;
            if (p.powerUpTimer <= 0f) { ExpirePowerUp(i); changed = true; }
        }
        return changed;
    }

    private void ExpirePowerUp(int id)
    {
        var p    = _state.players[id];
        var type = p.activePowerUp;
        p.activePowerUp = NeonPowerUpType.None;
        p.powerUpTimer  = 0f;
        p.isShielded    = false;

        switch (type)
        {
            case NeonPowerUpType.Speed:
                _moveInterval[id] = NeonBlitzConfig.BaseMoveInterval;
                break;

            case NeonPowerUpType.Freeze:
                for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
                    if (i != id && _state.players[i].activePowerUp != NeonPowerUpType.Speed)
                        _moveInterval[i] = NeonBlitzConfig.BaseMoveInterval;
                break;
        }
    }

    private void TrySpawnPowerUp()
    {
        // Find a free slot
        int slot = -1;
        for (int i = 0; i < NeonBlitzConfig.MaxActivePowerUps; i++)
            if (!_state.powerUps[i].active) { slot = i; break; }
        if (slot < 0) return;

        // Find an empty cell (max 30 attempts)
        for (int attempt = 0; attempt < 30; attempt++)
        {
            int x = _rng.Next(2, NeonBlitzConfig.GridWidth  - 2);
            int y = _rng.Next(2, NeonBlitzConfig.GridHeight - 2);
            if (_state.GetCell(x, y) != 0) continue;

            var pu    = _state.powerUps[slot];
            pu.id     = _nextPowerUpId++;
            pu.x      = x;
            pu.y      = y;
            pu.type   = (NeonPowerUpType)_rng.Next(1, 5);
            pu.active = true;
            return;
        }
    }

    // ── Elimination & win condition ───────────────────────────────────────────

    private void Eliminate(int id)
    {
        _state.players[id].alive = false;

        // Bonus to every surviving player
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
            if (i != id && _state.players[i].alive)
                _state.players[i].score += NeonBlitzConfig.ScoreElimination;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        int aliveCount = 0;
        int lastAlive  = -1;
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
            if (_state.players[i].connected && _state.players[i].alive)
            { aliveCount++; lastAlive = i; }

        if (aliveCount <= 1)
        {
            if (lastAlive >= 0)
            {
                _state.players[lastAlive].score += NeonBlitzConfig.ScoreWin;
                _state.winnerId = lastAlive;
            }
            EndGame();
        }
    }

    private void EndGame()
    {
        // Territory bonus: count trail cells
        int[] territory = new int[NeonBlitzConfig.MaxPlayers];
        foreach (int cell in _state.grid)
            if (cell > 0 && cell <= NeonBlitzConfig.MaxPlayers)
                territory[cell - 1]++;

        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
            _state.players[i].score += territory[i] * NeonBlitzConfig.ScorePerTerritoryCell;

        // Determine winner by score if not already set
        if (_state.winnerId < 0)
        {
            int best = -1;
            for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
                if (_state.players[i].connected && _state.players[i].score > best)
                    { best = _state.players[i].score; _state.winnerId = i; }
        }

        _state.phase = NeonGamePhase.GameOver;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsOpposite(NeonDirection a, NeonDirection b) =>
        (a == NeonDirection.Up    && b == NeonDirection.Down)  ||
        (a == NeonDirection.Down  && b == NeonDirection.Up)    ||
        (a == NeonDirection.Left  && b == NeonDirection.Right) ||
        (a == NeonDirection.Right && b == NeonDirection.Left);

    private static (int dx, int dy) DirDelta(NeonDirection d) => d switch
    {
        NeonDirection.Up    => ( 0,  1),
        NeonDirection.Down  => ( 0, -1),
        NeonDirection.Left  => (-1,  0),
        NeonDirection.Right => ( 1,  0),
        _                   => ( 0,  0),
    };
}

#endif // UNITY_SERVER
