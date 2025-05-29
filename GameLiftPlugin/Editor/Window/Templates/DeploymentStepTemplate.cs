// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;
using System;
using Castle.Core.Internal;

namespace AmazonGameLift.Editor
{
    /**
     * Wrapper of points of interest in a deployment step based on the 'DeploymentStep.uxml'
     * Recommendation is to use the Builder and the uxml template together.
     */
    public class DeploymentStepTemplate : StatefulInput
    {
        public const string BaseButtonProceed = "ProceedButton";
        public const string BaseButtonTryAgain = "TryAgainButton";
        public const string BaseButtonViewLogs = "ViewLogButton";

        private const string LinkUxmlTemplate = "EditorWindow/Templates/DeploymentStepHelpLink";
        private const string DividerClassName = "divider--vertical";

        public StatusBox StatusBox { get; private set; }
        /**
         * Container that stores the template buttons if the feature was enabled by the Builder.
         * If neither 'WithBaseButtons' or 'WithoutBaseButtons' was used, this will be 'null'.
         */
        public VisualElement ButtonContainer { get; private set; }
        /**
         * Container that stores the child content of the template.
         */
        public VisualElement ContentContainer { get; private set; }

        private VisualElement _container;

        internal DeploymentStepTemplate(StatusBox statusBox, VisualElement buttonContainer, VisualElement contentContainer, VisualElement container)
        {
            StatusBox = statusBox;
            ButtonContainer = buttonContainer;
            ContentContainer = contentContainer;
            _container = container;
        }

        public void UpdateTitle(string titleKey)
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("StepTitle", titleKey);
        }

        private static void InitializeHelpLinks(VisualElement linkCollection, DeploymentStepTemplateLink[] links)
        {
            if (links.Length == 0)
            {
                Hide(linkCollection);
                return;
            }

            linkCollection.Clear();
            Show(linkCollection);

            var linkTemplate = Resources.Load<VisualTreeAsset>(LinkUxmlTemplate);

            bool first = true;
            foreach (var link in links)
            {
                if (!first)
                {
                    var divider = new VisualElement();
                    divider.AddToClassList(DividerClassName);
                    linkCollection.Add(divider);
                }

                var linkUxml = linkTemplate.Instantiate();
                var l = new ElementLocalizer(linkUxml);

                linkUxml.RegisterCallback<ClickEvent>(_ => Application.OpenURL(link._linkUrl));
                l.SetElementText("LinkLabel", link._linkLabelKey);

                linkCollection.Add(linkUxml);
                first = false;
            }
        }

        private static T TryGetElement<T>(VisualElement container, string elementName, string feature = "This feature") where T : VisualElement
        {
            var element = container.Q<T>(elementName);
            if (element == null)
            {
                throw new InvalidOperationException($"{feature} is not supported because could not find '{elementName}' element.");
            }
            return element;
        }

        private static void EnsureClear(VisualElement container, string elementName)
        {
            var element = container.Q<VisualElement>(elementName);
            if (element != null)
            {
                Hide(element);
                element.Clear();
            }
        }

        protected override void UpdateGUI()
        {
            throw new System.NotImplementedException();
        }

        /**
         * Provides integration for the features setup in the 'DeploymentStep.uxml' template file.
         */
        public class Builder
        {
            private const string HelpLinksFeature = "Help links template feature";
            private const string ButtonsFeature = "Action buttons template feature";

            private const string ElementNameLinkCollection = "LinkCollection";
            private const string ElementNameButtonCollection = "ButtonCollection";
            private const string ElementNameStepContent = "StepContent";

            private string _titleKey;
            private string _descriptionKey;
            private bool _hasContent = true;
            private DeploymentStepTemplateLink[] _helpLinks;
            private ButtonUsage _buttonUsage = ButtonUsage.Unsupported;

            private enum ButtonUsage
            {
                WithBaseButtons,
                WithNoButtons,
                Unsupported,
            }

            public Builder(string titleKey, string descriptionKey)
            {
                _titleKey = titleKey;
                _descriptionKey = descriptionKey;
            }

            /**
             * Hides the content container in the template for use cases where no additional content is needed.
             * Without this, there will be extra spacing as the VisualElement will be visible but empty.
             * Note: This clears out the content container as well as hiding it.
             */
            public Builder WithNoContent()
            {
                _hasContent = false;
                return this;
            }

            /**
             * Enables support for the template's button container and leaves it populated with some commonly
             * used buttons: Proceed, Try again, View logs.
             * Note: These buttons will be visible initially.
             * Note: Conflicts with 'WithoutBaseButtons'. Whichever is called last takes precendence.
             */
            public Builder WithBaseButtons()
            {
                _buttonUsage = ButtonUsage.WithBaseButtons;
                return this;
            }

            /**
             * Enables support for the template's button container, but DOES NOT populate with any buttons.
             * Use this when the base buttons aren't needed, but some custom buttons will be.
             * Note: This also hides the button container so that it doesn't introduce additional spacing.
             * Note: Conflicts with 'WithBaseButtons'. Whichever is called last takes precendence.
             */
            public Builder WithoutBaseButtons()
            {
                _buttonUsage = ButtonUsage.WithNoButtons;
                return this;
            }

            /**
             * Populates the help link section in the template. The links are inserted in the order provided.
             */
            public Builder WithHelpLinks(params DeploymentStepTemplateLink[] links)
            {
                _helpLinks = links;
                return this;
            }

            /**
             * Builds the current set of features into the provided container and provides a wrapper with
             * properties to access useful pieces of that container.
             * Throws exceptions when features are enabled but their containers are missing.
             */
            public DeploymentStepTemplate Build(VisualElement container)
            {
                if (!_hasContent)
                {
                    EnsureClear(container, ElementNameStepContent);
                }
                LocalizeText(container);
                BuildLinks(container);
                var buttonContainer = PrepareButtonContainer(container);
                var statusBox = container.Q<StatusBox>();
                var contentContainer = container.Q<VisualElement>(ElementNameStepContent);

                return new DeploymentStepTemplate(statusBox, buttonContainer, contentContainer, container);
            }

            private void LocalizeText(VisualElement container)
            {
                var l = new ElementLocalizer(container);
                l.SetElementText("StepTitle", _titleKey);
                if (!_descriptionKey.IsNullOrEmpty())
                {
                    l.SetElementText("StepDescription", _descriptionKey);
                }
                else
                {
                    Hide(container.Q("StepDescription"));
                }
            }

            private void BuildLinks(VisualElement container)
            {
                if (_helpLinks == null || _helpLinks.Length == 0)
                {
                    EnsureClear(container, ElementNameLinkCollection);
                    return;
                }

                var linkCollection = TryGetElement<VisualElement>(container, ElementNameLinkCollection, HelpLinksFeature);
                InitializeHelpLinks(linkCollection, _helpLinks);
            }

            private VisualElement PrepareButtonContainer(VisualElement container)
            {
                if (_buttonUsage == ButtonUsage.Unsupported)
                {
                    return null;
                }

                if (_buttonUsage == ButtonUsage.WithNoButtons)
                {
                    EnsureClear(container, ElementNameButtonCollection);
                }

                return TryGetElement<VisualElement>(container, ElementNameButtonCollection, ButtonsFeature);
            }
        }
    }

    public struct DeploymentStepTemplateLink
    {
        internal string _linkUrl;
        internal string _linkLabelKey;

        public DeploymentStepTemplateLink(string linkUrl, string linkLabelKey)
        {
            _linkUrl = linkUrl;
            _linkLabelKey = linkLabelKey;
        }
    }
}
