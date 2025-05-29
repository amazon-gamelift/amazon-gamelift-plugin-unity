// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;

// Logic to initialize the board, receive the chords, determine matches, record scores, repopulate
// the board after a match, send the board for rendering, and send or receive the state of the class from the network code.
[System.Serializable]
public class Simulation
{
    private readonly GameLogic _gl;

    public Random.State RngState;
    public int PlayerIdx;
    public int[] BoardColors = new int[9];
    public int[] Scores = new int[4];
    public bool Playing;
    public ulong Frame;

    public Simulation(GameLogic gl)
    {
        _gl = gl;
    }

    public void ResetBoard()
    {
        if (_gl.Authoritative)
        {
            PlayerIdx = 0;

            for (int bcNum = 0; bcNum < BoardColors.Length; bcNum++)
            {
                BoardColors[bcNum] = Random.Range(0, 7);
            }
        }
    }

    public void ResetScores()
    {
        if (_gl.Authoritative)
        {
            for (int scNum = 0; scNum < Scores.Length; scNum++)
            {
                Scores[scNum] = _gl.IsConnected(scNum) ? 0 : -1;
            }
        }
    }

    public void ResetScore(int playerIdx)
    {
        Scores[playerIdx] = -1;
    }

    public void ZeroScore(int playerIdx)
    {
        Scores[playerIdx] = 0;
    }

    public bool SimulateOnInput(int playerIdx, Chord inputChord)
    {
        _gl.Log.WriteLine("SimulateOnInput()");
        Debug.Assert(inputChord.Keys.Length == BoardColors.Length);
        // test for a match
        bool match = false;
        int matchColor = -1; // don't know yet

        for (int bcNum = 0; bcNum < BoardColors.Length; bcNum++)
        {
            if (inputChord.Keys[bcNum])
            {
                if (matchColor == -1)
                {
                    matchColor = BoardColors[bcNum];
                }
                else
                {
                    if (BoardColors[bcNum] == matchColor)
                    {
                        match = true;
                    }
                    else
                    {
                        match = false;
                        break;
                    }
                }
            }
        }

        if (match)
        {
            // yes, a match!
            for (int bcNum = 0; bcNum < BoardColors.Length; bcNum++)
            {
                if (inputChord.Keys[bcNum])
                {
                    BoardColors[bcNum] = Random.Range(0, 7);
                    Scores[playerIdx]++;
                }
            }
        }

        return match;
    }

    public string Serialize(int playerIdx)
    {
        PlayerIdx = playerIdx;
        RngState = Random.state;
        return JsonUtility.ToJson(this);
    }

    public bool Deserialize(string json)
    {
        bool priorPlayingState = Playing;

        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
            Random.state = RngState;
            _gl.Status.PlayerIdx = PlayerIdx;
        }

        return priorPlayingState;
    }
}
