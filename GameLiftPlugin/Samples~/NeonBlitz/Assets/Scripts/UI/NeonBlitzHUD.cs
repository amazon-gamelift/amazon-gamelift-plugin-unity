// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game heads-up display.
///
/// Canvas hierarchy expected (create in Editor or via <see cref="NeonBlitzBootstrap"/>):
///   Canvas
///     HUD
///       TimerText   — countdown / match timer
///       ScorePanel  — score bars for each player
///         ScoreEntry_0..3
///       PhaseText   — "COUNTDOWN 3", "GO!", "GAME OVER", etc.
///       PowerUpBar  — shows active power-up icon for local player
///
/// All Text references are wired via Inspector fields; the component degrades
/// gracefully when fields are left null (nothing crashes, just missing text).
/// </summary>
public class NeonBlitzHUD : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Timer")]
    [SerializeField] private Text _timerText;

    [Header("Score entries (0..3)")]
    [SerializeField] private Text[]  _scoreTexts   = new Text[NeonBlitzConfig.MaxPlayers];
    [SerializeField] private Image[] _scoreIcons   = new Image[NeonBlitzConfig.MaxPlayers];

    [Header("Phase / message")]
    [SerializeField] private Text  _phaseText;
    [SerializeField] private float _phaseTextFadeTime = 2f;

    [Header("Power-up indicator")]
    [SerializeField] private Text  _powerUpText;
    [SerializeField] private Image _powerUpIcon;

    // ── Player colours (must match NeonBlitzRenderer) ────────────────────────
    private static readonly Color[] s_playerColors =
    {
        Color.HSVToRGB(0.50f, 1f, 1f),
        Color.HSVToRGB(0.83f, 1f, 1f),
        Color.HSVToRGB(0.28f, 1f, 0.9f),
        Color.HSVToRGB(0.08f, 1f, 1f),
    };

    // ── Power-up display names ────────────────────────────────────────────────
    private static readonly string[] s_puNames =
        { "", "SPEED", "SHIELD", "FREEZE", "BOMB" };

    // ── Phase message state ───────────────────────────────────────────────────
    private float     _phaseFadeTimer;
    private NeonGamePhase _lastPhase = NeonGamePhase.Lobby;

    // ── Public API ────────────────────────────────────────────────────────────

    public void Refresh(NeonBlitzGameState state)
    {
        UpdateTimer(state);
        UpdateScores(state);
        UpdatePowerUpIndicator(state);
        UpdatePhaseMessage(state);
    }

    // ── Timer ─────────────────────────────────────────────────────────────────

    private void UpdateTimer(NeonBlitzGameState state)
    {
        if (_timerText == null) return;

        switch (state.phase)
        {
            case NeonGamePhase.Countdown:
                int cd = Mathf.CeilToInt(state.countdownTimer);
                _timerText.text  = cd > 0 ? cd.ToString() : "GO!";
                _timerText.color = Color.red;
                break;

            case NeonGamePhase.Playing:
                int secs         = Mathf.CeilToInt(state.timeRemaining);
                int min          = secs / 60;
                int sec          = secs % 60;
                _timerText.text  = $"{min}:{sec:00}";
                _timerText.color = secs <= 10 ? Color.red : Color.white;
                break;

            case NeonGamePhase.GameOver:
                _timerText.text  = "0:00";
                _timerText.color = Color.grey;
                break;
        }
    }

    // ── Scores ────────────────────────────────────────────────────────────────

    private void UpdateScores(NeonBlitzGameState state)
    {
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            var p = state.players[i];

            if (_scoreTexts[i] != null)
            {
                if (!p.connected)
                {
                    _scoreTexts[i].text  = "—";
                    _scoreTexts[i].color = Color.grey;
                }
                else
                {
                    _scoreTexts[i].text  = p.score.ToString();
                    _scoreTexts[i].color = p.alive ? s_playerColors[i] : Color.grey * 0.6f;
                }
            }

            if (_scoreIcons[i] != null)
                _scoreIcons[i].color = p.connected && p.alive
                    ? s_playerColors[i]
                    : new Color(0.2f, 0.2f, 0.2f, 0.4f);
        }
    }

    // ── Power-up indicator ────────────────────────────────────────────────────

    private void UpdatePowerUpIndicator(NeonBlitzGameState state)
    {
        int localId = state.localPlayerId;
        if (localId < 0 || localId >= NeonBlitzConfig.MaxPlayers) return;
        var p = state.players[localId];

        bool hasPowerUp = p.activePowerUp != NeonPowerUpType.None && p.powerUpTimer > 0f;

        if (_powerUpText != null)
            _powerUpText.text = hasPowerUp
                ? $"{s_puNames[(int)p.activePowerUp]}  {p.powerUpTimer:F1}s"
                : "";

        if (_powerUpIcon != null)
            _powerUpIcon.enabled = hasPowerUp;
    }

    // ── Phase message ─────────────────────────────────────────────────────────

    private void UpdatePhaseMessage(NeonBlitzGameState state)
    {
        if (_phaseText == null) return;

        if (state.phase != _lastPhase)
        {
            _lastPhase = state.phase;
            switch (state.phase)
            {
                case NeonGamePhase.Countdown:
                    ShowPhaseText("GET READY!", Color.yellow);
                    break;
                case NeonGamePhase.Playing:
                    ShowPhaseText("GO!", Color.green, autoClear: true);
                    break;
                case NeonGamePhase.GameOver:
                    int wid = state.winnerId;
                    string winMsg = wid >= 0
                        ? $"PLAYER {wid + 1} WINS!"
                        : "DRAW!";
                    ShowPhaseText(winMsg, wid >= 0 ? s_playerColors[wid] : Color.white);
                    break;
            }
        }

        // Fade out auto-clear messages
        if (_phaseFadeTimer > 0f)
        {
            _phaseFadeTimer -= Time.deltaTime;
            float alpha     = Mathf.Clamp01(_phaseFadeTimer / _phaseTextFadeTime);
            var   c         = _phaseText.color;
            _phaseText.color = new Color(c.r, c.g, c.b, alpha);
            if (_phaseFadeTimer <= 0f) _phaseText.text = "";
        }
    }

    private void ShowPhaseText(string msg, Color color, bool autoClear = false)
    {
        _phaseText.text  = msg;
        _phaseText.color = color;
        _phaseFadeTimer  = autoClear ? _phaseTextFadeTime : 0f;
    }
}

#endif // !UNITY_SERVER
