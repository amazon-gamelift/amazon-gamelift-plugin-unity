// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class ConfigureCGDStep : ContainerStepComponent
    {
        private GameLiftCGDManager _cgdManager => _stateManager.CGDManager;
        private ContainerGroupDefinition _containerGroupDefinition;
        private ContainerGroupDefinitionStatus _cgdStatus;
        private GameLiftSynchronizationContext _mainThreadContext;
        private DeploymentStepTemplate _stepContent;
        private ContainersDeploymentSettings _deploymentSettings;
        private Button _proceedButton;
        private Button _tryAgainButton;
        private Button _viewLogButton;
        private string _cgdName;
        private readonly StatusIndicator _statusIndicator;
        private TextProvider _textProvider;

        public ConfigureCGDStep(VisualElement container, StateManager stateManager, ContainersDeploymentSettings deploymentSettings) : base(container, stateManager, "EditorWindow/Components/Containers/ConfigureCGDStep")
        {
            _stepContent = new DeploymentStepTemplate.Builder(Strings.ContainerConfigureCGDStepTitle, Strings.ContainerConfigureCGDStepDescription)
                .WithBaseButtons()
                .Build(container);

            _textProvider = new TextProvider();

            _deploymentSettings = deploymentSettings;
            
            if (stateManager.IsBootstrapped() && stateManager.IsInContainersRegion())
            {
                _deploymentSettings.Restore();
            }
            _deploymentSettings.Refresh();

            _deploymentSettings.CurrentStackInfoChanged += PollResourceStatus;

            _proceedButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonProceed);
            _tryAgainButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonTryAgain);
            _viewLogButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonViewLogs);

            _statusIndicator = _container.Q<StatusIndicator>();

            _proceedButton.RegisterCallback<ClickEvent>(_ => {}); // NO-OP HERE: THERE IS NO WARNING STATE FOR THIS STEP
            _tryAgainButton.RegisterCallback<ClickEvent>(_ => { base.ResetAndTryStart(); });
            _viewLogButton.RegisterCallback<ClickEvent>(_ => {}); // NO-OP HERE: We did not implement this callback

            Hide(_proceedButton);
            Hide(_tryAgainButton);
            Hide(_viewLogButton);

            Hide(_stepContent.ButtonContainer);
            Hide(_stepContent.ContentContainer);

            _mainThreadContext = GameLiftSynchronizationContext.Current;
        }

        protected sealed override Task StartOrResumeStep() {

            _cgdName = $"{_stateManager.ContainerGameName}-GroupDefinition";

            PopulateContent();

            if (_stateManager.ContainerECRImageId.Contains("sha256:"))
            {
                _stateManager.ContainerECRImageUri = _stateManager.ContainerECRRepositoryUri + "@" + _stateManager.ContainerECRImageId;
            }
            else
            {
                _stateManager.ContainerECRImageUri = _stateManager.ContainerECRRepositoryUri + ":" + _stateManager.ContainerECRImageId;
            }

            _deploymentSettings.Restore();

            _stateManager.IsCGDDeploying = true;

            _deploymentSettings.StartDeployment();

            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() 
        {
            Hide(_stepContent.ButtonContainer);
            Hide(_stepContent.ContentContainer);
            _stateManager.IsCGDDeploying = false;
            _stateManager.IsCGDDeployed = false;
            _cgdStatus = null;
            _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
        }

        protected void PollResourceStatus()
        {
            if (!_stateManager.IsCGDDeploying)
            {
                return;
            }
            
            GetCGDStatus();
            var stackStatus = _deploymentSettings.CurrentStackInfo.StackStatus;

            if (stackStatus == null)
            {
                _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }
            else if (stackStatus.IsStackStatusInProgress())
            {
                if (_cgdStatus == null)
                {
                    _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
                }
                else if (_cgdStatus == ContainerGroupDefinitionStatus.COPYING)
                {
                    _statusIndicator.Set(State.InProgress, _textProvider.Get(Strings.ManagedEC2DeployStatusDeploying));
                }
                else if (_cgdStatus == ContainerGroupDefinitionStatus.READY)
                {
                    _statusIndicator.Set(State.Success, _textProvider.Get(Strings.ManagedEC2DeployStatusDeployed));
                    CompleteCreateCGD();
                }
            }
            else if (stackStatus.IsStackStatusRollback() || stackStatus.IsStackStatusFailed())
            {
                _statusIndicator.Set(State.Failed, _textProvider.Get(Strings.ManagedEC2DeployStatusFailed));

                if (_cgdStatus == null)
                {
                    FailStep(StatusBox.StatusBoxType.Error, "Resource creation failed. Rolled back.");
                }
                else if (_cgdStatus == ContainerGroupDefinitionStatus.FAILED)
                {
                    FailStep(StatusBox.StatusBoxType.Error, "Group Definition creation failed. Rolled back.");
                }
                else
                {
                    FailStep(StatusBox.StatusBoxType.Error, "Resource creation failed. Rolled back.");
                }
            }
            else if (stackStatus.IsStackStatusOperationDone())
            {
                _statusIndicator.Set(State.Success, _textProvider.Get(Strings.ManagedEC2DeployStatusDeployed));
                CompleteCreateCGD();
            }
            else
            {
                _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }   
        }

        private async Task GetCGDStatus()
        {
            _containerGroupDefinition = await _cgdManager.GetContainerGroupDefinition(_cgdName, _containerGroupDefinition != null);

            if (_containerGroupDefinition == null)
            {
                return;
            }
            
            _cgdStatus = _containerGroupDefinition.Status;
            PopulateContent();
        }

        private void CompleteCreateCGD()
        {
            _stateManager.IsCGDDeploying = false;
            _stateManager.IsCGDDeployed = true;
            base.CompleteStep();
        }

        private string GetCGDVersion()
        {
            if (_containerGroupDefinition != null)
            {
                return _containerGroupDefinition.VersionNumber.ToString();
            }
            else
            {
                return "";
            }
        }
        protected void FailStep(StatusBox.StatusBoxType statusBoxType, string errorMessage, bool withLogs = false)
        {
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

            _stateManager.IsCGDDeploying = false;
            _stateManager.IsCGDDeployed = false;

            PopulateContent();
        }

        private void PopulateContent()
        {
            Show(_stepContent.ContentContainer);
            _stepContent.ContentContainer.Q<Label>("CGDNameValue").text = DashIfEmpty(_cgdName);
            _stepContent.ContentContainer.Q<Label>("CGDVersionValue").text = DashIfEmpty(GetCGDVersion());
            _stepContent.ContentContainer.Q<Label>("MemoryLimitValue").text = _stateManager.ContainerTotalMemory != null ? _stateManager.ContainerTotalMemory + " MiB" : "-";
            _stepContent.ContentContainer.Q<Label>("VcpuLimitValue").text = _stateManager.ContainerTotalVcpu != null ? _stateManager.ContainerTotalVcpu + " vCPUs" : "-";
        }
    }
}
