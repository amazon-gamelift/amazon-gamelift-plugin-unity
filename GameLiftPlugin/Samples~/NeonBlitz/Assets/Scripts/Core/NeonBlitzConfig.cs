// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

/// <summary>
/// All tunable constants for Neon Blitz. Adjust to balance gameplay feel.
/// </summary>
public static class NeonBlitzConfig
{
    // ── Grid ─────────────────────────────────────────────────────────────────
    public const int GridWidth  = 22;
    public const int GridHeight = 32;   // Taller grid — portrait smartphone

    // ── Players ──────────────────────────────────────────────────────────────
    public const int MaxPlayers        = 4;
    public const int MinPlayersToStart = 2;

    // ── Timing ───────────────────────────────────────────────────────────────
    /// <summary>Seconds between each grid-step at normal speed.</summary>
    public const float BaseMoveInterval  = 0.13f;
    public const float GameDuration      = 120f;  // 2-minute match cap
    public const float CountdownDuration = 3f;

    // ── Speed modifiers ───────────────────────────────────────────────────────
    public const float SpeedBoostFactor  = 1.65f;  // divide interval by this
    public const float FreezeFactor      = 0.50f;  // multiply interval by this (slower)

    // ── Power-ups ─────────────────────────────────────────────────────────────
    public const int   PowerUpSpawnEveryNTicks = 10;
    public const int   MaxActivePowerUps       = 5;
    public const float PowerUpDuration         = 6f;
    public const int   BombBlastRadius         = 2;

    // ── Scoring ───────────────────────────────────────────────────────────────
    public const int ScorePerSurvivalSecond  = 1;
    public const int ScoreElimination        = 50;   // bonus to survivors on kill
    public const int ScoreWin                = 100;
    public const int ScorePerTerritoryCell   = 2;    // counted at game end

    // ── Network ───────────────────────────────────────────────────────────────
    public const int DefaultPort = 7778;

    // ── Visual cues (used by renderer) ───────────────────────────────────────
    // Player color in HSV — hue only; saturation and brightness set by renderer.
    public static readonly float[] PlayerHues = { 0.50f, 0.83f, 0.28f, 0.08f };
    // Cyan  ~180° | Magenta ~300° | Lime ~100° | Orange ~30°
}
