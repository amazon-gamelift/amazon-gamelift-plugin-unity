// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Draws a scrollable list of items and a filter text field.
    /// </summary>
    [Serializable]
    public sealed class TextFilter
    {
        private readonly float _selectionHeightPixels;
        private readonly string _defaultText;
        private string _currentText = "";
        private string _confirmedText = "";
        private int _currentIndex = 0;
        private int _confirmedIndex = -1;
        private List<string> _options = new List<string>();
        private string[] _filteredOptions;
        private GUIStyle _normalStyle;
        private GUIStyle _selectedStyle;
        private Vector2 _scrollPosition;
        private Boolean _areOptionsUpdated = false;

        public string CurrentText
        {
            get => _currentText;
            set => _currentText = value;
        }

        public string ConfirmedOption =>
          _confirmedIndex >= 0 && _confirmedIndex < _filteredOptions.Length
            ? _filteredOptions[_confirmedIndex]
            : _defaultText;

        /// <summary>
        /// Don't call this from Unity field initializers.
        /// </summary>
        public TextFilter(string defaultText, float selectionHeightPixels)
        {
            _defaultText = defaultText;
            _selectionHeightPixels = selectionHeightPixels;
        }

        public void SetOptions(IReadOnlyList<string> value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _options = value.ToList();
            _currentText = value.Count == 0 ? _defaultText : string.Empty;
            _areOptionsUpdated = true;
        }

        public void Draw()
        {
            if (_normalStyle == null)
            { // If is not set up
                SetUp();
            }

            EditorGUILayout.BeginVertical();
            {
                _currentText = EditorGUILayout.TextField(_currentText);

                if (_areOptionsUpdated)
                {
                    _filteredOptions = _options.ToArray();
                    _areOptionsUpdated = false;
                }

                if (_currentText != _confirmedText)
                {
                    _confirmedText = _currentText;
                    _confirmedIndex = _currentIndex = -1;
                    _filteredOptions = _options.Where(option => option.Contains(_currentText)).ToArray();
                }

                if (_filteredOptions == null)
                {
                    _filteredOptions = _options.ToArray();
                }

                _currentIndex = DrawSelection(_currentIndex, _filteredOptions, _selectionHeightPixels);

                if (_currentIndex != _confirmedIndex)
                {
                    _confirmedIndex = _currentIndex;
                    _confirmedText = ConfirmedOption;
                    _currentText = _confirmedText;
                }
            }
            EditorGUILayout.EndVertical();
        }

        private int DrawSelection(int currentOption, string[] filteredOptions, float height)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, _normalStyle, GUILayout.Height(height));

            for (int i = 0; i < filteredOptions.Length; i++)
            {
                if (GUILayout.Button(filteredOptions[i], currentOption == i ? _selectedStyle : _normalStyle))
                {
                    currentOption = i;
                }
            }

            EditorGUILayout.EndScrollView();
            return currentOption;
        }

        private void SetUp()
        {
            _normalStyle = ResourceUtility.GetTextFilterNormalStyle();
            _selectedStyle = ResourceUtility.GetTextFilterSelectedStyle();
        }
    }
}
