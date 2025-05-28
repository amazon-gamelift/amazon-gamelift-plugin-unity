// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public abstract class GameScreen : UIBehaviour
{
    [SerializeField]
    private CanvasGroup _rootCanvasGroup;

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }

    public void Show()
    {
        if (!this)
        {
            return;
        }

        gameObject.SetActive(true);
        SetInteractable(true);
        OnShown();
    }

    public void Hide()
    {
        if (!this)
        {
            return;
        }

        OnHiding();
        gameObject.SetActive(false);
    }

    public void SetInteractable(bool value)
    {
        if (!this)
        {
            return;
        }

        _rootCanvasGroup.interactable = value;
        OnInteractable(value);
    }

    protected virtual void OnInteractable(bool value) { }

    protected virtual void OnShown() { }

    protected virtual void OnHiding() { }
}
