// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2GameParametersStep : ProgressBarStepComponent
    {
        private readonly FleetParametersInput _fleetParamsInput;
        private VisualElement parametersContainer;
        private VisualElement parametersContainerInProgress;
        private VisualElement parametersContainerComplete;
        private Button setParametersButton;
        private Button modifyParametersButton;
        private Label gameNameText;
        private Label fleetNameText;
        private Label buildNameText;
        private Label launchParametersText;
        private Label buildOSText;
        private Label gameServerFolderText;
        private Label gameServerFileText;
        private EC2DeploymentSettings _deploymentSettings;

        public ManagedEC2GameParametersStep(VisualElement container, StateManager stateManager, EC2DeploymentSettings deploymentSettings, ManagedEC2FleetParameters parameters, EC2DeployStep ec2DeployStep) : base(container, stateManager, "EditorWindow/Components/ManagedEC2/ManagedEC2GameParametersStep")
        {
            new DeploymentStepTemplate.Builder(Strings.ManagedEC2ParametersTitle, Strings.ManagedEC2ParametersDescription)
                 .WithoutBaseButtons()
                 .Build(container);

            _deploymentSettings = deploymentSettings;

            parametersContainer = container.Q<VisualElement>("ManagedEC2ParametersInputContainer");
            parametersContainerInProgress = container.Q<VisualElement>("ParametersContainerInProgress");
            parametersContainerComplete = container.Q<VisualElement>("ParametersContainerComplete");

            gameNameText = container.Q<Label>("GameNameDisplay");
            fleetNameText = container.Q<Label>("FleetNameDisplay");
            buildNameText = container.Q<Label>("BuildNameDisplay");
            launchParametersText = container.Q<Label>("LaunchParametersDisplay");
            buildOSText = container.Q<Label>("BuildOSDisplay");
            gameServerFolderText = container.Q<Label>("GameServerFolderDisplay");
            gameServerFileText = container.Q<Label>("GameServerFileDisplay");

            setParametersButton = container.Q<Button>("SetParametersButton");
            modifyParametersButton = container.Q<Button>("ModifyParametersButton");

            setParametersButton.RegisterCallback<ClickEvent>(_ => SetParametersClicked());
            modifyParametersButton.RegisterCallback<ClickEvent>(_ => ModifyParametersClicked());

            _fleetParamsInput = new FleetParametersInput(parametersContainer, parameters);
            _fleetParamsInput.SetEnabled(deploymentSettings.CanEdit);

            _fleetParamsInput.OnValueChanged += fleetParameters =>
            {
                ec2DeployStep.UpdateDeploymentSettings(fleetParameters);
            };

            Hide(parametersContainerInProgress);
            Hide(parametersContainerComplete);

            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;
        }

        protected sealed override Task StartOrResumeStep()
        {
            Show(parametersContainerInProgress);
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() 
        {
            Hide(parametersContainerComplete);
            Hide(parametersContainerInProgress);
        }

        private void SetParametersClicked()
        {
            TryStart();

            PopulateLabels();

            Hide(parametersContainerInProgress);
            Show(parametersContainerComplete);

            CompleteStep();
        }

        private void ModifyParametersClicked()
        {
            Reset();
            Show(parametersContainerInProgress);
            Hide(parametersContainerComplete);
        }

        private void PopulateLabels()
        {
            gameNameText.text = _deploymentSettings.GameName;
            fleetNameText.text = _deploymentSettings.FleetName;
            buildNameText.text = _deploymentSettings.BuildName;
            launchParametersText.text = _deploymentSettings.LaunchParameters;
            buildOSText.text = _deploymentSettings.BuildOperatingSystem;
            gameServerFolderText.text = _deploymentSettings.BuildFolderPath;
            gameServerFileText.text = _deploymentSettings.BuildFilePath;
        }

        protected sealed override void UpdateGUI()
        {
            _fleetParamsInput.SetEnabled(_deploymentSettings.CanEdit);
            if (_deploymentSettings.CurrentStackInfo.StackStatus == null)
            {
                return;
            }

            bool canModify = StackStatus.IsStackStatusOperationDone(_deploymentSettings.CurrentStackInfo.StackStatus);
            if (canModify)
            {
                modifyParametersButton.SetEnabled(true);
            }
            else
            {
                modifyParametersButton.SetEnabled(false);
            }
        }
    }
}
