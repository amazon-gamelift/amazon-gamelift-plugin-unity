// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Render
{
    private readonly GameObject[] _buttons = new GameObject[9];
    private readonly GameObject[] _highlights = new GameObject[9];
    private readonly Material[] _materials = new Material[8];
    private readonly Text _scoreText;
    private readonly Text _statusText;
    private readonly Text _msgText;

    public Render()
    {
        for (int butNum = 1; butNum <= _buttons.Length; butNum++)
        {
            _buttons[butNum - 1] = GameObject.Find("/Button" + butNum); // array index is one less than the keypad number it correlates to
            Debug.Assert(_buttons[butNum - 1] != null); // test our button was found (debug only)
        }

        for (int hlNum = 1; hlNum <= _highlights.Length; hlNum++)
        {
            _highlights[hlNum - 1] = GameObject.Find("/Highlight" + hlNum); // array index is one less than the keypad number it correlates to
            Debug.Assert(_highlights[hlNum - 1] != null); // test our highlight was found (debug only)
        }

        // Materials are not all active so we have to load them
        for (int matNum = 1; matNum <= _materials.Length; matNum++)
        {
            _materials[matNum - 1] = Resources.Load("Materials/Color" + matNum.ToString().PadLeft(3, '0'), typeof(Material)) as Material;
            Debug.Assert(_materials[matNum - 1] != null);
        }

        var score = GameObject.Find("/Canvas/Score");
        _scoreText = score.GetComponent<Text>();
        var status = GameObject.Find("/Canvas/Status");
        _statusText = status.GetComponent<Text>();
        var msg = GameObject.Find("/Canvas/MainMessage");
        _msgText = msg.GetComponent<Text>();
    }

    public void ShowHighlight(int keyNum)
    {
        _highlights[keyNum].SetActive(true);
    }

    public void HideHighlight(int keyNum)
    {
        _highlights[keyNum].SetActive(false);
    }

    public void SetButtonColor(int butNum, int matNum)
    {
        Debug.Assert(butNum < _buttons.Length);
        Debug.Assert(matNum < _materials.Length);
        Renderer rend = _buttons[butNum].GetComponent<Renderer>();
        rend.material = _materials[matNum];
    }

    private void SetScoreText(int[] scores)
    {
        _scoreText.text = "1UP " + (scores[0] < 0 ? "---" : scores[0].ToString().PadLeft(3, '0'));
        _scoreText.text += "      2UP " + (scores[1] < 0 ? "---" : scores[1].ToString().PadLeft(3, '0'));
        _scoreText.text += "      3UP " + (scores[2] < 0 ? "---" : scores[2].ToString().PadLeft(3, '0'));
        _scoreText.text += "      4UP " + (scores[3] < 0 ? "---" : scores[3].ToString().PadLeft(3, '0'));
    }

    public void RenderBoard(Simulation state, Status _)
    {
        for (int bcNum = 0; bcNum < state.BoardColors.Length; bcNum++)
        {
            SetButtonColor(bcNum, state.BoardColors[bcNum]);
        }

        SetScoreText(state.Scores);
    }

    public void SetStatusText(string text)
    {
        _statusText.text = text;
    }

    public void SetMessage(string msg)
    {
        _msgText.text = msg;
    }

    internal IEnumerator ResetMessage(int time)
    {
        string text = _msgText.text;
        yield return new WaitForSeconds(time);

        if (_msgText.text == text)
        {
            SetMessage("");
        }
    }

    internal IEnumerator FlashMessage(int time, int rate)
    {
        string text = _msgText.text;

        for (int i = 0; i < time * rate; i++)
        {
            SetMessage(text);
            yield return new WaitForSeconds(0.5f / rate);

            if (_msgText.text != text)
            {
                break;
            }

            SetMessage("");
            yield return new WaitForSeconds(0.5f / rate);

            if (_msgText.text != "")
            {
                break;
            }
        }
    }
}
