// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public abstract class FormScreen<TSubmitDelegate> : GameScreen
{
    [SerializeField]
    private GameObject _spinner;

    [SerializeField]
    private Button _submitButton;

    [SerializeField]
    private Text _submitResultText;

    [CanBeNull]
    private TSubmitDelegate _onSubmit;

    protected override void Awake()
    {
        base.Awake();
        _submitResultText.text = string.Empty;
        _submitButton.interactable = false;
    }

    public void SetSubmitAction(TSubmitDelegate onSubmit)
    {
        _onSubmit = onSubmit;
    }

    public void SetResultText(string message)
    {
        _submitResultText.text = message;
    }

    [UsedImplicitly]
    public void Validate()
    {
        _submitButton.interactable = CheckCanSubmit();
    }

    [UsedImplicitly]
    public void Submit()
    {
        if (!CheckCanSubmit() || _onSubmit == null)
        {
            return;
        }

        Submit(_onSubmit);
    }

    internal abstract bool CheckCanSubmit();

    protected abstract void Submit(TSubmitDelegate onSubmit);

    protected override void OnInteractable(bool value)
    {
        base.OnInteractable(value);

        if (_spinner)
        {
            _spinner.SetActive(!value);
        }
    }

    protected override void OnShown()
    {
        base.OnShown();
        Clear();
        Validate();
    }

    protected override void OnHiding()
    {
        base.OnHiding();
        Clear();
    }

    protected virtual void Clear()
    {
        SetResultText(string.Empty);
    }
}
