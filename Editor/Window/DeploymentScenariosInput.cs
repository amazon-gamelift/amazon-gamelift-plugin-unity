// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class DeploymentScenariosInput : StatefulInput
    {
        private bool _enabled;
        private DeploymentScenarios _deploymentScenarios;
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;

        private Func<DeploymentScenarios> _getDeploymentStrategy;
        public Action<DeploymentScenarios> OnValueChanged;
        private Dictionary<DeploymentScenarios , RadioButton> _radioButtons = new Dictionary<DeploymentScenarios, RadioButton>();

        public DeploymentScenariosInput(VisualElement container, DeploymentScenarios deploymentScenario, bool enabled, StateManager stateManager, Func<DeploymentScenarios> getDeploymentStrategy)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/DeploymentScenarios");
            container.Add(uxml.Instantiate());

            _container = container;
            _deploymentScenarios = deploymentScenario;
            _enabled = enabled;
            _stateManager = stateManager;

            _getDeploymentStrategy = getDeploymentStrategy;

            _container.SetEnabled(_enabled);

            SetupRadioButton(Strings.DeploymentScenarioSingleFleetRadio, DeploymentScenarios.SingleRegion);
            SetupRadioButton(Strings.DeploymentScenarioFlexMatchRadio, DeploymentScenarios.FlexMatch);
            
            stateManager.OnUserProfileUpdated += UpdateGUI;

            LocalizeText();
            UpdateGUI();
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            _container.SetEnabled(_enabled);
        }

        public void PopulateContent()
        {
            if (_stateManager.ContainerDeploymentScenario == DeploymentScenarios.FlexMatch)
            {
                _radioButtons[DeploymentScenarios.FlexMatch].value = true;
                _radioButtons[DeploymentScenarios.SingleRegion].value = false;
            }
            else
            {
                _radioButtons[DeploymentScenarios.FlexMatch].value = false;
                _radioButtons[DeploymentScenarios.SingleRegion].value = true;
            }
        }

        private void SetupRadioButton(string elementName, DeploymentScenarios deploymentScenario)
        {
            var radio = _container.Q<RadioButton>(elementName);
            if (radio == default) return;
            _radioButtons.Add(deploymentScenario, radio);
            radio.value = _deploymentScenarios == deploymentScenario;
            radio.RegisterValueChangedCallback(v =>
            {
                if (_enabled && v.newValue)
                {
                    _deploymentScenarios = deploymentScenario;
                    OnValueChanged?.Invoke(_deploymentScenarios);
                }
            });
        }

        protected sealed override void UpdateGUI()
        {
            var deploymentScenario = _getDeploymentStrategy();
            _radioButtons[deploymentScenario].value = true;
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText(Strings.DeploymentScenarioSingleFleetLabel, Strings.DeploymentScenarioSingleFleetLabelContainers);
            l.SetElementText(Strings.DeploymentScenarioSingleFleetLink, Strings.DeploymentScenarioSingleFleetLink);
            l.SetElementText(Strings.DeploymentScenarioFlexMatchLabel, Strings.DeploymentScenarioFlexMatchLabelContainers);
            l.SetElementText(Strings.DeploymentScenarioFlexMatchLink, Strings.DeploymentScenarioFlexMatchLink);
            l.SetElementText(Strings.DeploymentScenarioShowMoreButton, Strings.DeploymentScenarioShowMoreButton);
        }
    }
}
