// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2DeploymentScenariosStep : ProgressBarStepComponent
    {
        private EC2DeploymentSettings _deploymentSettings;
        private VisualElement _deploymentScenariosContainer;
        private VisualElement _deploymentScenariosContainerComplete;
        private RadioButton _singleRegionButton;
        private RadioButton _multiRegionButton;
        private Button _configureParametersButton;
        private Button _modifyScenarioButton;
        private Label _fleetTypeText;

        private string _singleRegionString = "Single-region fleet";
        private string _flexMatchString = "FlexMatch fleet";

        public ManagedEC2DeploymentScenariosStep(VisualElement container, StateManager stateManager, EC2DeploymentSettings deploymentSettings) : base(container, stateManager, "EditorWindow/Components/ManagedEC2/ManagedEC2DeploymentScenariosStep")
        {
            new DeploymentStepTemplate.Builder(Strings.DeploymentScenarioTitle, Strings.DeploymentScenarioDescription)
                 .WithHelpLinks(
                     new DeploymentStepTemplateLink(Urls.ManagedEC2ScenariosLearnMore, Strings.DeploymentScenarioHelpLinkScenarios),
                     new DeploymentStepTemplateLink(Urls.SupportedGameLiftRegions, Strings.DeploymentScenarioHelpLinkLocations),
                     new DeploymentStepTemplateLink(Urls.AboutGameLiftPricing, Strings.DeploymentScenarioHelpLinkPricing))
                 .WithoutBaseButtons()
                 .Build(container);

            _deploymentSettings = deploymentSettings;

            _deploymentScenariosContainer = container.Q<VisualElement>("ScenariosContainerInProgress");
            _deploymentScenariosContainerComplete = container.Q<VisualElement>("ScenariosContainerComplete");
            Hide(_deploymentScenariosContainer);
            Hide(_deploymentScenariosContainerComplete);

            _singleRegionButton = container.Q<RadioButton>(Strings.DeploymentScenarioSingleFleetRadio);
            _multiRegionButton = container.Q<RadioButton>(Strings.DeploymentScenarioFlexMatchRadio);

            _configureParametersButton = container.Q<Button>("ConfigureParametersButton");
            _modifyScenarioButton = container.Q<Button>("ModifyScenarioButton");

            _fleetTypeText = container.Q<Label>("EC2ScenarioDisplay");

            _singleRegionButton.RegisterCallback<ClickEvent>(_ => deploymentSettings.Scenario = DeploymentScenarios.SingleRegion);
            _multiRegionButton.RegisterCallback<ClickEvent>(_ => deploymentSettings.Scenario = DeploymentScenarios.FlexMatch);
            _configureParametersButton.RegisterCallback<ClickEvent>(_ => ConfigureParametersClicked());
            _modifyScenarioButton.RegisterCallback<ClickEvent>(_ => ModifyScenarioClicked());

            LocalizeText();
            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var strings = new[]
            {
                Strings.DeploymentScenarioSingleFleetLabelEc2,
                Strings.DeploymentScenarioFlexMatchLabelEc2
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }

        protected sealed override Task StartOrResumeStep()
        {
            Show(_deploymentScenariosContainer);
            SetupRadioButtons();
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() { }

        private void SetupRadioButtons()
        {
            _singleRegionButton.value = _deploymentSettings.Scenario == DeploymentScenarios.SingleRegion ? true : false;
            _multiRegionButton.value = _deploymentSettings.Scenario == DeploymentScenarios.FlexMatch ? true : false;
        }

        private void ConfigureParametersClicked()
        {
            TryStart();

            _deploymentSettings.Scenario = _singleRegionButton.value == true ? DeploymentScenarios.SingleRegion : DeploymentScenarios.FlexMatch;
            _fleetTypeText.text = _deploymentSettings.Scenario == DeploymentScenarios.SingleRegion ? _singleRegionString : _flexMatchString;
            Hide(_deploymentScenariosContainer);
            Show(_deploymentScenariosContainerComplete);

            CompleteStep();
        }

        private void ModifyScenarioClicked()
        {
            Reset();
            Hide(_deploymentScenariosContainerComplete);

            Show(_deploymentScenariosContainer);
        }

        protected sealed override void UpdateGUI()
        {
            if (_deploymentSettings.CurrentStackInfo.StackStatus == null)
            {
                return;
            }

            bool canModify = StackStatus.IsStackStatusOperationDone(_deploymentSettings.CurrentStackInfo.StackStatus);
            if (canModify)
            {
                _modifyScenarioButton.SetEnabled(true);
            }
            else
            {
                _modifyScenarioButton.SetEnabled(false);
            }
        }
    }
}
