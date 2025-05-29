// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using AmazonGameLiftPlugin.Core.ContainerManagement.Models;
using Amazon.ECR.Model;

namespace AmazonGameLift.Editor
{
    internal class ContainersUserInput : StatefulInput
    {
        private readonly VisualElement _container;
        private readonly DeploymentScenariosInput _deploymentScenarioInput;
        private VisualElement _containerServerBuildInputRow;
        private VisualElement _containerServerExecutableInputRow;
        private VisualElement _selectECRRepositoryDropdownRow;
        private VisualElement _containerECRImageDropdownRow;
        private VisualElement _dockerImageIDInputRow;
        private VisualElement _imageTagInputRow;
        private TextField _containerGameServerBuildInput;
        private TextField _containerGameServerExecutableInput;
        private TextField _dockerImageIDInput;
        private TextField _containerPortRangeInput;
        private TextField _containerTotalMemoryInput;
        private TextField _containerTotalVcpuInput;
        private TextField _containerImageTagInput;
        private TextField _containerGameNameInput;
        private Label _invalidPortRangeLabel;
        private Label _invalidMemoryLimitLabel;
        private Label _invalidVcpuLimitLabel;
        private Label _invalidGameNameLabel;
        private Label _invalidContainerImageTagLabel;
        private Label _invalidGameServerBuildLabel;
        private Label _invalidGameServerExecutableLabel;
        private Label _invalidECRRepoLabel;
        private Label _invalidECRImageLabel;
        private Label _invalidDockerImageIDLabel;
        private DropdownField _selectECRRepositoryDropdownContainer;
        private DropdownField _selectECRImageDropdownContainer;
        private Button _serverFolderButton;
        private Button _serverFileButton;
        private DescribeECRRepositoriesResponse _ecrRepositories;
        private Foldout _defaultPropertiesFoldout;

        private readonly Dictionary<string, string> _ecrRepoNameUriMap = new Dictionary<string, string>();

        private readonly StateManager _stateManager;
        private readonly CoreApi _coreApi;
        private readonly ContainersUserInputValidation _inputValidator;

        public Action<DeploymentScenarios> OnValueChanged;
        public Action OnValidationEvent;

        public ContainersUserInput(VisualElement container, StateManager stateManager, bool enabled)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/Containers/ContainersUserInput");
            container.Add(uxml.Instantiate());

            _container = container;
            _stateManager = stateManager;
            _coreApi = CoreApi.SharedInstance;

            ReferenceComponents();
            _inputValidator = new ContainersUserInputValidation(GetErrorMessageMappings(), GetInputMappings());
            RegisterCallbacks();

            var scenarioContainer = container.Q("ContainersDeploymentScenarioContainer");
            _deploymentScenarioInput =
                new DeploymentScenariosInput(scenarioContainer, _stateManager.ContainerDeploymentScenario,
                _stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion(), stateManager, () => _stateManager.ContainerDeploymentScenario);
            _deploymentScenarioInput.OnValueChanged += value => { _stateManager.ContainerDeploymentScenario = value; };
            _stateManager.OnContainerQuestionnaireScenarioChanged += UpdateGUI;
            _stateManager.OnUserProfileUpdated += UpdateGUI;

            SetupInputsValues();

            _defaultPropertiesFoldout.value = false;

            SetEnabled(enabled);

            _inputValidator.OnValidationEvent += TriggerValidation;

            LocalizeText();
            UpdateGUI();
        }


        private void ReferenceComponents()
        {

            _containerServerBuildInputRow = _container.Q<VisualElement>("ContainerServerBuildInputRow");
            _containerServerExecutableInputRow = _container.Q<VisualElement>("ContainerServerExecutableInputRow");
            _selectECRRepositoryDropdownRow = _container.Q<VisualElement>("SelectECRRepositoryDropdownRow");
            _containerECRImageDropdownRow = _container.Q<VisualElement>("ContainerECRImageDropdownRow");
            _dockerImageIDInputRow = _container.Q<VisualElement>("DockerImageIDInputRow");
            _imageTagInputRow = _container.Q<VisualElement>("ImageTagInputInputRow");

            _containerGameServerBuildInput = _container.Q<TextField>("ContainerGameServerBuildInput");
            _containerGameServerExecutableInput = _container.Q<TextField>("ContainerGameServerExecutableInput");
            _dockerImageIDInput = _container.Q<TextField>("DockerImageIDInput");
            _containerPortRangeInput = _container.Q<TextField>("ContainerPortRangeInput");
            _containerTotalMemoryInput = _container.Q<TextField>("ContainerTotalMemoryInput");
            _containerTotalVcpuInput = _container.Q<TextField>("ContainerTotalVcpuInput");
            _containerImageTagInput = _container.Q<TextField>("ContainerImageTagInput");
            _containerGameNameInput = _container.Q<TextField>("ContainerGameNameInput");

            _invalidPortRangeLabel = _container.Q<Label>("ContainerConnectionPortRangeInvalidMessage");
            _invalidMemoryLimitLabel = _container.Q<Label>("ContainerTotalMemoryInvalidMessage");
            _invalidVcpuLimitLabel = _container.Q<Label>("ContainerTotalVcpuInvalidMessage");
            _invalidGameNameLabel = _container.Q<Label>("ContainerGameNameInvalidMessage");
            _invalidContainerImageTagLabel = _container.Q<Label>("ContainerImageTagInvalidMessage");
            _invalidDockerImageIDLabel = _container.Q<Label>("DockerImageIDInvalidMessage");
            _invalidECRImageLabel = _container.Q<Label>("SelectECRImageDropdownInvalidMessage");
            _invalidECRRepoLabel = _container.Q<Label>("SelectECRRepositoryDropdownInvalidMessage");
            _invalidGameServerExecutableLabel = _container.Q<Label>("ContainerGameServerExecutableInputInvalidMessage");
            _invalidGameServerBuildLabel = _container.Q<Label>("ContainerGameServerBuildInputInvalidMessage");


            _selectECRRepositoryDropdownContainer = _container.Q<DropdownField>("SelectECRRepositoryDropdown");
            _selectECRImageDropdownContainer = _container.Q<DropdownField>("SelectECRImageDropdown");

            _serverFileButton = _container.Q<Button>("ContainerGameServerExecutableButton");
            _serverFolderButton = _container.Q<Button>("ContainerGameServerBuildButton");

            _defaultPropertiesFoldout = _container.Q<Foldout>("DefaultSettingsFoldout");
        }

        private void RegisterCallbacks()
        {
            // buttons
            _serverFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFilePanel("Game Server Build Executable Path",
                    _stateManager.ContainerGameServerBuildPath, "");
                _containerGameServerExecutableInput.value = value;
            });
            _serverFolderButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFolderPanel("Game Server Build Directory Path",
                    Application.dataPath, "");
                _containerGameServerBuildInput.value = value;
            });

            // dropdown selections
            _selectECRRepositoryDropdownContainer.RegisterValueChangedCallback(evt => {
                _stateManager.ContainerECRRepositoryName = evt.newValue;
                _stateManager.ContainerECRRepositoryUri = evt.newValue != null ? _ecrRepoNameUriMap[evt.newValue] : evt.newValue;
                // repopulate or clear images when ecr repo changes
                PopulateECRImagesDropdown(evt.newValue);
            });
            _selectECRImageDropdownContainer.RegisterValueChangedCallback(evt => {
                _stateManager.ContainerECRImageId = evt.newValue;
            });

            // input validation actions
            _inputValidator.RegisterValidationCallbacks();
        }

        private Dictionary<ContainersUserInputType, Label> GetErrorMessageMappings()
        {
            var errorMessagesMappings = new Dictionary<ContainersUserInputType, Label>
            {
                { ContainersUserInputType.ConnectionPortRangeInput, _invalidPortRangeLabel },
                { ContainersUserInputType.ContainerImageTagInput, _invalidContainerImageTagLabel },
                { ContainersUserInputType.DockerImageInput, _invalidDockerImageIDLabel },
                { ContainersUserInputType.EcrImageDropdown, _invalidECRImageLabel },
                { ContainersUserInputType.EcrRepositoryDropdown, _invalidECRRepoLabel },
                { ContainersUserInputType.GameNameInput, _invalidGameNameLabel },
                { ContainersUserInputType.GameServerExecutableInput, _invalidGameServerExecutableLabel },
                { ContainersUserInputType.GameServerFolderInput, _invalidGameServerBuildLabel },
                { ContainersUserInputType.MemoryLimitInput, _invalidMemoryLimitLabel },
                { ContainersUserInputType.VcpuLimitInput, _invalidVcpuLimitLabel }
            };
            return errorMessagesMappings;
        }

        private Dictionary<ContainersUserInputType, VisualElement> GetInputMappings()
        {
            var inputMappings = new Dictionary<ContainersUserInputType, VisualElement>
            {
                { ContainersUserInputType.ConnectionPortRangeInput, _containerPortRangeInput },
                { ContainersUserInputType.ContainerImageTagInput, _containerImageTagInput },
                { ContainersUserInputType.DockerImageInput, _dockerImageIDInput },
                { ContainersUserInputType.EcrImageDropdown, _selectECRImageDropdownContainer },
                { ContainersUserInputType.EcrRepositoryDropdown, _selectECRRepositoryDropdownContainer },
                { ContainersUserInputType.GameNameInput, _containerGameNameInput },
                { ContainersUserInputType.GameServerExecutableInput, _containerGameServerExecutableInput },
                { ContainersUserInputType.GameServerFolderInput, _containerGameServerBuildInput },
                { ContainersUserInputType.MemoryLimitInput, _containerTotalMemoryInput },
                { ContainersUserInputType.VcpuLimitInput, _containerTotalVcpuInput }
            };
            return inputMappings;
        }

        protected sealed override void UpdateGUI()
        {
            PopulateDeploymentInputs();
        }

        private void TriggerValidation()
        {
            OnValidationEvent?.Invoke();
        }

        public void SetEnabled(bool value)
        {
            _containerServerBuildInputRow.SetEnabled(value);
            _containerServerExecutableInputRow.SetEnabled(value);
            _selectECRRepositoryDropdownContainer.SetEnabled(value);
            _selectECRImageDropdownContainer.SetEnabled(value);
            _selectECRRepositoryDropdownRow.SetEnabled(value);
            _containerECRImageDropdownRow.SetEnabled(value);
            _dockerImageIDInputRow.SetEnabled(value);
            _imageTagInputRow.SetEnabled(value);
            _deploymentScenarioInput.SetEnabled(value);
            _containerGameNameInput.SetEnabled(value);
            _containerTotalVcpuInput.SetEnabled(value);
            _containerTotalMemoryInput.SetEnabled(value);
            _containerPortRangeInput.SetEnabled(value);
            _imageTagInputRow.SetEnabled(value);

        }

        private void HideAll()
        {
            Hide(_containerServerBuildInputRow);
            Hide(_containerServerExecutableInputRow);
            Hide(_selectECRRepositoryDropdownRow);
            Hide(_containerECRImageDropdownRow);
            Hide(_dockerImageIDInputRow);
            Hide(_imageTagInputRow);

            Hide(_invalidECRImageLabel);
            Hide(_invalidContainerImageTagLabel);
            Hide(_invalidPortRangeLabel);
            Hide(_invalidMemoryLimitLabel);
            Hide(_invalidVcpuLimitLabel);
            Hide(_invalidGameNameLabel);
            Hide(_invalidGameServerBuildLabel);
            Hide(_invalidGameServerExecutableLabel);
            Hide(_invalidECRRepoLabel);
            Hide(_invalidDockerImageIDLabel);
        }

        private void PopulateDeploymentInputs()
        {
            var deploymentScenario = _stateManager.ContainerQuestionnaireScenario;
            HideAll();
            List<ContainersUserInputType> activeInputs = new List<ContainersUserInputType>
            {
                ContainersUserInputType.VcpuLimitInput,
                ContainersUserInputType.MemoryLimitInput,
                ContainersUserInputType.GameNameInput,
                ContainersUserInputType.ConnectionPortRangeInput,
            };
            switch (deploymentScenario)
            {
                case ContainerScenarios.NoContainerImageNoExistingEcrRepo:
                    Show(_containerServerBuildInputRow);
                    Show(_containerServerExecutableInputRow);
                    Show(_imageTagInputRow);
                    activeInputs.Add(ContainersUserInputType.GameServerFolderInput);
                    activeInputs.Add(ContainersUserInputType.GameServerExecutableInput);
                    activeInputs.Add(ContainersUserInputType.ContainerImageTagInput);
                    break;
                case ContainerScenarios.NoContainerImageUseExistingEcrRepo:
                    Show(_containerServerBuildInputRow);
                    Show(_containerServerExecutableInputRow);
                    Show(_selectECRRepositoryDropdownRow);
                    Show(_imageTagInputRow);
                    activeInputs.Add(ContainersUserInputType.GameServerFolderInput);
                    activeInputs.Add(ContainersUserInputType.GameServerExecutableInput);
                    activeInputs.Add(ContainersUserInputType.ContainerImageTagInput);
                    activeInputs.Add(ContainersUserInputType.EcrRepositoryDropdown);

                    PopulateECRRepositoriesDropdown();

                    break;
                case ContainerScenarios.HaveContainerImageInDocker:
                    Show(_dockerImageIDInputRow);
                    Show(_imageTagInputRow);
                    activeInputs.Add(ContainersUserInputType.ContainerImageTagInput);
                    activeInputs.Add(ContainersUserInputType.DockerImageInput);
                    break;
                case ContainerScenarios.HaveContainerImageInEcr:
                    Show(_selectECRRepositoryDropdownRow);
                    Show(_containerECRImageDropdownRow);
                    activeInputs.Add(ContainersUserInputType.EcrRepositoryDropdown);
                    activeInputs.Add(ContainersUserInputType.EcrImageDropdown);

                    PopulateECRRepositoriesDropdown();
                    PopulateECRImagesDropdown(_stateManager.ContainerECRRepositoryName);
                    break;
            }
            _inputValidator.SetActiveInputs(activeInputs);
        }

        private void PopulateECRImagesDropdown(string repositoryName)
        {
            if (_stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion())
            {
                if (!string.IsNullOrEmpty(repositoryName))
                {
                    var images = _coreApi.ListECRImages(_stateManager.ProfileName, _stateManager.Region, repositoryName);
                    List<string> choices = images.ECRImages.Select(image => string.IsNullOrEmpty(image.ImageTag) ? image.ImageDigest : image.ImageTag).ToList();
                    _selectECRImageDropdownContainer.choices = choices;
                    if (!string.IsNullOrEmpty(_stateManager.ContainerECRImageId) && choices.Contains(_stateManager.ContainerECRImageId))
                    {
                        _selectECRImageDropdownContainer.value = _stateManager.ContainerECRImageId;
                        return;
                    }
                }
            }
            _selectECRImageDropdownContainer.value = null;
            _stateManager.ContainerECRImageId = null;
        }
        private void PopulateECRRepositoriesDropdown()
        {
            if (_stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion())
            {
                _ecrRepositories = _coreApi.DescribeECRRepositories(_stateManager.ProfileName, _stateManager.Region);
                if (_ecrRepositories != null && _ecrRepositories.ECRRepositories != null)
                {
                    List<string> choices = _ecrRepositories.ECRRepositories.Select(repository => repository.RepositoryName).ToList();
                    _selectECRRepositoryDropdownContainer.choices = choices;
                    foreach (Repository repo in _ecrRepositories.ECRRepositories)
                    {
                        if (!_ecrRepoNameUriMap.ContainsKey(repo.RepositoryName))
                        {
                            _ecrRepoNameUriMap.Add(repo.RepositoryName, repo.RepositoryUri);
                        }
                    }
                    if (!string.IsNullOrEmpty(_stateManager.ContainerECRRepositoryName) && choices.Contains(_stateManager.ContainerECRRepositoryName))
                    {
                        _selectECRRepositoryDropdownContainer.value = _stateManager.ContainerECRRepositoryName;
                        _stateManager.ContainerECRRepositoryUri = _ecrRepoNameUriMap[_stateManager.ContainerECRRepositoryName];
                        return;
                    }
                }
            }
            _selectECRRepositoryDropdownContainer.value = null;
            _stateManager.ContainerECRRepositoryUri = null;
            _stateManager.ContainerECRRepositoryName = null;
        }

        public void SetupInputsValues()
        {
            _containerGameServerBuildInput.value = _stateManager.ContainerGameServerBuildPath;
            _containerGameServerExecutableInput.value = _stateManager.ContainerGameServerExecutable;
            _dockerImageIDInput.value = _stateManager.ContainerDockerImageId;

            _containerPortRangeInput.value = string.IsNullOrEmpty(_stateManager.ContainerPortRange) ? ContainersUserInputValidation.DEFAULT_PORT_RANGE : _stateManager.ContainerPortRange;
            _containerTotalMemoryInput.value = string.IsNullOrEmpty(_stateManager.ContainerTotalMemory) ? ContainersUserInputValidation.DEFAULT_MEMORY_LIMIT : _stateManager.ContainerTotalMemory;
            _containerTotalVcpuInput.value = string.IsNullOrEmpty(_stateManager.ContainerTotalVcpu) ? ContainersUserInputValidation.DEFAULT_VCPU_LIMIT : _stateManager.ContainerTotalVcpu;
            _containerImageTagInput.value = string.IsNullOrEmpty(_stateManager.ContainerImageTag) ? ContainersUserInputValidation.DEFAULT_IMAGE_TAG : _stateManager.ContainerImageTag;
            _containerGameNameInput.value = string.IsNullOrEmpty(_stateManager.ContainerGameName) ? ContainersUserInputValidation.DEFAULT_GAME_NAME : _stateManager.ContainerGameName;
        }

        public void PopulateContent()
        {
            SetupInputsValues();
            PopulateDeploymentInputs();
            _deploymentScenarioInput.PopulateContent();
        }

        public void SaveValues()
        {
            _stateManager.ContainerPortRange = _containerPortRangeInput.value;
            _stateManager.ContainerTotalMemory = _containerTotalMemoryInput.value;
            _stateManager.ContainerTotalVcpu = _containerTotalVcpuInput.value;
            _stateManager.ContainerGameName = _containerGameNameInput.value;

            switch (_stateManager.ContainerQuestionnaireScenario)
            {
                case ContainerScenarios.NoContainerImageNoExistingEcrRepo:
                    _stateManager.ContainerImageTag = _containerImageTagInput.value;
                    _stateManager.ContainerGameServerBuildPath = _containerGameServerBuildInput.value;
                    _stateManager.ContainerGameServerExecutable = _containerGameServerExecutableInput.value;
                    break;
                case ContainerScenarios.NoContainerImageUseExistingEcrRepo:
                    _stateManager.ContainerImageTag = _containerImageTagInput.value;
                    _stateManager.ContainerGameServerBuildPath = _containerGameServerBuildInput.value;
                    _stateManager.ContainerGameServerExecutable = _containerGameServerExecutableInput.value;
                    _stateManager.ContainerECRRepositoryName = _selectECRRepositoryDropdownContainer.value;
                    break;
                case ContainerScenarios.HaveContainerImageInDocker:
                    _stateManager.ContainerDockerImageId = _dockerImageIDInput.value;
                    _stateManager.ContainerImageTag = _containerImageTagInput.value;
                    break;
                case ContainerScenarios.HaveContainerImageInEcr:
                    _stateManager.ContainerECRRepositoryName = _selectECRRepositoryDropdownContainer.value;
                    _stateManager.ContainerECRImageId = _selectECRImageDropdownContainer.value;
                    break;
            }
        }

        public bool AllInputsValid()
        {
            return _inputValidator.AllInputsValid();
        }
       

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("ContainerGameServerBuildLabel", Strings.ContainerGameServerBuildLabel);
            l.SetElementText("ContainerGameServerExecutableLabel", Strings.ContainerGameServerExecutableLabel);
            l.SetElementText("SelectECRRepositoryLabel", Strings.SelectECRRepositoryLabel);
            l.SetElementText("ContainerSelectImageLabel", Strings.ContainerSelectImageLabel);
            l.SetElementText("DockerImageIDLabel", Strings.DockerImageIDLabel);
            l.SetElementText("DefaultSettingsFoldout", Strings.DefaultSettings);
            l.SetElementText("DefaultSettingsDescription", Strings.DefaultSettingsDescription);
            l.SetElementText("ContainerConnectionPortRangeLabel", Strings.ContainerConnectionPortRangeLabel);
            l.SetElementText("ContainerTotalMemoryLabel", Strings.ContainerTotalMemoryLabel);
            l.SetElementText("ContainerTotalVcpuLabel", Strings.ContainerTotalVcpuLabel);
            l.SetElementText("ContainerImageTagLabel", Strings.ContainerImageTagLabel);
        }
    }


}
