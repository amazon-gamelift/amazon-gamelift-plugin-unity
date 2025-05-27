// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class HelpAndDocumentationPage
    {
        private readonly VisualElement _container;
        private readonly HelpfulResources _helpfulResources;

        public HelpAndDocumentationPage(VisualElement container)
        {
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/HelpAndDocumentationPage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            ApplyText();

            var helpfulResourcesContainer = _container.Q<Foldout>("HelpfulResourcesFoldout");
            _helpfulResources = new HelpfulResources(helpfulResourcesContainer);
            helpfulResourcesContainer.value = false;

            _container.Q<VisualElement>("HelpPageEstimatingPriceLinkParent").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.GameLiftPricingCalculator));
            _container.Q<VisualElement>("HelpPageFlexMatchLinkParent").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsFlexMatchDocumentation));
        }

        private void ApplyText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("HelpPageTitle", Strings.HelpPageTitle);
            l.SetElementText("HelpPageDescription", Strings.HelpPageDescription);
            l.SetElementText("HelpPageEstimatingPriceTitle", Strings.HelpPageEstimatingPriceTitle);
            l.SetElementText("HelpPageEstimatingPriceDescription", Strings.HelpPageEstimatingPriceDescription);
            l.SetElementText("HelpPageEstimatingPriceLink", Strings.HelpPageEstimatingPriceLink);
            l.SetElementText("HelpPageFlexMatchTitle", Strings.HelpPageFlexMatchTitle);
            l.SetElementText("HelpPageFlexMatchDescription", Strings.HelpPageFlexMatchDescription);
            l.SetElementText("HelpPageFlexMatchLink", Strings.HelpPageFlexMatchLink);
        }
    }
}