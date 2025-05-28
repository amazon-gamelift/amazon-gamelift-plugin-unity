// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace AmazonGameLift.Editor
{
    public class AnywherePage
    {
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private readonly AnywhereIntegrateStep _integrateInput;
        private readonly ConnectToFleetInput _connectToFleetInput;
        private readonly RegisterComputeInput _registerComputeInput;
        private readonly AnywhereLaunchStep _launchInput;
        private readonly List<ProgressBarStepComponent> _progressBarSteps;
        private StatusBox _statusBox;
        private Action _startAction;

        public AnywherePage(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AnywherePage");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            LocalizeText();
            SetupStatusBoxes();

            container.Q<Button>("ManageCredentialsButton")
                .RegisterCallback<ClickEvent>(_ => EditorMenu.OpenAccountProfilesTab());

            _stateManager.OnFleetChanged += UpdateGui;
            _stateManager.OnUserProfileUpdated += UpdateGui;
            _stateManager.OnComputeChanged += UpdateGui;
            _stateManager.OnClientSettingsChanged += UpdateGui;
            _stateManager.OnUserProfileUpdated += () =>
            {
                var firstStep = _progressBarSteps[0];
                firstStep.Reset();
                firstStep.TryStart();
            };

            var integrateContainer = uxml.Q("AnywherePageIntegrateStepTitle");
            _integrateInput = new AnywhereIntegrateStep(integrateContainer, stateManager);
            var fleetInputContainer = uxml.Q("AnywherePageConnectFleetTitle");
            _connectToFleetInput = new ConnectToFleetInput(fleetInputContainer, stateManager);
            var computeInputContainer = uxml.Q("AnywherePageComputeTitle");
            _registerComputeInput = new RegisterComputeInput(computeInputContainer, stateManager);
            var launchInputContainer = uxml.Q("AnywherePageLaunchTitle");
            _launchInput = new AnywhereLaunchStep(launchInputContainer, stateManager);

            _progressBarSteps = new() { _integrateInput, _connectToFleetInput, _registerComputeInput, _launchInput };
            _startAction = ProgressFlowContainer.SetupSteps(_progressBarSteps);

            UpdateGui();
            _startAction();
        }


        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("AnywherePageStatusBox");
        } 

        private void UpdateGui()
        {
            if (!_stateManager.IsBootstrapped())
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.AnywherePageStatusBoxNotBootstrappedWarning);
            }
            else
            {
                _statusBox.Close();
            } 

            _startAction = ProgressFlowContainer.SetupSteps(_progressBarSteps);

        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageTitle", Strings.AnywherePageTitle);
            l.SetElementText("AnywherePageDescription", Strings.AnywherePageDescription);
        }
    }
}
