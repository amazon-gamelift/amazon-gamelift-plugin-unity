// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;

[System.Serializable]
public class Chord
{
    private bool _chordChanged = false;

    public bool[] Keys = new bool[9];

    public void Reset()
    {
        for (int keyIdx = 0; keyIdx < Keys.Length; keyIdx++)
        {
            Keys[keyIdx] = false;
        }

        _chordChanged = false;
    }

    public void Set(int keyIdx)
    {
        Keys[keyIdx] = true;
        _chordChanged = true;
    }

    public bool IsChanged()
    {
        return _chordChanged;
    }

    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public void Deserialize(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    public static Chord CreateFromSerial(string json)
    {
        var temp = new Chord();
        temp.Deserialize(json);
        return temp;
    }
}
