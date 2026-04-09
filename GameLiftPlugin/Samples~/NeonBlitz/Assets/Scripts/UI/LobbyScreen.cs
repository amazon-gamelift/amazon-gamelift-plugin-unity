// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pre-match lobby screen.
///
/// Shows connected player slots, a READY button, and error feedback.
/// The manager calls <see cref="SetPlayerSlots"/> whenever a player joins/leaves,
/// and <see cref="SetErrorText"/> when connection fails.
/// </summary>
public class LobbyScreen : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Player slot indicators (0..MaxPlayers-1)")]
    [SerializeField] private Image[]  _slotIcons      = new Image[NeonBlitzConfig.MaxPlayers];
    [SerializeField] private Text[]   _slotLabels     = new Text[NeonBlitzConfig.MaxPlayers];

    [Header("Controls")]
    [SerializeField] private Button   _readyButton;
    [SerializeField] private Text     _readyButtonText;
    [SerializeField] private Text     _statusText;
    [SerializeField] private InputField _nameField;

    // ── Slot colours ──────────────────────────────────────────────────────────
    private static readonly Color s_empty    = new Color(0.15f, 0.15f, 0.15f, 0.5f);
    private static readonly Color[] s_filled =
    {
        Color.HSVToRGB(0.50f, 1f, 1f),
        Color.HSVToRGB(0.83f, 1f, 1f),
        Color.HSVToRGB(0.28f, 1f, 0.9f),
        Color.HSVToRGB(0.08f, 1f, 1f),
    };

    // ── Events ────────────────────────────────────────────────────────────────
    public event Action<string> OnReadyPressed;   // string = chosen player name

    // ── State ─────────────────────────────────────────────────────────────────
    private bool _readyLocked;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _readyButton?.onClick.AddListener(HandleReadyPressed);
        Hide();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()    => gameObject.SetActive(true);
    public void Hide()    => gameObject.SetActive(false);

    public void SetPlayerSlots(bool[] connected, string[] names)
    {
        for (int i = 0; i < NeonBlitzConfig.MaxPlayers; i++)
        {
            bool isConnected = i < connected.Length && connected[i];

            if (_slotIcons[i] != null)
                _slotIcons[i].color = isConnected ? s_filled[i] : s_empty;

            if (_slotLabels[i] != null)
            {
                _slotLabels[i].text  = isConnected
                    ? (names != null && i < names.Length ? names[i] : $"Player {i + 1}")
                    : "OPEN";
                _slotLabels[i].color = isConnected ? s_filled[i] : Color.grey;
            }
        }
    }

    public void SetStatusText(string msg, Color? color = null)
    {
        if (_statusText == null) return;
        _statusText.text  = msg;
        _statusText.color = color ?? Color.white;
    }

    public void SetReadyInteractable(bool interactable)
    {
        if (_readyButton != null)
            _readyButton.interactable = interactable && !_readyLocked;
    }

    public void LockReady()
    {
        _readyLocked = true;
        if (_readyButtonText != null) _readyButtonText.text = "WAITING...";
        if (_readyButton      != null) _readyButton.interactable = false;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void HandleReadyPressed()
    {
        LockReady();
        string chosenName = (_nameField != null && !string.IsNullOrEmpty(_nameField.text))
            ? _nameField.text
            : $"Player {SystemInfo.deviceName}";
        OnReadyPressed?.Invoke(chosenName);
    }
}

#endif // !UNITY_SERVER
