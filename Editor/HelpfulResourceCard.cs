// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class HelpfulResourceCard : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<HelpfulResourceCard> { }

        private bool ShowElement { get; set; }

        private const string HiddenClassName = "hidden";
        private const string DocumentationClassName = "resource--documentation";
        private const string GuidanceClassName = "resource--awsguidance";
        private const string WhitepaperClassName = "resource--whitepaper";

        private const string DocumentationText = "Documentation";
        private const string GuidanceText = "AWS Guidance";
        private const string WhitePaperText = "Whitepaper";


        private ResourceType _currentType;

        private readonly IReadOnlyDictionary<ResourceType, string> _resourceTypeToClass =
            new Dictionary<ResourceType, string>
            {
                { ResourceType.Documentation, DocumentationClassName },
                { ResourceType.Guidance, GuidanceClassName },
                { ResourceType.Whitepaper, WhitepaperClassName },
            };

        private readonly IReadOnlyDictionary<ResourceType, string> _resourceTypeToText =
            new Dictionary<ResourceType, string>
            {
                { ResourceType.Documentation, DocumentationText },
                { ResourceType.Guidance, GuidanceText },
                { ResourceType.Whitepaper, WhitePaperText },
            };

        private ElementLocalizer _elementLocalizer;
        private readonly Label _tagOuterLabel;
        private readonly Label _tagInnerLabel;
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private string _link;

        public HelpfulResourceCard()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/HelpfulResourceCard");
            asset.CloneTree(this);

            _tagOuterLabel = this.Q<Label>("HelpfulResourceTag");
            _tagInnerLabel = this.Q<Label>("HelpfulResourceTagText");
            _titleLabel = this.Q<Label>("HelpfulResourceTitle");
            _descriptionLabel = this.Q<Label>("HelpfulResourceDescription");
            _elementLocalizer = new ElementLocalizer(this);
            this.RegisterCallback<ClickEvent>(_ => OpenURL());
        }

        private void OpenURL()
        {
            if (!string.IsNullOrWhiteSpace(_link))
            {
                Application.OpenURL(_link);
            }
        }

        public void UpdateCard(ResourceType ResourceType, string title, string description = null, string link = null)
        {
            UpdateResource(ResourceType);
            _titleLabel.text = _elementLocalizer.GetText(title);
            if (!string.IsNullOrWhiteSpace(description))
            {
                _elementLocalizer.SetElementText(_descriptionLabel.name, description);
            }

            AddLink(link);
        }

        private void UpdateResource(ResourceType resourceType)
        {
            _tagOuterLabel.RemoveFromClassList(_resourceTypeToClass[_currentType]);
            _tagOuterLabel.AddToClassList(_resourceTypeToClass[resourceType]);
            _elementLocalizer.SetElementText(_tagInnerLabel.name, _resourceTypeToText[resourceType]);
            _currentType = resourceType;
        }

        private void AddLink(string link)
        {
            if (!string.IsNullOrWhiteSpace(link))
            {
                _link = link;
            }
        }

        public enum ResourceType
        {
            Documentation,
            Guidance,
            Whitepaper,
        }
    }
}