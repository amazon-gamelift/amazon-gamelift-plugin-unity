// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal abstract class Dialog : EditorWindow
    {
        private const float TopMarginPixels = 10f;
        private const float LeftMarginPixels = 10f;
        private const float RightMarginPixels = 10f;

        private readonly List<DialogAction> _actions = new List<DialogAction>();
        private float _cachedContentsHeightPixels;

        protected TextProvider TextProvider { get; private set; }

        protected float WindowHeightNoContentsPixels { get; } = 38f;

        protected abstract bool IsModal { get; }

        protected abstract float WindowWidthPixels { get; }

        protected abstract string TitleKey { get; }

        protected Action DefaultAction { get; set; }

        public void AddAction(DialogAction action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _actions.Add(action);
        }

        protected abstract void SetUpContents();

        protected abstract Rect DrawContents();

        private void OnEnable()
        {
            _actions.Clear();
            TextProvider = TextProviderFactory.Create();
            titleContent = new GUIContent(TextProvider.Get(TitleKey));
            SetWindowSize(WindowHeightNoContentsPixels);
            SetUpContents();
            ShowUtility();
        }

        private void OnDisable()
        {
            DefaultAction?.Invoke();
            DefaultAction = null;
            _actions.Clear();
        }

        private void OnGUI()
        {
            if (IsModal && focusedWindow != this)
            {
                FocusWindowIfItsOpen(GetType());
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(LeftMarginPixels);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(TopMarginPixels);
                    DrawControls();
                }

                GUILayout.Space(RightMarginPixels);
            }
        }

        private void DrawControls()
        {
            Rect contentsRect = DrawContents();

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (DialogAction item in _actions)
                {
                    if (GUILayout.Button(TextProvider.Get(item.LabelKey)))
                    {
                        item.Action();
                        Close();
                        break;
                    }
                }
            }

            if (Event.current.type == EventType.Repaint && _cachedContentsHeightPixels != contentsRect.height)
            {
                _cachedContentsHeightPixels = contentsRect.height;
                SetWindowSize(WindowHeightNoContentsPixels + _cachedContentsHeightPixels);
            }
        }

        private void SetWindowSize(float height)
        {
            this.SetConstantSize(new Vector2(x: WindowWidthPixels, y: height));
        }
    }
}
