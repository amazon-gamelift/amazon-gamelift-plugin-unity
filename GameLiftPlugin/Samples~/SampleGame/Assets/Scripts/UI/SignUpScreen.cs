// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;
using UnityEngine.UI;

public sealed class SignUpScreen : FormScreen<SignUpScreen.SubmitDelegate>
{
    public delegate void SubmitDelegate(string email, string password);

    [SerializeField]
    private InputField _emailField;

    [SerializeField]
    private InputField _passwordField;

    [SerializeField]
    private int _minPasswordLength = 8;

    internal override bool CheckCanSubmit()
    {
        return !string.IsNullOrEmpty(_emailField.text)
            && !string.IsNullOrEmpty(_passwordField.text)
            && _passwordField.text.Length >= _minPasswordLength;
    }

    protected override void Submit(SubmitDelegate onSubmit)
    {
        onSubmit(_emailField.text, _passwordField.text);
    }

    protected override void Clear()
    {
        base.Clear();
        _emailField.text = string.Empty;
        _passwordField.text = string.Empty;
    }
}
