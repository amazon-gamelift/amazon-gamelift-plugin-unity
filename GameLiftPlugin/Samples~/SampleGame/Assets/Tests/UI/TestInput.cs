// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SampleTests.UI
{
    public static class TestInput
    {
        public static string GetText(GameObject text)
        {
            return text.GetComponent<Text>().text;
        }

        public static IEnumerator EnterText(GameObject inputField, string text)
        {
            inputField.GetComponent<InputField>().text = text;
            yield break;
        }

        public static IEnumerator PressButton(GameObject button)
        {
            button.GetComponent<Button>().OnSubmit(null);
            yield return null;
        }
    }
}
