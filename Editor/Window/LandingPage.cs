// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class LandingPage
    {
        private readonly VisualElement _container;
        private readonly HelpfulResources _helpfulResources;
        private StatusBox _statusBox;
        private StateManager _stateManager;


        public LandingPage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/LandingPage");
            var uxml = mVisualTreeAsset.Instantiate();
            container.Add(uxml);
            SetupStatusBoxes();
            LocalizeText();
            _stateManager = stateManager;

            var helpfulResourcesContainer = _container.Q<Foldout>("HelpfulResourcesFoldout");
            _helpfulResources = new HelpfulResources(helpfulResourcesContainer);
            helpfulResourcesContainer.value = false;

            UpdateGui();
            
            _stateManager.OnUserProfileUpdated += UpdateGui;
            
            _container.Q<Button>("CreateAccount").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.CreateAwsAccountLearnMore));
            _container.Q<Button>("LandingPageAccountCardButton").RegisterCallback<ClickEvent>(_ => OnAddProfileClicked());
            _container.Q<Button>("LandingPageAnywhereButton").RegisterCallback<ClickEvent>(_ => OnAnywhereButtonClicked());
            _container.Q<Button>("LandingPageManagedButton").RegisterCallback<ClickEvent>(_ => OnManagedButtonClicked());
            _container.Q<Button>("LandingPageContainerButton").RegisterCallback<ClickEvent>(_ => OnContainerButtonClicked());
            _container.Q<Button>("ManageCredentialsButton").RegisterCallback<ClickEvent>(_ => OnCredentialsButtonClicked());
        }

        private static void OnAddProfileClicked()
        {
            EditorMenu.OpenAccountProfilesTab();
        }

        private static void OnAnywhereButtonClicked()
        {
            EditorMenu.OpenAnywhereTab();
        }

        private static void OnManagedButtonClicked()
        {
            EditorMenu.OpenEC2Tab();
        }

        private static void OnContainerButtonClicked()
        {
            EditorMenu.OpenContainersTab();
        }

        private static void OnCredentialsButtonClicked()
        {
            EditorMenu.OpenAccountProfilesTab();
        }

        private static void OnImportSampleClicked()
        {
            EditorMenu.ImportSampleGame();
        }
        
        private void UpdateGui()
        {
            VisualElement _accountLandingPage = _container.Q<VisualElement>("LandingPageAccount");
            ProfileTable _profileTable = _container.Q<ProfileTable>("ProfileTable");
            VisualElement _profileTableDivider = _container.Q<VisualElement>("ProfileTableDivider");
            Button _manageCredentialsButton = _container.Q<Button>("ManageCredentialsButton");
            VisualElement _containersSection = _container.Q<VisualElement>("LandingPageContainer");

            VisualElement _noAccountLandingPage = _container.Q<VisualElement>("LandingPageNoAccount");

            if (_stateManager.SelectedProfile == null)
            {
                _statusBox.Show(StatusBox.StatusBoxType.Info, Strings.LandingPageInfoStatusBoxText);
            }
            else if (!_stateManager.IsBootstrapped())
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.LandingPageWarningStatusBoxText);
                _noAccountLandingPage.RemoveFromClassList("hidden");

                _accountLandingPage.AddToClassList("hidden");
                _profileTable.AddToClassList("hidden");
                _profileTableDivider.AddToClassList("hidden");
                _manageCredentialsButton.AddToClassList("hidden");
            }
            else
            {
                _statusBox.Close();
                _noAccountLandingPage.AddToClassList("hidden");

                _accountLandingPage.RemoveFromClassList("hidden");
                _profileTable.RemoveFromClassList("hidden");
                _profileTableDivider.RemoveFromClassList("hidden");
                _manageCredentialsButton.RemoveFromClassList("hidden");
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var strings = new[]
            {
                Strings.LandingPageHeader,
                Strings.LandingPageDescription,

                Strings.LandingPageAnywhereTitle,
                Strings.LandingPageAnywhereDescription,
                Strings.LandingPageAnywhereButton,
                Strings.LandingPageManagedTitle,
                Strings.LandingPageManagedDescription,
                Strings.LandingPageManagedButton,
                Strings.LandingPageContainerTitle,
                Strings.LandingPageContainerDescription,
                Strings.LandingPageContainerButton,

                Strings.LandingPageNoAccountCardText,
                Strings.LandingPageNoAccountCardButton,
                Strings.LandingPageAccountCardText,
                Strings.LandingPageAccountCardButton,
                Strings.LandingPageSampleHeader,
                Strings.LandingPageSampleDescription,
                Strings.LandingPageSampleButton
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }

        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("LandingPageStatusBox");
        }
    }
}