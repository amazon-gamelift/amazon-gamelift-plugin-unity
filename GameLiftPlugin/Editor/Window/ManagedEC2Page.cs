// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;
using AmazonGameLift.Runtime;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2Page
    {
        private readonly VisualElement _container;
        private readonly StateManager _stateManager;
        private readonly EC2DeploymentSettings _deploymentSettings;

        private StatusBox _bootstrapStatusBox;

        private readonly List<ProgressBarStepComponent> _progressBarSteps;
        private readonly EC2DeployStep _ec2DeployStep;
        private readonly EC2LaunchClientStep _ec2LaunchClientStep;
        private readonly ElementLocalizer _elementLocalizer;

        private readonly ManagedEC2IntegrateStep _integrateInput;
        private readonly ManagedEC2DeploymentScenariosStep _deploymentScenarioInput;
        private readonly ManagedEC2GameParametersStep _gameParametersInput;


        public ManagedEC2Page(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;
            _deploymentSettings = DeploymentSettingsFactory.Create(stateManager);
            if (_stateManager.IsBootstrapped())
            {
                _deploymentSettings.Restore();
            }
            _deploymentSettings.Refresh();
            var parameters = GetManagedEC2Parameters(_deploymentSettings);

            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/ManagedEC2Page");
            var uxml = mVisualTreeAsset.Instantiate();

            container.Add(uxml);
            SetupStatusBoxes();

            container.Q<Button>("ManageCredentialsButton")
                .RegisterCallback<ClickEvent>(_ => EditorMenu.OpenAccountProfilesTab());

            _stateManager.OnUserProfileUpdated += UpdateStatusBoxes;
            _stateManager.OnUserProfileUpdated += _deploymentSettings.Refresh;

            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;
            _deploymentSettings.Scenario = _stateManager.DeploymentScenario;

            _elementLocalizer = new ElementLocalizer(_container);

            var integrateContainer = uxml.Q("ManagedEC2IntegrateStepTitle");
            _integrateInput = new ManagedEC2IntegrateStep(integrateContainer, stateManager);

            var deploymentScenarioContainer = uxml.Q("ManagedEC2DeploymentScenariosStepTitle");
            _deploymentScenarioInput = new ManagedEC2DeploymentScenariosStep(deploymentScenarioContainer, stateManager, _deploymentSettings);

            // Deploy step
            var deployContainer = uxml.Q("ManagedEC2PageDeployTitle");
            _ec2DeployStep = new EC2DeployStep(deployContainer, stateManager, _deploymentSettings, parameters);

            var gameParametersContainer = uxml.Q("ManagedEC2GameParametersStep");
            _gameParametersInput = new ManagedEC2GameParametersStep(gameParametersContainer, stateManager, _deploymentSettings, parameters, _ec2DeployStep);

            // Launch client step
            var launchClientContainer = uxml.Q("ManagedEC2PageLaunchClientTitle");
            _ec2LaunchClientStep = new EC2LaunchClientStep(launchClientContainer, stateManager, _deploymentSettings);

            _progressBarSteps = new() { _integrateInput, _deploymentScenarioInput, _gameParametersInput, _ec2DeployStep, _ec2LaunchClientStep };
            ProgressFlowContainer.SetupSteps(_progressBarSteps);


            UpdateGUI();
            UpdateStatusBoxes();
        }

        private ManagedEC2FleetParameters GetManagedEC2Parameters(EC2DeploymentSettings deploymentSettings)
        {
            return new ManagedEC2FleetParameters
            {
                GameName = deploymentSettings.GameName ?? Application.productName,
                FleetName = deploymentSettings.FleetName ?? $"{Application.productName}-ManagedFleet",
                LaunchParameters = deploymentSettings.LaunchParameters ?? $"",
                BuildName = deploymentSettings.BuildName ??
                            $"{Application.productName}-{deploymentSettings.ScenarioName.Replace(" ", "_")}-Build",
                GameServerFile = deploymentSettings.BuildFilePath,
                GameServerFolder = deploymentSettings.BuildFolderPath,
                OperatingSystem = OperatingSystem.FindValue(deploymentSettings.BuildOperatingSystem) ??
                                  OperatingSystem.AMAZON_LINUX_2
            };
        }

        private void UpdateGUI()
        {
            LocalizeText();
        }
        
        private void SetupStatusBoxes()
        {
            _bootstrapStatusBox = _container.Q<StatusBox>("ManagedEC2StatusBox");
        }
        
        private void UpdateStatusBoxes()
        {
            if (!_stateManager.IsBootstrapped())
            {
                _bootstrapStatusBox.Show(StatusBox.StatusBoxType.Warning, Strings.ManagedEC2StatusBoxNotBootstrappedWarning);
            }
            else
            {
                _bootstrapStatusBox.Close();
            }
        }

        private void LocalizeText()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "GameName", Application.productName },
            };
            _elementLocalizer.SetElementText("ManagedEC2Title", Strings.ManagedEC2Title);
            _elementLocalizer.SetElementText("ManagedEC2Description", Strings.ManagedEC2Description);
            _elementLocalizer.SetElementText("DeploymentScenarioTitle", Strings.DeploymentScenarioTitle);
            _elementLocalizer.SetElementText("ManagedEC2ParametersTitle", Strings.ManagedEC2ParametersTitle, replacements);
        }

        private string GetScenarioType() => _deploymentSettings.Scenario switch
        {
            DeploymentScenarios.SingleRegion => _elementLocalizer.GetText(Strings.DeploymentScenarioSingleFleetLabelEc2),
            DeploymentScenarios.FlexMatch => _elementLocalizer.GetText(Strings.DeploymentScenarioFlexMatchLabelEc2),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
