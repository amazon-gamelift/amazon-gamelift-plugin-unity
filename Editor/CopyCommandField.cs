// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class CopyCommandField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<CopyCommandField> { }

        private Label _textLabel;
        private string _text;

        private ElementLocalizer _elementLocalizer;

        public CopyCommandField()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/CopyCommandField");
            asset.CloneTree(this);

            _textLabel = this.Q<Label>("CommandTextLabel");
            
            _elementLocalizer = new ElementLocalizer(this);

            Image copyIcon = this.Q<Image>("CopyIcon");

            copyIcon.RegisterCallback<ClickEvent>(_ => CopyToClipboard());
            copyIcon.RegisterCallback<MouseDownEvent>(_ => copyIcon.AddToClassList("mouse-down"));
            copyIcon.RegisterCallback<MouseUpEvent>(_ => copyIcon.RemoveFromClassList("mouse-down"));
        }

        private void CopyToClipboard()
        {
            TextEditor te = new TextEditor();
            te.text = _text;
            te.SelectAll();
            te.Copy();
        }

        public void UpdateText(string text)
        {
            _text = text;
            _elementLocalizer.SetElementText(_textLabel.name, text);
        }
    }
}