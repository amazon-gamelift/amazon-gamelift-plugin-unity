// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLift.Editor
{
    public class EC2DeployStep : ProgressBarStepComponent
    {
        private const string _primaryButtonClassName = "button--primary";
        private const int RefreshUIMilliseconds = 2000;
        private GameLiftClientSettings _gameLiftClientSettings;
        private readonly GameLiftClientSettingsLoader _gameLiftClientSettingsLoader;
        private readonly DeploymentStepTemplate _templateContent;

        private readonly VisualElement _statusLink;
        private readonly StatusIndicator _statusIndicator;
        private EC2DeploymentSettings _deploymentSettings;
        private readonly Button _deployButton;
        private readonly Button _deleteButton;
        private readonly ManagedEC2Deployment _ec2Deployment;
        private readonly ManagedEC2FleetParameters _managedEC2FleetParameters;

        private VisualElement _ec2DeployContainer;
        private VisualElement _ec2DeployButtonContainer;
        private bool _hasStarted;

        public EC2DeployStep(
            VisualElement container,
            StateManager stateManager,
            EC2DeploymentSettings deploymentSettings,
            ManagedEC2FleetParameters managedEC2FleetParameters) : base(container, stateManager, "EditorWindow/Components/ManagedEC2/EC2DeployStep")
        {
            _deploymentSettings = deploymentSettings;
            _managedEC2FleetParameters = managedEC2FleetParameters;
            _statusIndicator = _container.Q<StatusIndicator>();
            _statusLink = container.Q("ManagedEC2DeployStatusLink");
            _statusLink.RegisterCallback<ClickEvent>(_ => Application.OpenURL(
                string.Format(Urls.AwsCloudFormationEventsTemplate, _stateManager.Region, _deploymentSettings.CurrentStackInfo.StackId)));

            _ec2DeployContainer = container.Q<VisualElement>("ManagedEC2DeployContainer");
            _ec2DeployButtonContainer = container.Q<VisualElement>("ManagedEC2ButtonContainer");
            Hide(_ec2DeployContainer);
            Hide(_ec2DeployButtonContainer);

            _ec2Deployment = new ManagedEC2Deployment(_deploymentSettings);

            _templateContent = new DeploymentStepTemplate.Builder("Deploy scenario", Strings.ManagedEC2DeployDescription)
                 .WithoutBaseButtons()
                 .Build(container);

            LocalizeText();

            _deployButton = container.Q<Button>("ManagedEC2CreateStackButton");
            _deployButton.RegisterCallback<ClickEvent>(_ =>
            {
                TryStart(); // Use it to close status box and mark progress bar as in progress
                _ec2Deployment.StartDeployment();
                UpdateGUI();
            });

            _deleteButton = container.Q<Button>("ManagedEC2DeleteStackButton");
            _deleteButton.RegisterCallback<ClickEvent>(async _ =>
            {
                await _ec2Deployment.DeleteDeployment();
                _deploymentSettings.RefreshCurrentStackInfo();
                UpdateGUI();
                Reset();
                Show(_ec2DeployContainer);
                Show(_ec2DeployButtonContainer);
            });

            _stateManager.OnUserProfileUpdated += () => UpdateDeploymentSettings(_managedEC2FleetParameters);
            _stateManager.OnUserProfileUpdated += UpdateGUI;
            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;

            _deploymentSettings.RefreshCurrentStackInfo();
        }

        public void UpdateDeploymentSettings(ManagedEC2FleetParameters managedEC2FleetParameters)
        {
            if (_stateManager.IsBootstrapped())
            {
                _deploymentSettings.Refresh();
                _deploymentSettings.Restore();
                _ec2Deployment.UpdateModelFromParameters(managedEC2FleetParameters);
            }

            UpdateGUI();
        }

        protected sealed override Task StartOrResumeStep()
        {
            _hasStarted = true;
            Show(_ec2DeployButtonContainer);
            Show(_ec2DeployContainer);
            UpdateGUI();
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() 
        {
           Hide(_ec2DeployContainer);
           Hide(_ec2DeployButtonContainer);
        }

        protected sealed override void UpdateGUI()
        {
            if (!_hasStarted) return;
            bool canDeploy = _deploymentSettings.CurrentStackInfo.StackStatus == null &&
                                     _deploymentSettings.CanDeploy;

            _deployButton.SetEnabled(canDeploy);
            if (canDeploy)
            {
                _deployButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _deployButton.RemoveFromClassList(_primaryButtonClassName);
            }

            _deleteButton.SetEnabled(_deploymentSettings.CanDelete);


            _templateContent.StatusBox.Close();
            var stackStatus = _deploymentSettings.CurrentStackInfo.StackStatus;
            var textProvider = new TextProvider();
            if (stackStatus == null)
            {
                _statusIndicator.Set(State.Inactive, textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }
            else if (stackStatus.IsStackStatusFailed())
            {
                _statusIndicator.Set(State.Failed, textProvider.Get(Strings.ManagedEC2DeployStatusFailed));
                EncounteredException(StatusBox.StatusBoxType.Error, textProvider.GetError(ErrorCode.StackStatusInvalid));
            }
            else if (stackStatus == StackStatus.DeleteInProgress)
            {
                _statusIndicator.Set(State.InProgress, textProvider.Get(Strings.ManagedEC2DeployStatusDeleting));
            }
            else if (stackStatus.IsStackStatusRollback())
            {
                _statusIndicator.Set(State.Failed, textProvider.Get(stackStatus.IsStackStatusInProgress()
                    ? Strings.ManagedEC2DeployStatusRollingBack
                    : Strings.ManagedEC2DeployStatusRolledBack));
                EncounteredException(StatusBox.StatusBoxType.Error,
                    textProvider.GetError(ErrorCode.StackStatusInvalid));
            }
            else if (stackStatus.IsStackStatusInProgress())
            {
                _statusIndicator.Set(State.InProgress, textProvider.Get(Strings.ManagedEC2DeployStatusDeploying));
            }
            else if (stackStatus.IsStackStatusOperationDone())
            {
                _statusIndicator.Set(State.Success, textProvider.Get(Strings.ManagedEC2DeployStatusDeployed));
                CompleteStep();
            }
            else
            {
                _statusIndicator.Set(State.Inactive, textProvider.Get(Strings.ManagedEC2DeployStatusNotDeployed));
            }

            _statusLink.visible = _deploymentSettings.HasCurrentStack;
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("ManagedEC2DeployStatusLabel", Strings.ManagedEC2DeployStatusLabel);
            l.SetElementText("ManagedEC2DeployActionsLabel", Strings.ManagedEC2DeployActionsLabel);
            l.SetElementText("ManagedEC2CreateStackButton", Strings.ManagedEC2CreateStackButton);
            l.SetElementText("ManagedEC2DeleteStackButton", Strings.ManagedEC2DeleteStackButton);
            l.SetElementText("ManagedEC2DeployStatusLinkLabel", Strings.ManagedEC2DeployStatusLink);
        }
    }
}
