// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;
using UnityEngine.UI;

public sealed class ConfirmationCodeScreen : FormScreen<ConfirmationCodeScreen.SubmitDelegate>
{
    public delegate void SubmitDelegate(string code);

    [SerializeField]
    private InputField _codeField;

    internal override bool CheckCanSubmit()
    {
        return !string.IsNullOrEmpty(_codeField.text);
    }

    protected override void Submit(SubmitDelegate onSubmit)
    {
        onSubmit(_codeField.text);
    }

    protected override void Clear()
    {
        base.Clear();
        _codeField.text = string.Empty;
    }
}
