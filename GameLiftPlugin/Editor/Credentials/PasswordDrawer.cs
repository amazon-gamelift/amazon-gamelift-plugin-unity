// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class PasswordDrawer
    {
        private readonly ControlDrawer _controlDrawer;
        private readonly TextProvider _textProvider;
        private readonly string _labelPasswordShow;
        private readonly string _labelPasswordHide;
        private readonly string _label;
        private readonly string _tooltip;
        private bool _isValueShown;

        public PasswordDrawer(TextProvider textProvider, ControlDrawer controlDrawer, string labelKey, string tooltipKey)
        {
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _controlDrawer = controlDrawer ?? throw new ArgumentNullException(nameof(controlDrawer));
            _labelPasswordShow = _textProvider.Get(Strings.LabelPasswordShow);
            _labelPasswordHide = _textProvider.Get(Strings.LabelPasswordHide);
            _label = _textProvider.Get(labelKey);
            _tooltip = _textProvider.Get(tooltipKey);
        }

        public string Draw(string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string newValue = _isValueShown
                    ? _controlDrawer.DrawTextField(_label, value, _tooltip)
                    : _controlDrawer.DrawPasswordField(_label, value, _tooltip);

                string buttonText = _isValueShown ? _labelPasswordHide : _labelPasswordShow;
                bool needSwitch = GUILayout.Button(buttonText, EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false), GUILayout.Width(45f));

                if (needSwitch)
                {
                    _isValueShown = !_isValueShown;
                    GUI.FocusControl(null);
                }

                return newValue;
            }
        }

        public void Hide()
        {
            _isValueShown = false;
        }
    }
}
