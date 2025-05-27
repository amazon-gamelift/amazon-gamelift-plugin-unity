// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AmazonGameLift.Runtime;

namespace AmazonGameLift.Editor
{
    internal class HelpfulResources : VisualElement
    {
        private bool _enabled;
        private readonly VisualElement _container;
        private readonly HelpfulResourceCard _setupAwsCard;
        private readonly HelpfulResourceCard _pluginGuideCard;
        private readonly HelpfulResourceCard _gettingStartedCard;
        private readonly HelpfulResourceCard _organizingAwsEnvironmentCard;
        private readonly HelpfulResourceCard _getAccessKeysCard;
        private readonly HelpfulResourceCard _serviceLocationsCard;
        private readonly HelpfulResourceCard _manageAccessKeysCard;
        private readonly HelpfulResourceCard _hostingSolutionsCard;
        private readonly HelpfulResourceCard _hostingFleetCard;

        public HelpfulResources(VisualElement container)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/HelpfulResources");
            container.Add(uxml.Instantiate());

            _container = container;
            _setupAwsCard = _container.Q<HelpfulResourceCard>("SetupAWSCard");
            _pluginGuideCard = _container.Q<HelpfulResourceCard>("PluginGuideCard");
            _gettingStartedCard = _container.Q<HelpfulResourceCard>("GettingStartedCard");
            _organizingAwsEnvironmentCard = _container.Q<HelpfulResourceCard>("OrganizingEnvCard");
            _getAccessKeysCard = _container.Q<HelpfulResourceCard>("GetAccessKeysCard");
            _serviceLocationsCard = _container.Q<HelpfulResourceCard>("ServiceLocationsCard");
            _manageAccessKeysCard = _container.Q<HelpfulResourceCard>("ManageAccessKeysCard");
            _hostingSolutionsCard = _container.Q<HelpfulResourceCard>("HostingSolutionsCard");
            _hostingFleetCard = _container.Q<HelpfulResourceCard>("HostingFleetCard");

            LocalizeText();
            PopulateContent();
        }

        public void PopulateContent()
        {
            _setupAwsCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceSetupAccountTitle,
                Strings.HelpfulResourceSetupAccountDescription, Urls.SetupAwsAccount);
            _pluginGuideCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourcePluginGuideTitle,
                Strings.HelpfulResourcePluginGuideDescription, Urls.PluginGuideCreateProfile);
            _gettingStartedCard.UpdateCard(HelpfulResourceCard.ResourceType.Guidance, Strings.HelpfulResourceGettingStartedTitle,
                link: Urls.GettingStarted);
            _organizingAwsEnvironmentCard.UpdateCard(HelpfulResourceCard.ResourceType.Whitepaper, Strings.HelpfulResourceOrganizingEnvTitle,
                link: Urls.OrganizingEnv);
            _getAccessKeysCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceGetAccessKeysTitle,
                Strings.HelpfulResourceGetAccessKeysDescription, Urls.GetAccessKeys);
            _serviceLocationsCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceServiceLocationsTitle,
                Strings.HelpfulResourceSetupAccountDescription, Urls.ServiceLocations);
            _manageAccessKeysCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceManageAccessKeysTitle,
                Strings.HelpfulResourceManageAccessKeysDescription, Urls.ManageAccessKeys);
            _hostingSolutionsCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceHostingSolutionsTitle,
                Strings.HelpfulResourceHostingSolutionsDescription, Urls.HostingSolutions);
            _hostingFleetCard.UpdateCard(HelpfulResourceCard.ResourceType.Documentation, Strings.HelpfulResourceHostingFleetTitle,
                Strings.HelpfulResourceHostingFleetDescription, Urls.HostingFleet);
        }


        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("HelpfulResourceSetupAccountTitle", Strings.HelpfulResourceSetupAccountTitle);
            l.SetElementText("HelpfulResourceSetupAccountDescription", Strings.HelpfulResourceSetupAccountDescription);
            l.SetElementText("HelpfulResourcePluginGuideTitle", Strings.HelpfulResourcePluginGuideTitle);
            l.SetElementText("HelpfulResourcePluginGuideDescription", Strings.HelpfulResourcePluginGuideDescription);
            l.SetElementText("HelpfulResourceGettingStartedTitle", Strings.HelpfulResourceGettingStartedTitle);
            l.SetElementText("HelpfulResourceOrganizingEnvTitle", Strings.HelpfulResourceOrganizingEnvTitle);
            l.SetElementText("HelpfulResourceGetAccessKeysTitle", Strings.HelpfulResourceGetAccessKeysTitle);
            l.SetElementText("HelpfulResourceGetAccessKeysDescription", Strings.HelpfulResourceGetAccessKeysDescription);
            l.SetElementText("HelpfulResourceServiceLocationsTitle", Strings.HelpfulResourceServiceLocationsTitle);
            l.SetElementText("HelpfulResourceServiceLocationsDescription", Strings.HelpfulResourceServiceLocationsDescription);
            l.SetElementText("HelpfulResourceHostingSolutionsTitle", Strings.HelpfulResourceHostingSolutionsTitle);
            l.SetElementText("HelpfulResourceHostingSolutionsDescription", Strings.HelpfulResourceHostingSolutionsDescription);
            l.SetElementText("HelpfulResourceHostingFleetTitle", Strings.HelpfulResourceHostingFleetTitle);
            l.SetElementText("HelpfulResourceHostingFleetDescription", Strings.HelpfulResourceHostingFleetDescription);
        }
    }
}
