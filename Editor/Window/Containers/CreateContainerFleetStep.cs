// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class CreateContainerFleetStep : ContainerStepComponent
    {
        private GameLiftSynchronizationContext _mainThreadContext;
        private DeploymentStepTemplate _stepContent;
        private Button _proceedButton;
        private Button _tryAgainButton;
        private Button _viewLogButton;
        private ContainersDeploymentSettings _deploymentSettings;
        private readonly StatusIndicator _statusIndicator;
        private TextProvider _textProvider;
        private string _containerFleetTypeValue = "ON_DEMAND";
        private string _containerFleetInstanceTypeValue = "c4.xlarge";

        public CreateContainerFleetStep(VisualElement container, StateManager stateManager, ContainersDeploymentSettings deploymentSettings) : base(container, stateManager, "EditorWindow/Components/Containers/CreateContainerFleetStep")
        {
            _stepContent = new DeploymentStepTemplate.Builder(Strings.ContainerCreateContainerFleetStepTitle, Strings.ContainerCreateContainerFleetDescription)
                .WithBaseButtons()
                .Build(container);

            _statusIndicator = _container.Q<StatusIndicator>();

            _textProvider = new TextProvider();

            _deploymentSettings = deploymentSettings;
            _deploymentSettings.CurrentStackInfoChanged += PollStackStatus;

            _proceedButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonProceed);
            _tryAgainButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonTryAgain);
            _viewLogButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonViewLogs);

            _proceedButton.RegisterCallback<ClickEvent>(_ => { }); // NO-OP HERE: THERE IS NO WARNING STATE FOR THIS STEP
            _tryAgainButton.RegisterCallback<ClickEvent>(_ => { TryAgain(); });
            _viewLogButton.RegisterCallback<ClickEvent>(_ => { }); // NO-OP HERE: We did not implement this callback

            Hide(_proceedButton);
            Hide(_tryAgainButton);
            Hide(_viewLogButton);

            Hide(_stepContent.ButtonContainer);
            Hide(_stepContent.ContentContainer);

            _mainThreadContext = GameLiftSynchronizationContext.Current;
        }

        protected sealed override Task StartOrResumeStep()
        {
            PopulateContent();

            return null;
        }

        protected sealed override void ResetStep() 
        {
            Hide(_stepContent.ButtonContainer);
            Hide(_stepContent.ContentContainer);
            _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
        }

        protected void TryAgain()
        {
            // Restart deployment from the CGD Step
            ResetStep();
            _prevStep.ResetAndTryStart();
        }

        protected void PollStackStatus()
        {
            if (_stateManager.IsCGDDeployed)
            {
                var stackStatus = _deploymentSettings.CurrentStackInfo.StackStatus;
                if (stackStatus == null)
                {
                    _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
                }
                else if (stackStatus.IsStackStatusFailed())
                {
                    FailStep(StatusBox.StatusBoxType.Error, "Failed.");
                    _statusIndicator.Set(State.Failed, _textProvider.Get(Strings.ManagedEC2DeployStatusFailed));
                }
                else if (stackStatus.IsStackStatusInProgress())
                {
                    _statusIndicator.Set(State.InProgress, _textProvider.Get(Strings.ManagedEC2DeployStatusDeploying));
                }
                else if (stackStatus.IsStackStatusRollback())
                {
                    FailStep(StatusBox.StatusBoxType.Error, stackStatus.IsStackStatusInProgress() ? "Rolling back." : "Rolled back.");
                    _statusIndicator.Set(State.Failed, _textProvider.Get(Strings.ManagedEC2DeployStatusRolledBack));
                }
                else if (stackStatus.IsStackStatusOperationDone())
                {
                    _statusIndicator.Set(State.Success, _textProvider.Get(Strings.ManagedEC2DeployStatusDeployed));
                    _stateManager.ContainersDeploymentComplete = true;
                    base.CompleteStep();
                }
            }   
        }

        protected void FailStep(StatusBox.StatusBoxType statusBoxType, string errorMessage, bool withLogs = false)
        {
            // TO-DO: Add failure logic
            base.EncounteredException(
                statusBoxType: statusBoxType,
                text: errorMessage,
                externalButtonLink: string.Format(Urls.AwsCloudFormationEventsTemplate, _stateManager.Region, _deploymentSettings.CurrentStackInfo.StackId),
                externalButtonText: _textProvider.Get(Strings.ContainerFailStepViewInConsole),
                externalTargetType: StatusBox.StatusBoxExternalTargetType.Link
                );

            Show(_stepContent.ButtonContainer);
            Show(_tryAgainButton);
            ShowHide(_viewLogButton, withLogs);
            Hide(_proceedButton);

            PopulateContent();
        }

        protected string GetContainerFleetID()
        {
            string containerFleetID = _deploymentSettings.GetContainerFleetID();
            if (containerFleetID != null)
            {
                return containerFleetID;
            }
            else
            {
                return "";
            }
        }

        private void PopulateContent()
        {
            Show(_stepContent.ContentContainer);
            _stepContent.ContentContainer.Q<Label>("ContainerFleetIDValue").text = DashIfEmpty(GetContainerFleetID());
            _stepContent.ContentContainer.Q<Label>("ContainerFleetTypeValue").text = DashIfEmpty(_containerFleetTypeValue);
            _stepContent.ContentContainer.Q<Label>("ContainerFleetInstanceTypeValue").text = DashIfEmpty(_containerFleetInstanceTypeValue);
        }
    }
}
