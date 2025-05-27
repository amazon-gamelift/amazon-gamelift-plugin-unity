// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class AnywhereLaunchStep : ProgressBarStepComponent
    {
        private const string _primaryButtonClassName = "button--primary";
        private const int RefreshUIMilliseconds = 2000;
        private GameLiftClientSettings _gameLiftClientSettings;
        private readonly GameLiftClientSettingsLoader _gameLiftClientSettingsLoader;
        private readonly DeploymentStepTemplate _templateContent;
        private readonly Button _launchServerButton;
        private readonly Button _configureClientButton;

        public AnywhereLaunchStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/Anywhere/AnywhereLaunchStep")
        {
            _templateContent = new DeploymentStepTemplate.Builder(Strings.AnywherePageLaunchTitle, Strings.AnywherePageLaunchDescription)
                 .WithoutBaseButtons()
                 .Build(container);

            LocalizeText();

            _gameLiftClientSettingsLoader = new GameLiftClientSettingsLoader(_templateContent.StatusBox);
            LoadGameLiftClientSettings();

            _launchServerButton = container.Q<Button>("AnywherePageLaunchServerButton");
            _launchServerButton.RegisterCallback<ClickEvent>(_ =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server,
                EditorUserBuildSettings.selectedStandaloneTarget);
                EditorApplication.EnterPlaymode(); 
            });

            _configureClientButton = container.Q<Button>("AnywherePageConfigureClientButton");
            _configureClientButton.RegisterCallback<ClickEvent>(_ =>
            {
                _gameLiftClientSettings.ConfigureAnywhereClientSettings();
                _stateManager.OnClientSettingsChanged?.Invoke();
            });

            UpdateGUI();
        }

        protected sealed override Task StartOrResumeStep()
        {
            CompleteStep();
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() { }

        protected sealed override void UpdateGUI()
        {
            _container.SetEnabled(_stateManager.IsBootstrapped());

            bool isComputeRegistered = true;

            bool isClientConfigured = _gameLiftClientSettings && _gameLiftClientSettings.IsGameLiftAnywhere;
            bool isConfigureClientEnabled = isComputeRegistered && !isClientConfigured && _gameLiftClientSettings;

            _configureClientButton.SetEnabled(isConfigureClientEnabled);

            if (isConfigureClientEnabled)
            {
                _configureClientButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _configureClientButton.RemoveFromClassList(_primaryButtonClassName);
            }

            bool isLaunchServerEnabled = isComputeRegistered && isClientConfigured;
            _launchServerButton.SetEnabled(isLaunchServerEnabled);

            if (isLaunchServerEnabled)
            {
                _launchServerButton.AddToClassList(_primaryButtonClassName);
            }
            else
            {
                _launchServerButton.RemoveFromClassList(_primaryButtonClassName);
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageConfigureClientLabel", Strings.AnywherePageConfigureClientLabel);
            l.SetElementText("AnywherePageConfigureClientButton", Strings.AnywherePageConfigureClientButton);
            l.SetElementText("AnywherePageLaunchServerLabel", Strings.AnywherePageLaunchServerLabel);
            l.SetElementText("AnywherePageLaunchServerButton", Strings.AnywherePageLaunchServerButton);
            l.SetElementText("AnywherePageLaunchClientLabel", Strings.AnywherePageLaunchClientLabel);
            l.SetElementText("AnywherePageLaunchClientDescription", Strings.AnywherePageLaunchClientDescription);
        }

        private void LoadGameLiftClientSettings()
        {
            _gameLiftClientSettings = _gameLiftClientSettingsLoader.LoadAsset();
            _container.schedule.Execute(() => {
                LoadGameLiftClientSettings();
                UpdateGUI();
            }).StartingIn(RefreshUIMilliseconds);
        }
    }
}
