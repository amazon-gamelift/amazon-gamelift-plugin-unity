﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusBox : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StatusBox>
        {
        }

        private Label _statusTextLabel;
        private bool ShowElement { get; set; }

        private const string HiddenClassName = "hidden";
        private const string SuccessBoxElementClass = "status-box--success";
        private const string InfoBoxElementClass = "status-box--info";
        private const string WarningBoxElementClass = "status-box--warning";
        private const string ErrorBoxElementClass = "status-box--error";

        private StatusBoxType _currentType;

        private readonly IReadOnlyDictionary<StatusBoxType, string> _statusBoxClasses =
            new Dictionary<StatusBoxType, string>
            {
                { StatusBoxType.Success, SuccessBoxElementClass },
                { StatusBoxType.Info, InfoBoxElementClass },
                { StatusBoxType.Warning, WarningBoxElementClass },
                { StatusBoxType.Error, ErrorBoxElementClass }
            };

        private ElementLocalizer _elementLocalizer;
        private string _linkURL;
        private readonly Button _externalButton;
        private readonly Label _externalButtonLabel;

        public StatusBox()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusBox");
            asset.CloneTree(this);

            _statusTextLabel = this.Q<Label>("StatusBoxLabel");
            _externalButton = this.Q<Button>("StatusBoxExternalButton");
            _externalButtonLabel = this.Q<Label>("StatusBoxExternalButtonLabel");
            _elementLocalizer = new ElementLocalizer(this);
            this.Q<Button>("StatusBoxCloseButton").RegisterCallback<ClickEvent>(_ => { Close(); });
            _externalButton.RegisterCallback<ClickEvent>(_ => OpenURL());

            UpdateStatusBoxesState();
        }

        private void AddExternalButton(string externalButtonLink, string externalButtonText)
        {
            if (!string.IsNullOrWhiteSpace(externalButtonText))
            {
                _externalButtonLabel.text = _elementLocalizer.GetText(externalButtonText);
                _linkURL = externalButtonLink;
                _externalButton.RemoveFromClassList(HiddenClassName);
            }
        }

        private void OpenURL()
        {
            if (!string.IsNullOrWhiteSpace(_linkURL))
            {
                Application.OpenURL(_linkURL);
            }
        }

        public void Show(StatusBoxType statusBoxType, string text, string additionalText = null,
            string externalButtonLink = null, string externalButtonText = null)
        {
            RemoveFromClassList(_statusBoxClasses[_currentType]);
            AddToClassList(_statusBoxClasses[statusBoxType]);
            _currentType = statusBoxType;

            if (string.IsNullOrWhiteSpace(additionalText))
            {
                _elementLocalizer.SetElementText(_statusTextLabel.name, text);
            }
            else
            {
                _elementLocalizer.SetElementText(_statusTextLabel.name, text, additionalText);
            }

            if (string.IsNullOrWhiteSpace(externalButtonLink))
            {
                _externalButton.AddToClassList(HiddenClassName);
            }
            else
            {
                AddExternalButton(externalButtonLink, externalButtonText);
            }

            ShowElement = true;
            UpdateStatusBoxesState();
        }

        public void Close()
        {
            ShowElement = false;
            UpdateStatusBoxesState();
        }

        private void UpdateStatusBoxesState()
        {
            if (ShowElement)
            {
                RemoveFromClassList(HiddenClassName);
            }
            else
            {
                AddToClassList(HiddenClassName);
            }
        }

        public enum StatusBoxType
        {
            Success,
            Info,
            Warning,
            Error
        }
    }
}