// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using UnityEngine;

/// <summary>
/// Translates mobile swipe gestures (and WASD / arrow keys for desktop testing)
/// into <see cref="NeonDirection"/> events.
///
/// Subscribe to <see cref="OnSwipe"/> to receive direction changes.
/// Attach this component to any persistent GameObject.
/// </summary>
public class TouchSwipeInput : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Tooltip("Minimum swipe distance in pixels before the gesture is recognised.")]
    [SerializeField] private float _minSwipeDistance = 40f;

    [Tooltip("Maximum elapsed time (seconds) for a valid swipe.")]
    [SerializeField] private float _maxSwipeTime = 0.4f;

    // ── Events ─────────────────────────────────────────────────────────────────
    public event System.Action<NeonDirection> OnSwipe;

    // ── Private state ──────────────────────────────────────────────────────────
    private Vector2 _touchStart;
    private float   _touchStartTime;
    private bool    _tracking;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Update()
    {
        HandleKeyboard();
        HandleTouch();
    }

    // ── Keyboard (desktop / dev) ───────────────────────────────────────────────

    private void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W))
            Emit(NeonDirection.Up);
        else if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S))
            Emit(NeonDirection.Down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A))
            Emit(NeonDirection.Left);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Emit(NeonDirection.Right);
    }

    // ── Touch ──────────────────────────────────────────────────────────────────

    private void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _touchStart     = touch.position;
                _touchStartTime = Time.unscaledTime;
                _tracking       = true;
                break;

            case TouchPhase.Ended when _tracking:
                _tracking = false;
                float elapsed = Time.unscaledTime - _touchStartTime;
                if (elapsed > _maxSwipeTime) break;

                Vector2 delta = touch.position - _touchStart;
                if (delta.magnitude < _minSwipeDistance) break;

                // Dominant axis determines direction
                if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                    Emit(delta.x > 0 ? NeonDirection.Right : NeonDirection.Left);
                else
                    Emit(delta.y > 0 ? NeonDirection.Up : NeonDirection.Down);
                break;

            case TouchPhase.Cancelled:
                _tracking = false;
                break;
        }
    }

    private void Emit(NeonDirection dir) => OnSwipe?.Invoke(dir);
}

#endif // !UNITY_SERVER
