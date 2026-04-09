// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shown at end of match.  Displays final scores, the winner banner, and
/// a Play Again / Main Menu button pair.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Winner banner")]
    [SerializeField] private Text  _winnerText;
    [SerializeField] private Image _winnerBanner;

    [Header("Score entries")]
    [SerializeField] private Text[]  _finalScoreTexts  = new Text[TraxionConfig.MaxPlayers];
    [SerializeField] private Text[]  _finalRankTexts   = new Text[TraxionConfig.MaxPlayers];
    [SerializeField] private Image[] _finalScoreIcons  = new Image[TraxionConfig.MaxPlayers];

    [Header("Buttons")]
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _mainMenuButton;

    // ── Player colours ────────────────────────────────────────────────────────
    private static readonly Color[] s_playerColors =
    {
        Color.HSVToRGB(0.50f, 1f, 1f),
        Color.HSVToRGB(0.83f, 1f, 1f),
        Color.HSVToRGB(0.28f, 1f, 0.9f),
        Color.HSVToRGB(0.08f, 1f, 1f),
    };

    // ── Events ────────────────────────────────────────────────────────────────
    public event Action OnPlayAgain;
    public event Action OnMainMenu;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _playAgainButton?.onClick.AddListener(() => OnPlayAgain?.Invoke());
        _mainMenuButton ?.onClick.AddListener(() => OnMainMenu ?.Invoke());
        Hide();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()  => gameObject.SetActive(true);
    public void Hide()  => gameObject.SetActive(false);

    /// <summary>Populate the screen from the final game state.</summary>
    public void Populate(TraxionGameState state)
    {
        // Winner banner
        int wid = state.winnerId;
        if (_winnerText != null)
        {
            _winnerText.text  = wid >= 0 ? $"PLAYER {wid + 1} WINS!" : "DRAW!";
            _winnerText.color = wid >= 0 ? s_playerColors[wid] : Color.white;
        }

        if (_winnerBanner != null && wid >= 0)
            _winnerBanner.color = s_playerColors[wid] * 0.4f;

        // Sort players by score descending
        var order = SortByScore(state);

        for (int rank = 0; rank < TraxionConfig.MaxPlayers; rank++)
        {
            int i = order[rank];
            var p = state.players[i];

            if (_finalScoreTexts[i] != null)
            {
                _finalScoreTexts[i].text  = p.connected ? p.score.ToString() : "—";
                _finalScoreTexts[i].color = p.connected ? s_playerColors[i] : Color.grey;
            }

            if (_finalRankTexts[i] != null)
            {
                _finalRankTexts[i].text  = p.connected ? RankLabel(rank) : "";
                _finalRankTexts[i].color = RankColor(rank);
            }

            if (_finalScoreIcons[i] != null)
                _finalScoreIcons[i].color = p.connected
                    ? s_playerColors[i]
                    : new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int[] SortByScore(TraxionGameState state)
    {
        int[] idx = { 0, 1, 2, 3 };
        System.Array.Sort(idx, (a, b) => state.players[b].score - state.players[a].score);
        return idx;
    }

    private static string RankLabel(int rank) => rank switch
    {
        0 => "1st",
        1 => "2nd",
        2 => "3rd",
        _ => "4th",
    };

    private static Color RankColor(int rank) => rank switch
    {
        0 => new Color(1f,    0.84f, 0f,   1f),  // Gold
        1 => new Color(0.75f, 0.75f, 0.75f, 1f),  // Silver
        2 => new Color(0.8f,  0.5f,  0.2f, 1f),  // Bronze
        _ => Color.grey,
    };
}

#endif // !UNITY_SERVER
