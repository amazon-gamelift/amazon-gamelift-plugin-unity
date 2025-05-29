// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public sealed class SignInScreen : FormScreen<SignInScreen.SubmitDelegate>
{
    public delegate void SubmitDelegate(string email, string password);

    [SerializeField]
    private InputField _emailField;

    [SerializeField]
    private InputField _passwordField;

    [SerializeField]
    private Text _hintText;

    [CanBeNull]
    private Action _onShowSignUp;

    [UsedImplicitly]
    public void ShowSignUp()
    {
        _onShowSignUp?.Invoke();
    }

    internal override bool CheckCanSubmit()
    {
        return !string.IsNullOrEmpty(_emailField.text)
            && !string.IsNullOrEmpty(_passwordField.text);
    }

    internal void SetShowSignUpAction(Action onShowSignUp)
    {
        _onShowSignUp = onShowSignUp;
    }

    internal void SetHint(string hint)
    {
        _hintText.text = hint;
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
