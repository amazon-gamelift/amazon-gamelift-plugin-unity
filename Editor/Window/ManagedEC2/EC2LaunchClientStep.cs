// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class EC2LaunchClientStep : ProgressBarStepComponent
    {
        private const string _primaryButtonClassName = "button--primary";
        private const int RefreshUIMilliseconds = 2000;
        private readonly GameLiftClientSettingsLoader _gameLiftClientSettingsLoader;
        private readonly DeploymentStepTemplate _templateContent;
        private readonly Button _configureClientButton;

        private readonly Button _launchClientButton;
        private readonly VisualElement _launchClientDescription;

        private EC2DeploymentSettings _deploymentSettings;

        private GameLiftClientSettings _gameLiftClientSettings;


        public EC2LaunchClientStep(VisualElement container, StateManager stateManager, EC2DeploymentSettings deploymentSettings) : base(container, stateManager, "EditorWindow/Components/ManagedEC2/EC2LaunchClientStep")
        {
            _templateContent = new DeploymentStepTemplate.Builder(Strings.ManagedEC2LaunchClientTitle, null)
                 .WithoutBaseButtons()
                 .Build(container);

            LocalizeText();

            _deploymentSettings = deploymentSettings;

            _gameLiftClientSettingsLoader = new GameLiftClientSettingsLoader(_templateContent.StatusBox);
            LoadGameLiftClientSettings();


            _launchClientButton = container.Q<Button>("ManagedEC2LaunchClientButton");
            _launchClientButton.RegisterCallback<ClickEvent>(_ =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone,
                    EditorUserBuildSettings.selectedStandaloneTarget);
                EditorApplication.EnterPlaymode();
            });

            _launchClientDescription = container.Q<VisualElement>("ManagedEC2LaunchClientDescription");

            _configureClientButton = container.Q<Button>("ManagedEC2ConfigureClientButton");
            _configureClientButton.RegisterCallback<ClickEvent>(_ =>
            {
                _gameLiftClientSettings.ConfigureManagedEC2ClientSettings(_stateManager.Region, _deploymentSettings.CurrentStackInfo.ApiGatewayEndpoint, _deploymentSettings.CurrentStackInfo.UserPoolClientId);
                _stateManager.OnClientSettingsChanged?.Invoke();
            });

            _stateManager.OnClientSettingsChanged += UpdateGUI;
            _deploymentSettings.CurrentStackInfoChanged += UpdateGUI;

            UpdateGUI();
        }

        private void LoadGameLiftClientSettings()
        {
            _gameLiftClientSettings = _gameLiftClientSettingsLoader.LoadAsset();
            _container.schedule.Execute(() => {
                LoadGameLiftClientSettings();
                UpdateGUI();
            }).StartingIn(RefreshUIMilliseconds);
        }

        protected sealed override Task StartOrResumeStep()
        {
            // To be implemented. For now it's a manual step
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() { }

        protected sealed override void UpdateGUI()
        {

            bool canLaunchClient = _deploymentSettings.CurrentStackInfo.StackStatus is StackStatus.CreateComplete or StackStatus.UpdateComplete;

            // if the client settings have changed due to a deployment or due to manual changes, this will require the user to configure the client settings again
            bool isClientConfigured = _gameLiftClientSettings && !_gameLiftClientSettings.IsGameLiftAnywhere
                                            && _gameLiftClientSettings.AwsRegion == _stateManager.Region
                                            && _gameLiftClientSettings.ApiGatewayUrl == _deploymentSettings.CurrentStackInfo.ApiGatewayEndpoint
                                            && _gameLiftClientSettings.UserPoolClientId == _deploymentSettings.CurrentStackInfo.UserPoolClientId;

            bool isLaunchClientEnabled = canLaunchClient && isClientConfigured;
            bool isConfigureClientEnabled = canLaunchClient && !isClientConfigured && _gameLiftClientSettings;

            _launchClientButton.SetEnabled(isLaunchClientEnabled);
            if (isLaunchClientEnabled)
            {
                _launchClientButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _launchClientButton.RemoveFromClassList(_primaryButtonClassName);
            }

            if (_deploymentSettings.Scenario == DeploymentScenarios.FlexMatch)
            {
                Hide(_launchClientButton);
                Show(_launchClientDescription);
            }
            else
            {
                Hide(_launchClientDescription);
                Show(_launchClientButton);
            }

            _configureClientButton.SetEnabled(isConfigureClientEnabled);
            if (isConfigureClientEnabled)
            {
                _configureClientButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _configureClientButton.RemoveFromClassList(_primaryButtonClassName);
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("ManagedEC2LaunchClientTitle", Strings.ManagedEC2LaunchClientTitle);
            l.SetElementText("ManagedEC2LaunchClientLabel", Strings.ManagedEC2LaunchClientLabel);
            l.SetElementText("ManagedEC2LaunchClientButton", Strings.ManagedEC2LaunchClientButton);
            l.SetElementText("ManagedEC2LaunchClientDescription", Strings.ManagedEC2LaunchClientDescription);
            l.SetElementText("ManagedEC2ConfigureClientLabel", Strings.ManagedEC2ConfigureClientLabel);
            l.SetElementText("ManagedEC2ConfigureClientButton", Strings.ManagedEC2ConfigureClientButton);
        }
    }
}
