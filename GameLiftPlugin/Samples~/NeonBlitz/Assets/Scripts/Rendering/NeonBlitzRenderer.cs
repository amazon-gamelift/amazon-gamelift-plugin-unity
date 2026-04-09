// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Collections;
using UnityEngine;

/// <summary>
/// Renders the Neon Blitz arena using a flat grid of <see cref="SpriteRenderer"/>s.
///
/// Design goals:
///   • No external assets required — all visuals are procedural.
///   • Neon glow illusion: bright saturated cells on a near-black background.
///   • Power-up pickups pulse in scale to attract attention.
///   • Bomb detonation flashes the screen white briefly.
///
/// Attach to a GameObject in the Game scene.  Set the <c>_whitePixel</c>
/// field in the Inspector (a 1×1 white Sprite) or the component will
/// auto-generate one at runtime.
/// </summary>
public class NeonBlitzRenderer : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Layout")]
    [SerializeField] private float _cellSize    = 0.28f;
    [SerializeField] private float _cellPadding = 0.02f;

    [Header("Colours")]
    [SerializeField] private Color _emptyColor    = new Color(0.04f, 0.04f, 0.08f, 1f);
    [SerializeField] private Color _flashColor    = new Color(1f,    1f,    1f,    0.85f);

    [Header("References")]
    [SerializeField] private Sprite _whitePixel;   // auto-generated if null
    [SerializeField] private Camera _mainCamera;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private SpriteRenderer[,] _cells;
    private SpriteRenderer[]  _puRenderers;
    private bool              _initialised;

    // Neon colours per player (bright, saturated)
    private static readonly Color[] s_playerColors =
    {
        FromHSV(0.50f, 1f, 1f),   // Cyan   — Player 0
        FromHSV(0.83f, 1f, 1f),   // Magenta — Player 1
        FromHSV(0.28f, 1f, 0.9f), // Lime    — Player 2
        FromHSV(0.08f, 1f, 1f),   // Orange  — Player 3
    };

    // Dimmer version painted into older trail cells (depth cue)
    private static readonly Color[] s_trailColors =
    {
        FromHSV(0.50f, 0.8f, 0.55f),
        FromHSV(0.83f, 0.8f, 0.55f),
        FromHSV(0.28f, 0.8f, 0.50f),
        FromHSV(0.08f, 0.8f, 0.55f),
    };

    // Power-up pickup colours
    private static readonly Color[] s_puColors =
    {
        Color.white,                  // None (unused)
        FromHSV(0.14f, 1f, 1f),      // Speed  — Yellow
        FromHSV(0.55f, 0.6f, 1f),    // Shield — Light-blue
        FromHSV(0.63f, 1f, 0.8f),    // Freeze — Indigo
        FromHSV(0.00f, 1f, 1f),      // Bomb   — Red
    };

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (_whitePixel == null) _whitePixel = CreateWhitePixelSprite();
        if (_mainCamera == null) _mainCamera = Camera.main;
        BuildGrid();
        BuildPowerUpRenderers();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Full repaint from a game-state snapshot.</summary>
    public void Render(NeonBlitzGameState state)
    {
        if (!_initialised) return;
        PaintGrid(state);
        PaintPowerUps(state);
        PaintHeads(state);
    }

    /// <summary>Flash white for one frame (bomb / elimination feedback).</summary>
    public void FlashScreen() => StartCoroutine(DoFlash());

    /// <summary>Pulse a single cell (e.g. player head after death).</summary>
    public void PulseCell(int x, int y) => StartCoroutine(DoPulse(x, y));

    // ── Grid construction ─────────────────────────────────────────────────────

    private void BuildGrid()
    {
        int W = NeonBlitzConfig.GridWidth;
        int H = NeonBlitzConfig.GridHeight;
        _cells = new SpriteRenderer[W, H];

        float step   = _cellSize + _cellPadding;
        float originX = -(W - 1) * step * 0.5f;
        float originY = -(H - 1) * step * 0.5f;

        var parent = new GameObject("Grid").transform;
        parent.SetParent(transform, false);

        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            var go = new GameObject($"Cell_{x}_{y}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(
                originX + x * step,
                originY + y * step,
                0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite        = _whitePixel;
            sr.color         = _emptyColor;
            sr.drawMode      = SpriteDrawMode.Sliced;
            sr.size          = Vector2.one * _cellSize;
            _cells[x, y]     = sr;
        }

        // Fit camera to grid
        if (_mainCamera != null && _mainCamera.orthographic)
        {
            float vertExtent = (H * step) * 0.5f + step;
            _mainCamera.orthographicSize = vertExtent;
        }

        _initialised = true;
    }

    private void BuildPowerUpRenderers()
    {
        _puRenderers = new SpriteRenderer[NeonBlitzConfig.MaxActivePowerUps];
        var parent   = new GameObject("PowerUps").transform;
        parent.SetParent(transform, false);

        float step    = _cellSize + _cellPadding;
        float W       = NeonBlitzConfig.GridWidth;
        float H       = NeonBlitzConfig.GridHeight;
        float originX = -(W - 1) * step * 0.5f;
        float originY = -(H - 1) * step * 0.5f;

        for (int i = 0; i < NeonBlitzConfig.MaxActivePowerUps; i++)
        {
            var go = new GameObject($"PowerUp_{i}");
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = _whitePixel;
            sr.color        = Color.clear;
            sr.drawMode     = SpriteDrawMode.Sliced;
            sr.size         = Vector2.one * _cellSize * 0.7f;
            sr.sortingOrder = 2;
            _puRenderers[i] = sr;
        }
    }

    // ── Paint helpers ─────────────────────────────────────────────────────────

    private void PaintGrid(NeonBlitzGameState state)
    {
        int W = NeonBlitzConfig.GridWidth;
        int H = NeonBlitzConfig.GridHeight;

        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            int cell = state.GetCell(x, y);
            _cells[x, y].color = cell == 0
                ? _emptyColor
                : s_trailColors[Mathf.Clamp(cell - 1, 0, s_trailColors.Length - 1)];
        }
    }

    private void PaintPowerUps(NeonBlitzGameState state)
    {
        float step    = _cellSize + _cellPadding;
        float W       = NeonBlitzConfig.GridWidth;
        float H       = NeonBlitzConfig.GridHeight;
        float originX = -(W - 1) * step * 0.5f;
        float originY = -(H - 1) * step * 0.5f;

        for (int i = 0; i < NeonBlitzConfig.MaxActivePowerUps; i++)
        {
            var pu = state.powerUps[i];
            var sr = _puRenderers[i];

            if (!pu.active)
            {
                sr.color = Color.clear;
                continue;
            }

            sr.transform.localPosition = new Vector3(
                originX + pu.x * step,
                originY + pu.y * step,
                -0.1f);

            int typeIdx = Mathf.Clamp((int)pu.type, 0, s_puColors.Length - 1);
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 4f);
            sr.color    = s_puColors[typeIdx] * pulse;
        }
    }

    private void PaintHeads(NeonBlitzGameState state)
    {
        float step    = _cellSize + _cellPadding;
        float W       = NeonBlitzConfig.GridWidth;
        float H       = NeonBlitzConfig.GridHeight;
        float originX = -(W - 1) * step * 0.5f;
        float originY = -(H - 1) * step * 0.5f;

        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            var p = state.players[i];
            if (!p.alive) continue;

            // Bright head cell
            _cells[p.headX, p.headY].color =
                s_playerColors[Mathf.Clamp(i, 0, s_playerColors.Length - 1)];

            // Shield halo: slightly brighter neighbouring ring
            if (p.isShielded)
            {
                Color shieldTint = new Color(1f, 1f, 1f, 0.4f);
                PaintIfInBounds(p.headX - 1, p.headY, shieldTint);
                PaintIfInBounds(p.headX + 1, p.headY, shieldTint);
                PaintIfInBounds(p.headX, p.headY - 1, shieldTint);
                PaintIfInBounds(p.headX, p.headY + 1, shieldTint);
            }
        }
    }

    private void PaintIfInBounds(int x, int y, Color c)
    {
        if (x < 0 || x >= NeonBlitzConfig.GridWidth)  return;
        if (y < 0 || y >= NeonBlitzConfig.GridHeight) return;
        _cells[x, y].color = c;
    }

    // ── Screen flash coroutine ────────────────────────────────────────────────

    private IEnumerator DoFlash()
    {
        Color original = _mainCamera.backgroundColor;
        _mainCamera.backgroundColor = _flashColor;
        yield return new WaitForSeconds(0.07f);
        _mainCamera.backgroundColor = original;
    }

    private IEnumerator DoPulse(int x, int y)
    {
        if (x < 0 || x >= NeonBlitzConfig.GridWidth)  yield break;
        if (y < 0 || y >= NeonBlitzConfig.GridHeight) yield break;

        var sr = _cells[x, y];
        Vector3 orig = sr.transform.localScale;
        sr.transform.localScale = orig * 1.5f;
        yield return new WaitForSeconds(0.12f);
        sr.transform.localScale = orig;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private static Sprite CreateWhitePixelSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static Color FromHSV(float h, float s, float v)
    {
        Color c = Color.HSVToRGB(h, s, v);
        c.a = 1f;
        return c;
    }
}

#endif // !UNITY_SERVER
