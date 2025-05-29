// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Build;
using AmazonGameLift.Runtime;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace AmazonGameLift.Editor
{
    public enum ContainerSteps
    {
        ConfigureDCIStep,
        CreateECRRepoStep,
        PushImageStep,
        ConfigureCGDStep,
        CreateContainerFleetStep
    }

    public class ContainersPage : StatefulInput
    {
        private const string _primaryButtonClassName = "button--primary";
        private const int RefreshUIMilliseconds = 2000;
        private readonly VisualElement _container;
        private readonly VisualElement _launchBarContainer;
        private readonly VisualElement questionaireContainer;
        private readonly VisualElement profileTableContainer;
        private readonly VisualElement questionaireFoldoutContainer;
        private readonly VisualElement profileTableFoldoutContainer;
        private readonly VisualElement controlBarContainer;
        private readonly Foldout questionnaireFoldout;
        private readonly Foldout profileTableFoldout;
        private readonly VisualElement userInputContainer;
        private readonly VisualElement userInputFoldoutContainer;
        private readonly Foldout userInputFoldout;
        private readonly StateManager _stateManager;
        private readonly ContainersUserInput _containersUserInput;
        private readonly ContainersUserInput _containersUserInputFoldout;
        private readonly ContainersQuestionnaire _containersQuestionnaire;
        private readonly ContainersQuestionnaire _containersQuestionnaireFoldout;
        private readonly Label userInputFoldoutTitle;
        private readonly Label questionnaireFoldoutTitle;
        private readonly Label profileTableFoldoutTitle;
        private GameLiftClientSettings _gameLiftClientSettings;
        private GameLiftClientSettingsLoader _gameLiftClientSettingsLoader;
        private StatusBox _statusBox;
        private StatusBox _launchStatusBox;
        private StatusBox _containersUnsupportedStatusBox;
        private StatusBox _missingWslDockerStatusBox;
        private StatusBox _deploymentNoticeStatusBox;
        private Button _launchServerButton;
        private Button _configureClientButton;
        private Button _launchClientButton;
        private Button _deployContainerFleetButton;
        private Button _resetDeploymentButton;
        private Button _viewFleetOnConsoleButton;
        private Action _startDeploymentAction;
        private bool _isDockerInstalled;
        private ContainersDeploymentSettings _deploymentSettings;
        
        private readonly Dictionary<ContainerScenarios, ContainerSteps[]> _stepsByQuestionaireScenario = new Dictionary<ContainerScenarios, ContainerSteps[]>
        {
            {  ContainerScenarios.NoContainerImageNoExistingEcrRepo, new ContainerSteps[] {
                ContainerSteps.ConfigureDCIStep,
                ContainerSteps.CreateECRRepoStep,
                ContainerSteps.PushImageStep,
                ContainerSteps.ConfigureCGDStep,
                ContainerSteps.CreateContainerFleetStep,
            } },
            {  ContainerScenarios.NoContainerImageUseExistingEcrRepo, new ContainerSteps[] {
                ContainerSteps.ConfigureDCIStep,
                ContainerSteps.PushImageStep,
                ContainerSteps.ConfigureCGDStep,
                ContainerSteps.CreateContainerFleetStep,
            } },
            {  ContainerScenarios.HaveContainerImageInDocker, new ContainerSteps[] {
                ContainerSteps.CreateECRRepoStep,
                ContainerSteps.PushImageStep,
                ContainerSteps.ConfigureCGDStep,
                ContainerSteps.CreateContainerFleetStep,
            } },
            {  ContainerScenarios.HaveContainerImageInEcr, new ContainerSteps[] {
                ContainerSteps.ConfigureCGDStep,
                ContainerSteps.CreateContainerFleetStep,

            } },
        };
        
        private Dictionary<ContainerSteps, ContainerStepComponent> _containerStepsComponentsMap = new Dictionary<ContainerSteps, ContainerStepComponent>();

        private List<ProgressBarStepComponent> progressBarSteps = new List<ProgressBarStepComponent>();

        public ContainersPage(VisualElement container, StateManager stateManager)
        {  
            _container = container;
            _stateManager = stateManager;
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/ContainersPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _deploymentSettings = DeploymentSettingsFactory.CreateContainerDeploymentSettings(_stateManager);

            container.Add(uxml);
            LocalizeText();
            SetupStatusBoxes();

            _stateManager.OnUserProfileUpdated += UpdateStatusBoxes;
            _stateManager.OnUserProfileUpdated += DisableUIIfNotSetup;

            _deployContainerFleetButton = container.Q<Button>("DeployContainerFleetButton");
            _resetDeploymentButton = container.Q<Button>("ResetDeploymentButton");
            _viewFleetOnConsoleButton = container.Q<Button>("AWSConsoleButton");

            // Profile table
            profileTableContainer = container.Q<VisualElement>("ProfileTableContainer");

            //Profile table foldout
            profileTableFoldoutTitle = container.Q<Label>("ProfileTableFoldoutTitle");
            profileTableFoldoutContainer = container.Q<VisualElement>("ProfileTableFoldoutContainer");
            profileTableFoldout = container.Q<Foldout>("ProfileTableFoldout");

            InitializeInputFoldout(profileTableFoldout, profileTableFoldoutTitle);

            //Questionnaire
            questionaireContainer = container.Q<VisualElement>("ContainerQuestionnaireContainer");
            _containersQuestionnaire = new ContainersQuestionnaire(questionaireContainer, stateManager, true);

            // Questionnaire foldout
            questionnaireFoldoutTitle = container.Q<Label>("QuestionnaireFoldoutTitle");
            questionaireFoldoutContainer = container.Q<VisualElement>("ContainerQuestionnaireFoldoutContainer");
            questionnaireFoldout = container.Q<Foldout>("ContainerQuestionnaireFoldout");
            _containersQuestionnaireFoldout = new ContainersQuestionnaire(questionnaireFoldout, stateManager, false);
            _containersQuestionnaireFoldout.PopulateContent();

            InitializeInputFoldout(questionnaireFoldout, questionnaireFoldoutTitle);

            // User input
            userInputContainer = container.Q<VisualElement>("ConfigureImageDeploymentContainer");
            _containersUserInput = new ContainersUserInput(userInputContainer, stateManager, true);
            _containersUserInput.OnValidationEvent += ShowHideDeployButton;
            
            // User input foldout
            userInputFoldoutTitle = container.Q<Label>("UserInputFoldoutTitle");
            userInputFoldoutContainer = container.Q<VisualElement>("ContainerUserInputFoldoutContainer");
            userInputFoldout = container.Q<Foldout>("ContainerUserInputFoldout");
            _containersUserInputFoldout = new ContainersUserInput(userInputFoldout, stateManager, false);
            _containersUserInputFoldout.PopulateContent();

            InitializeInputFoldout(userInputFoldout, userInputFoldoutTitle);

            // Control bar
            controlBarContainer = container.Q<VisualElement>("ContainerControlBar");

            // Launch bar
            _launchBarContainer = container.Q<VisualElement>("ContainersLaunchBar");

            // Containers Unsupported Warning
            _containersUnsupportedStatusBox = container.Q<StatusBox>("RegionUnsupportedStatusBox");
            var unsupportedRegionWarning =
                string.Format(new TextProvider().Get(Strings.ContainersPageRegionUnsupportedStatusBoxTemplate),
                    _stateManager.Region);
            _containersUnsupportedStatusBox.Show(StatusBox.StatusBoxType.Warning, unsupportedRegionWarning,
                externalButtonLink: Urls.SupportedContainersRegions, externalButtonText: "View supported regions");

            _isDockerInstalled = IsDockerInstalled();

            _missingWslDockerStatusBox = container.Q<StatusBox>("MissingWslDockerStatusBox");
            _missingWslDockerStatusBox.Show(StatusBox.StatusBoxType.Warning, Strings.ContainersPageMissingWslDockerStatusBoxText,
    externalButtonLink: Urls.InstallDockerEngine, externalButtonText: "Install Docker Engine");
            _missingWslDockerStatusBox.HideCloseButton();

            _deploymentNoticeStatusBox = container.Q<StatusBox>("DeploymentNoticeStatusBox");
            _deploymentNoticeStatusBox.Show(StatusBox.StatusBoxType.Info, Strings.ContainersPageDeploymentNoticeStatusBoxText);
            ShowHideNotices();

            // Initialize all the steps
            foreach (ContainerSteps step in getAllSteps())
            {
                InitializeStep(step);
            }

            container.Q<Button>("ManageCredentialsButton")
                .RegisterCallback<ClickEvent>(_ => EditorMenu.OpenAccountProfilesTab());

            container.Q<Button>("UpdateDeploymentButton")
                .RegisterCallback<ClickEvent>(_ => OpenUpdateContainersFleetPopup());

            stateManager.OnContainerQuestionnaireScenarioChanged += UpdateGUI;

            stateManager.OnContainersDeploymentStatusChanged += ShowHideLaunchBar;

            _gameLiftClientSettingsLoader = new GameLiftClientSettingsLoader();
            LoadGameLiftClientSettings();

            _launchClientButton = container.Q<Button>("StartClientButton");
            _launchClientButton.RegisterCallback<ClickEvent>(_ =>
            {
                _gameLiftClientSettings.ConfigureContainersClientSettings(_stateManager.Region, _deploymentSettings.CurrentStackInfo.ApiGatewayEndpoint, _deploymentSettings.CurrentStackInfo.UserPoolClientId);
                _stateManager.OnClientSettingsChanged?.Invoke();
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone,
                    EditorUserBuildSettings.selectedStandaloneTarget);
                EditorApplication.EnterPlaymode();
            });


            DisableUIIfNotSetup();
            UpdateGUI();
            UpdateStatusBoxes();
        }

        private void InitializeInputFoldout(Foldout foldout, VisualElement foldoutTitle)
        {
            foldout.value = false;
            // Run the OnClick event once to ensure accurate UI based on state
            OnInputFoldoutClick(foldout, foldoutTitle);
            foldout.RegisterCallback<ClickEvent>(_ => OnInputFoldoutClick(foldout, foldoutTitle));
        }

        private void OnInputFoldoutClick(Foldout foldout, VisualElement foldoutTitle)
        {
            // The extra title is outside the foldout so it should be visible iff the foldout is closed.
            ShowHide(foldoutTitle, !foldout.value);
        }

        private void LoadGameLiftClientSettings()
        {
            _gameLiftClientSettings = _gameLiftClientSettingsLoader.LoadAsset();
            _container.schedule.Execute(() => {
                LoadGameLiftClientSettings();
                UpdateGUI();
            }).StartingIn(RefreshUIMilliseconds);
        }

        private IEnumerable<ContainerSteps> getAllSteps()
        {
            return Enum.GetValues(typeof(ContainerSteps)).Cast<ContainerSteps>();
        }

        private void HideAllSteps()
        {
            foreach (ContainerSteps step in getAllSteps())
            {
                Hide(getElementByStep(step));
            }
        }

        private void ShowHideDeployButton()
        {
            if (_stateManager.ContainerDeploymentInProgress)
            {
                _deployContainerFleetButton.AddToClassList("hidden");
                _resetDeploymentButton.RemoveFromClassList("hidden");
                CollapseUserInput();
            }
            else
            {
                _deployContainerFleetButton.RemoveFromClassList("hidden");
                if (_containersUserInput.AllInputsValid())
                {
                    _deployContainerFleetButton.SetEnabled(true);
                }
                else
                {
                    _deployContainerFleetButton.SetEnabled(false);
                }
                _resetDeploymentButton.AddToClassList("hidden");
                ShowUserInput();
            }
        }

        private void ShowHideNotices()
        {
            if (_stateManager.ContainerDeploymentInProgress || !_stateManager.IsBootstrapped() || !_stateManager.IsInContainersRegion())
            {
                _missingWslDockerStatusBox.AddToClassList("hidden");
                _deploymentNoticeStatusBox.AddToClassList("hidden");
            }
            else
            {
                if (!_isDockerInstalled && _stateManager.ContainerQuestionnaireScenario != ContainerScenarios.HaveContainerImageInEcr)
                {
                    _missingWslDockerStatusBox.RemoveFromClassList("hidden");
                }
                else
                {
                    _missingWslDockerStatusBox.AddToClassList("hidden");
                }

                _deploymentNoticeStatusBox.RemoveFromClassList("hidden");
            }
        }

        private void ShowHideLaunchBar()
        {
            // show launch bar logic
            if (_stateManager.ContainerDeploymentInProgress)
            {
                _launchBarContainer.RemoveFromClassList("hidden");
            }
            else
            {
                _launchBarContainer.AddToClassList("hidden");
            }

            // enable launch bar logic
            if (_stateManager.ContainersDeploymentComplete)
            {
                _launchBarContainer.SetEnabled(true);
            }
            else
            {
                _launchBarContainer.SetEnabled(false);
            }
        }

        private void CollapseUserInput()
        {
            profileTableContainer.AddToClassList("hidden");
            questionaireContainer.AddToClassList("hidden");
            userInputContainer.AddToClassList("hidden");
            userInputFoldoutContainer.RemoveFromClassList("hidden");
            profileTableFoldoutContainer.RemoveFromClassList("hidden");
            questionaireFoldoutContainer.RemoveFromClassList("hidden");

            _containersUserInputFoldout.PopulateContent();
            _containersQuestionnaireFoldout.PopulateContent();

        }

        private void ShowUserInput()
        {
            profileTableContainer.RemoveFromClassList("hidden");
            questionaireContainer.RemoveFromClassList("hidden");
            userInputContainer.RemoveFromClassList("hidden");
            userInputFoldoutContainer.AddToClassList("hidden");
            profileTableFoldoutContainer.AddToClassList("hidden");
            questionaireFoldoutContainer.AddToClassList("hidden");
        }

        private void InitializeStep(ContainerSteps step)
        {
            var stepContainer = _container.Q(step.ToString());
            switch (step)
            {
                case ContainerSteps.ConfigureDCIStep:
                    _containerStepsComponentsMap.Add(step, new ConfigureDCIStep(stepContainer, _stateManager));
                    return;
                case ContainerSteps.CreateECRRepoStep:
                    _containerStepsComponentsMap.Add(step, new CreateECRRepoStep(stepContainer, _stateManager));
                    return;
                case ContainerSteps.PushImageStep:
                    if (_isDockerInstalled)
                    {
                        _containerStepsComponentsMap.Add(step, new PushImageAutoStep(stepContainer, _stateManager));
                    } 
                    else
                    {
                        _containerStepsComponentsMap.Add(step, new PushImageManualStep(stepContainer, _stateManager));
                    }
                    return;
                case ContainerSteps.ConfigureCGDStep:
                    _containerStepsComponentsMap.Add(step, new ConfigureCGDStep(stepContainer, _stateManager, _deploymentSettings));
                    return;
                case ContainerSteps.CreateContainerFleetStep:
                    _containerStepsComponentsMap.Add(step, new CreateContainerFleetStep(stepContainer, _stateManager, _deploymentSettings));
                    return;
            }
        }

        private VisualElement getElementByStep(ContainerSteps step)
        {
            return _container.Q<VisualElement>(step.ToString());
        }

        protected sealed override void UpdateGUI()
        {
            ShowHideLaunchBar();
            ShowHideDeployButton();
            ShowHideNotices();
            SetupSteps();
            _deployContainerFleetButton.RegisterCallback<ClickEvent>(_ =>
            {
                // sanity input validation check
                if (_containersUserInput.AllInputsValid())
                {
                    questionnaireFoldout.value = false;
                    userInputFoldout.value = false;
                    profileTableFoldout.value = false;

                    _containersUserInput.SaveValues();
                    _stateManager.ContainerDeploymentInProgress = true;
                    ShowHideDeployButton();
                    ShowHideNotices();
                    _startDeploymentAction();
                }
                else
                {
                    ShowHideDeployButton();
                }
            });

            _resetDeploymentButton.RegisterCallback<ClickEvent>(_ =>
            {
                OpenResetPopup();
            });

            _viewFleetOnConsoleButton.RegisterCallback<ClickEvent>(_ =>
            {
                Application.OpenURL(string.Format(Urls.AwsContainerFleetsConsole, _stateManager.Region));
            });
        }

        private void DisableUIIfNotSetup()
        {
            bool bootstrapped = _stateManager.IsBootstrapped();
            bool containers = _stateManager.IsInContainersRegion();

            questionaireContainer.SetEnabled(bootstrapped && containers);
            userInputContainer.SetEnabled(bootstrapped && containers);
            controlBarContainer.SetEnabled(bootstrapped && containers);
            if (containers)
            {
                Hide(_containersUnsupportedStatusBox);
            }
            else
            {
                Show(_containersUnsupportedStatusBox);
            }
        }

        private void SetupSteps()
        {
            HideAllSteps();
            if (_stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion())
            {
                var questionnaireScenario = _stateManager.ContainerQuestionnaireScenario;
                ContainerSteps[] stepsToShow = _stepsByQuestionaireScenario[questionnaireScenario];
                progressBarSteps = new();
                foreach (ContainerSteps step in stepsToShow)
                {
                    Show(getElementByStep(step));
                    progressBarSteps.Add(_containerStepsComponentsMap[step]);
                }
                _startDeploymentAction = ProgressFlowContainer.SetupSteps(progressBarSteps);
            }
        }

        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("ContainersStatusBox");
        }

        private bool IsDockerInstalled()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "-v";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                process.Start();

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                // It will hit an exception if docker is not installed or in the PATH.
                return false;
            }
        }

        private void OpenResetPopup()
        {
            var popup = ScriptableObject.CreateInstance<ResetPopup>();
            popup.Init(_stateManager.Region);
            popup.OnConfirm += ResetDeployment;
            popup.ShowModalUtility();
        }

        private void ResetDeployment()
        {
            userInputFoldoutTitle.RemoveFromClassList("hidden");
            questionnaireFoldoutTitle.RemoveFromClassList("hidden");
            profileTableFoldoutTitle.RemoveFromClassList("hidden");
            _stateManager.ContainerDeploymentInProgress = false;
            _stateManager.ContainersDeploymentComplete = false;
            _deploymentSettings = DeploymentSettingsFactory.CreateContainerDeploymentSettings(_stateManager);
            ShowHideDeployButton();
            ShowHideNotices();
            ResetDeploymentSteps();
            ResetUserInput();
        }

        private void OpenUpdateContainersFleetPopup()
        {
            var popup = ScriptableObject.CreateInstance<ContainersFleetUpdatePopup>();
            popup.Init(_stateManager);
            popup.OnConfirm += UpdateDeployment;
            popup.ShowModalUtility();
        }

        private void UpdateDeployment()
        {
            questionnaireFoldout.value = false;
            userInputFoldout.value = false;
            profileTableFoldout.value = false;

            ResetDeploymentSteps();
            _stateManager.ContainerDeploymentInProgress = true;
            ShowHideDeployButton();
            _startDeploymentAction = ProgressFlowContainer.SetupSteps(progressBarSteps);
            _startDeploymentAction();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("WhatIsContainerImage", Strings.WhatIsContainerImage);
            l.SetElementText("DockerDocumentation", Strings.DockerDocumentation);
        }

        private void UpdateStatusBoxes()
        {
            if (!_stateManager.IsBootstrapped())
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.ManagedEC2StatusBoxNotBootstrappedWarning);
            }
            else
            {
                _statusBox.Close();
            }
        }

        private void ResetUserInput()
        {
            // Reset ALL user input EXCEPT for default values
            _stateManager.ContainerGameServerBuildPath = null;
            _stateManager.ContainerGameServerExecutable = null;
            _stateManager.ContainerDockerImageId = null;
            _stateManager.ContainerECRRepositoryName = null;
            _stateManager.ContainerECRRepositoryUri = null;
            _stateManager.ContainerECRImageId = null;

            // Reset default values to their defaults
            _stateManager.ContainerPortRange = ContainersUserInputValidation.DEFAULT_PORT_RANGE;
            _stateManager.ContainerTotalMemory = ContainersUserInputValidation.DEFAULT_MEMORY_LIMIT;
            _stateManager.ContainerTotalVcpu = ContainersUserInputValidation.DEFAULT_VCPU_LIMIT;
            _stateManager.ContainerImageTag = ContainersUserInputValidation.DEFAULT_IMAGE_TAG;
            _stateManager.ContainerGameName = ContainersUserInputValidation.DEFAULT_GAME_NAME;

            // Setup inputs in User Input container
            _containersUserInput.SetupInputsValues();

        }

        private void ResetDeploymentSteps()
        {
            ProgressBarStepComponent _firstStep = progressBarSteps.First();
            _firstStep.Reset();
        }
    }
}