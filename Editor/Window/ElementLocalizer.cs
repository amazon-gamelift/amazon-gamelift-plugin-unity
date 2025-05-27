// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ElementLocalizer
    {
        private readonly VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        public ElementLocalizer(VisualElement root)
        {
            _root = root;
        }

        public string GetText(string textKey)
        {
            return _textProvider.Get(textKey);
        }

        public string GetError(string errorCode)
        {
            return _textProvider.GetError(errorCode);
        }

        public void SetElementText(string elementName, string textKey)
        {
            var text = _textProvider.Get(textKey);
            SetText(elementName, text);
        }
        
        // The status box use case where you would want to display an Exception message in addition to the generic error text.
        public void SetElementText(string elementName, string textKey, string additionalTextKey)
        {
            var text = _textProvider.Get(textKey);
            var additionalText = _textProvider.Get(additionalTextKey);
            SetText(elementName, string.Format("{0}: {1}", text, additionalText));
        }

        public void SetElementText(string elementName, string textKey, Dictionary<string, string> wordReplacements)
        {
            var text = wordReplacements.Aggregate(_textProvider.Get(textKey), (result, next) => result.Replace($"[{next.Key}]", next.Value));
            SetText(elementName, text);
        }

        public void SetElementTooltip(string elementName, string textKey)
        {
            var text = _textProvider.Get(textKey);
            var element = _root.Q<TextElement>(elementName);
            if (element != null)
            {
                element.tooltip = text;
            }
        }

        private void SetText(string elementName, string text)
        {
            var element = _root.Q<TextElement>(elementName);
            if (element != null)
            {
                element.text = text;
            }
            else
            {
                var foldout = _root.Q<Foldout>(elementName);
                if (foldout != null)
                {
                    foldout.text = text;
                }
            }
        }
    }
}