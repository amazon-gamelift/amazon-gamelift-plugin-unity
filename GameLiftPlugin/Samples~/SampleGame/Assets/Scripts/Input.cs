// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections.Generic;
using UnityEngine;

public class Input
{
    // record what keys are in the current move that we are making
    private readonly Chord _chord = new Chord();
    private readonly GameLogic _gl;

    private readonly Dictionary<KeyCode, int> _keys = new Dictionary<KeyCode, int>
    {
        {KeyCode.Keypad1, 0},
        {KeyCode.Keypad2, 1},
        {KeyCode.Keypad3, 2},
        {KeyCode.Keypad4, 3},
        {KeyCode.Keypad5, 4},
        {KeyCode.Keypad6, 5},
        {KeyCode.Keypad7, 6},
        {KeyCode.Keypad8, 7},
        {KeyCode.Keypad9, 8},
        {KeyCode.N, 0},
        {KeyCode.M, 1},
        {KeyCode.Comma, 2},
        {KeyCode.H, 3},
        {KeyCode.J, 4},
        {KeyCode.K, 5},
        {KeyCode.Y, 6},
        {KeyCode.U, 7},
        {KeyCode.I, 8}
    };

    public Input(GameLogic gl)
    {
        _gl = gl;
    }

    public void Start()
    {
        _chord.Reset();

        for (int keyIdx = 0; keyIdx < 9; keyIdx++)
        {
            _gl.HideHighlight(keyIdx);
        }
    }

    public void Update()
    {
        if (_gl.Playing)
        {
            // quit?
            if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
            {
                _gl.End();
                return;
            }

            // game move
            bool released = true;

            foreach (KeyValuePair<KeyCode, int> kv in _keys)
            {
                if (UnityEngine.Input.GetKey(kv.Key))
                {
                    _chord.Set(kv.Value);
                    _gl.ShowHighlight(kv.Value);
                    released = false;
                }
            }

            if (released)
            {
                if (_chord.IsChanged())
                {
                    // all keys are released, chord is complete
                    _gl.InputEvent(_gl.PlayerIdx, _chord);
                    Start();
                }
            }
        }
        else
        {
            if (UnityEngine.Input.GetKeyUp(KeyCode.Return))
            {
                _gl.Ready();
                return;
            }
        }
    }
}
