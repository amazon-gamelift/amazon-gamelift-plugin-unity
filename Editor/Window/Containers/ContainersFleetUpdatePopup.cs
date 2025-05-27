// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.ECR.Model;
using AmazonGameLiftPlugin.Core.ContainerManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ContainersFleetUpdatePopup : EditorWindow
    {
        // Unity window fields
        private static VisualTreeAsset m_VisualTreeAsset;
        private VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();
        public Action OnConfirm;
        private const float MinPopupWidth = 1000f;
        private const float MinPopupHeight = 330f;
        private const float MaxPopupWidth = 2 * MinPopupWidth;
        private const float MaxPopupHeight = 3 * MinPopupHeight;

        // Visual elements
        private VisualElement _containerServerBuildInputRow;
        private VisualElement _containerServerExecutableInputRow;
        private VisualElement _containerECRImageDropdownRow;
        private VisualElement _dockerImageIDInputRow;
        private VisualElement _containerImageTagInputRow;

        // Common elements
        private StateManager _stateManager;
        private StatusBox _statusBox;
        private Button _updateButton;
        private Button _visitAwsConsoleButton;
        private Button _cancelButton;

        // Building docker
        private TextField _buildDirectoryInput;
        private TextField _buildExecutableInput;
        private Button _serverFolderButton;
        private Button _serverFileButton;

        // Using docker
        private TextField _dockerImageIdInput;

        // Using ECR repository
        private DropdownField _selectECRRepositoryDropdown;
        private DropdownField _selectECRImageDropdown;
        private TextField _containerImageTagInput;
        private readonly Dictionary<string, string> _ecrRepoNameUriMap = new Dictionary<string, string>();
        private DescribeECRRepositoriesResponse _ecrRepositories;

        // Deployment scenario
        private DeploymentScenariosInput _deploymentScenarioInput;
        private DeploymentScenarios _selectedDeploymentScenario;

        // Always present settings
        private TextField _containerPortRangeInput;
        private TextField _containerTotalMemoryInput;
        private TextField _containerTotalVcpuInput;
        private Label _invalidPortRangeLabel;
        private Label _invalidMemoryLimitLabel;
        private Label _invalidVcpuLimitLabel;
        private Label _invalidContainerImageTagLabel;
        private Label _invalidGameServerBuildLabel;
        private Label _invalidGameServerExecutableLabel;
        private Label _invalidECRRepoLabel;
        private Label _invalidECRImageLabel;
        private Label _invalidDockerImageIDLabel;

        private CoreApi _coreApi;

        private ContainersUserInputValidation _inputValidator;

        public void OnEnable()
        {
            _root = rootVisualElement;
            m_VisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Components/Containers/ContainersFleetUpdatePopup");
            _root.Add(m_VisualTreeAsset.Instantiate());
            titleContent = new GUIContent(_textProvider.Get(Strings.ContainersFleetUpdatePopupWindowTitle));
            maxSize = new Vector2(MaxPopupWidth, MaxPopupHeight);
            minSize = new Vector2(MinPopupWidth, MinPopupHeight);
        }

        public void Init(StateManager stateManager)
        {
            _stateManager = stateManager;
            _coreApi = CoreApi.SharedInstance;

            ReferenceComponents();
            _inputValidator = new ContainersUserInputValidation(GetErrorMessageMappings(), GetInputMappings());
            RegisterCallbacks();

            var scenarioContainer = _root.Q("ContainersDeploymentScenarios");
            _deploymentScenarioInput =
                new DeploymentScenariosInput(scenarioContainer, _stateManager.ContainerDeploymentScenario,
                _stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion(), _stateManager, () => _stateManager.ContainerDeploymentScenario);
            _deploymentScenarioInput.OnValueChanged += value => { _selectedDeploymentScenario = value; };

            PopulateComponents();
            LocalizeText();

            _inputValidator.OnValidationEvent += EnableDisableUpdateButton;
            EnableDisableUpdateButton();
        }


        private void ReferenceComponents()
        {
            _containerServerBuildInputRow = _root.Q<VisualElement>("ContainerServerBuildInputRow");
            _containerServerExecutableInputRow = _root.Q<VisualElement>("ContainerServerExecutableInputRow");
            _containerECRImageDropdownRow = _root.Q<VisualElement>("ContainerECRImageDropdownRow");
            _dockerImageIDInputRow = _root.Q<VisualElement>("DockerImageIDRow");
            _containerImageTagInputRow = _root.Q<VisualElement>("ContainerImageTagInputRow");

            _buildDirectoryInput = _root.Q<TextField>("ContainerGameServerBuildInput");
            _buildExecutableInput = _root.Q<TextField>("ContainerGameServerExecutableInput");
            _dockerImageIdInput = _root.Q<TextField>("DockerImageIDInput");
            _containerPortRangeInput = _root.Q<TextField>("ContainerPortRangeInput");
            _containerTotalMemoryInput = _root.Q<TextField>("ContainerTotalMemoryInput");
            _containerTotalVcpuInput = _root.Q<TextField>("ContainerTotalVcpuInput");
            _containerImageTagInput = _root.Q<TextField>("ContainerImageTagInput");

            _invalidPortRangeLabel = _root.Q<Label>("ContainerConnectionPortRangeInvalidMessage");
            _invalidMemoryLimitLabel = _root.Q<Label>("ContainerTotalMemoryInvalidMessage");
            _invalidVcpuLimitLabel = _root.Q<Label>("ContainerTotalVcpuInvalidMessage");
            _invalidContainerImageTagLabel = _root.Q<Label>("ContainerImageTagInvalidMessage");
            _invalidDockerImageIDLabel = _root.Q<Label>("DockerImageIDInvalidMessage");
            _invalidECRImageLabel = _root.Q<Label>("SelectECRImageDropdownInvalidMessage");
            _invalidECRRepoLabel = _root.Q<Label>("SelectECRRepositoryDropdownInvalidMessage");
            _invalidGameServerExecutableLabel = _root.Q<Label>("ContainerGameServerExecutableInputInvalidMessage");
            _invalidGameServerBuildLabel = _root.Q<Label>("ContainerGameServerBuildInputInvalidMessage");


            _selectECRRepositoryDropdown = _root.Q<DropdownField>("SelectECRRepositoryDropdown");
            _selectECRImageDropdown = _root.Q<DropdownField>("SelectECRImageDropdown");

            _serverFileButton = _root.Q<Button>("ContainerGameServerExecutableButton");
            _serverFolderButton = _root.Q<Button>("ContainerGameServerBuildButton");
            _visitAwsConsoleButton = _root.Q<Button>("ContainersFleetUpdatePopupVisitConsoleButton");
            _updateButton = _root.Q<Button>(Strings.ContainersFleetUpdatePopupUpdateButton);
            _cancelButton = _root.Q<Button>(Strings.ContainersFleetUpdatePopupCancelButton);

            _statusBox = _root.Q<StatusBox>("ContainersFleetUpdatePopupStatusBox");
            _statusBox.Show(StatusBox.StatusBoxType.Warning,
                 _textProvider.Get(Strings.ContainersFleetUpdateStatusBoxText), null, Urls.AwsFreeTier, _textProvider.Get(Strings.ContainersFleetUpdateStatusBoxButtonText));
        }

        private void RegisterCallbacks()
        {
            // buttons
            _visitAwsConsoleButton.RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AboutGameLift));
            _cancelButton.RegisterCallback<ClickEvent>(_ => Close());
            _updateButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (_inputValidator.AllInputsValid())
                {
                    SaveValues();
                    OnConfirm?.Invoke();
                    Close();
                }
                else
                {
                    EnableDisableUpdateButton();
                }
            });
            _serverFolderButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFolderPanel("Game Server Build Directory Path",
                    Application.dataPath, "");
                _buildDirectoryInput.value = value;
            });
            _serverFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var value = EditorUtility.OpenFilePanel("Game Server Build Executable Path",
                    _stateManager.ContainerGameServerBuildPath, "");
                _buildExecutableInput.value = value;
            });

            //dropdowns
            _selectECRRepositoryDropdown.RegisterValueChangedCallback(evt => {
                PopulateECRImagesDropdown(evt.newValue);
            });

            //validations
            _inputValidator.RegisterValidationCallbacks();

        }

        private void PopulateComponents()
        {
            // Building docker
            _buildDirectoryInput.value = _stateManager.ContainerGameServerBuildPath;
            _buildExecutableInput.value = _stateManager.ContainerGameServerExecutable;

            // Using docker
            _dockerImageIdInput.value = GetOrDefault(_stateManager.ContainerDockerImageId, ContainersUserInputValidation.DEFAULT_IMAGE_TAG);

            // Using ECR repository
            _selectECRRepositoryDropdown.value = _stateManager.ContainerECRRepositoryName;
            _selectECRImageDropdown.value = _stateManager.ContainerECRImageId;

            // Image tag
            _containerImageTagInput.value = GetOrDefault(_stateManager.ContainerImageTag, ContainersUserInputValidation.DEFAULT_IMAGE_TAG);

            // Always present settings
            _containerPortRangeInput.value = GetOrDefault(_stateManager.ContainerPortRange, ContainersUserInputValidation.DEFAULT_PORT_RANGE);
            _containerTotalMemoryInput.value = GetOrDefault(_stateManager.ContainerTotalMemory, ContainersUserInputValidation.DEFAULT_MEMORY_LIMIT);
            _containerTotalVcpuInput.value = GetOrDefault(_stateManager.ContainerTotalVcpu, ContainersUserInputValidation.DEFAULT_VCPU_LIMIT);

            PopulateECRRepositoriesDropdown();
            if (_stateManager.ContainerQuestionnaireScenario == ContainerScenarios.HaveContainerImageInEcr)
            {
                PopulateECRImagesDropdown(_stateManager.ContainerECRRepositoryName);
            }
            ShowHideComponents();
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
                { ContainersUserInputType.DockerImageInput, _dockerImageIdInput },
                { ContainersUserInputType.EcrImageDropdown, _selectECRImageDropdown },
                { ContainersUserInputType.EcrRepositoryDropdown, _selectECRRepositoryDropdown },
                { ContainersUserInputType.GameServerExecutableInput, _buildExecutableInput },
                { ContainersUserInputType.GameServerFolderInput, _buildDirectoryInput },
                { ContainersUserInputType.MemoryLimitInput, _containerTotalMemoryInput },
                { ContainersUserInputType.VcpuLimitInput, _containerTotalVcpuInput }
            };
            return inputMappings;
        }


        private void EnableDisableUpdateButton()
        {
            _updateButton.SetEnabled(_inputValidator.AllInputsValid());
        }

        private void HideAll()
        {
            StatefulInput.Hide(_containerServerBuildInputRow);
            StatefulInput.Hide(_containerServerExecutableInputRow);
            StatefulInput.Hide(_containerECRImageDropdownRow);
            StatefulInput.Hide(_dockerImageIDInputRow);
            StatefulInput.Hide(_containerImageTagInputRow);

            StatefulInput.Hide(_invalidECRImageLabel);
            StatefulInput.Hide(_invalidContainerImageTagLabel);
            StatefulInput.Hide(_invalidPortRangeLabel);
            StatefulInput.Hide(_invalidMemoryLimitLabel);
            StatefulInput.Hide(_invalidVcpuLimitLabel);
            StatefulInput.Hide(_invalidGameServerBuildLabel);
            StatefulInput.Hide(_invalidGameServerExecutableLabel);
            StatefulInput.Hide(_invalidECRRepoLabel);
            StatefulInput.Hide(_invalidDockerImageIDLabel);
        }

        private void ShowHideComponents()
        {
            // always active
            List<ContainersUserInputType> activeInputs = new List<ContainersUserInputType>
            {
                ContainersUserInputType.VcpuLimitInput,
                ContainersUserInputType.MemoryLimitInput,
                ContainersUserInputType.ConnectionPortRangeInput,
                ContainersUserInputType.EcrRepositoryDropdown,
            };
            HideAll();
            switch (_stateManager.ContainerQuestionnaireScenario)
            {
                case ContainerScenarios.NoContainerImageNoExistingEcrRepo:
                case ContainerScenarios.NoContainerImageUseExistingEcrRepo:
                    StatefulInput.Show(_containerServerBuildInputRow);
                    StatefulInput.Show(_containerServerExecutableInputRow);
                    StatefulInput.Show(_containerImageTagInputRow);
                    activeInputs.Add(ContainersUserInputType.GameServerFolderInput);
                    activeInputs.Add(ContainersUserInputType.GameServerExecutableInput);
                    activeInputs.Add(ContainersUserInputType.ContainerImageTagInput);
                    break;

                case ContainerScenarios.HaveContainerImageInDocker:
                    StatefulInput.Show(_dockerImageIDInputRow);
                    StatefulInput.Show(_containerImageTagInputRow);
                    activeInputs.Add(ContainersUserInputType.DockerImageInput);
                    activeInputs.Add(ContainersUserInputType.ContainerImageTagInput);
                    break;

                case ContainerScenarios.HaveContainerImageInEcr:
                    StatefulInput.Show(_containerECRImageDropdownRow);
                    activeInputs.Add(ContainersUserInputType.EcrImageDropdown);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Expected questionnaire to be filled out before popup opened. State was {_stateManager.ContainerQuestionnaireScenario}.");
            }
            _inputValidator.SetActiveInputs(activeInputs);
        }

        private void SaveValues()
        {
            _stateManager.ContainerPortRange = _containerPortRangeInput.value;
            _stateManager.ContainerTotalMemory = _containerTotalMemoryInput.value;
            _stateManager.ContainerTotalVcpu = _containerTotalVcpuInput.value;
            _stateManager.ContainerDeploymentScenario = _selectedDeploymentScenario;
            _stateManager.ContainerECRRepositoryName = _selectECRRepositoryDropdown.value;
            _stateManager.ContainerECRRepositoryUri = _ecrRepoNameUriMap[_selectECRRepositoryDropdown.value];

            switch (_stateManager.ContainerQuestionnaireScenario)
            {
                case ContainerScenarios.NoContainerImageNoExistingEcrRepo:
                case ContainerScenarios.NoContainerImageUseExistingEcrRepo:
                    _stateManager.ContainerGameServerBuildPath = _buildDirectoryInput.value;
                    _stateManager.ContainerGameServerExecutable = _buildExecutableInput.value;
                    _stateManager.ContainerImageTag = _containerImageTagInput.value;
                    break;
                case ContainerScenarios.HaveContainerImageInDocker:
                    _stateManager.ContainerDockerImageId = _dockerImageIdInput.value;
                    _stateManager.ContainerImageTag = _containerImageTagInput.value;
                    break;
                case ContainerScenarios.HaveContainerImageInEcr:
                    _stateManager.ContainerECRImageId = _selectECRImageDropdown.value;
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Expected questionnaire to be filled out before popup opened. State was {_stateManager.ContainerQuestionnaireScenario}.");
            }
        }

        private static string GetOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_root);
            var strings = new[]
            {
                Strings.ContainerGameServerBuildLabel,
                Strings.ContainerGameServerExecutableLabel,

                Strings.DockerImageIDLabel,
                
                Strings.ContainersFleetUpdatePopupDescription,
                Strings.ContainersFleetUpdateDeploymentDetailsTitle,
                Strings.SelectECRRepositoryLabel,
                Strings.ContainerSelectImageLabel,

                Strings.ContainerImageTagLabel,

                Strings.ContainerConnectionPortRangeLabel,
                Strings.ContainerConnectionPortRangeInvalidMessage,
                Strings.ContainerTotalMemoryLabel,
                Strings.ContainerTotalMemoryInvalidMessage,
                Strings.ContainerTotalVcpuLabel,
                Strings.ContainerTotalVcpuInvalidMessage,

                Strings.ContainersFleetUpdatePopupVisitConsoleButtonLabel,
                Strings.ContainersFleetUpdatePopupCancelButton,
                Strings.ContainersFleetUpdatePopupUpdateButton,
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }

        private void PopulateECRImagesDropdown(string repositoryName)
        {
            if (_stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion())
            {
                if (!string.IsNullOrEmpty(repositoryName))
                {
                    var images = _coreApi.ListECRImages(_stateManager.ProfileName, _stateManager.Region, repositoryName);
                    List<string> choices = images.ECRImages.Select(image => string.IsNullOrEmpty(image.ImageTag) ? image.ImageDigest : image.ImageTag).ToList();
                    _selectECRImageDropdown.choices = choices;
                    if (!string.IsNullOrEmpty(_stateManager.ContainerECRImageId) && choices.Contains(_stateManager.ContainerECRImageId))
                    {
                        _selectECRImageDropdown.value = _stateManager.ContainerECRImageId;
                        return;
                    }
                }
            }
            _selectECRImageDropdown.value = null;
        }

        private void PopulateECRRepositoriesDropdown()
        {
            if (_stateManager.IsBootstrapped() && _stateManager.IsInContainersRegion())
            {
                _ecrRepositories = _coreApi.DescribeECRRepositories(_stateManager.ProfileName, _stateManager.Region);
                if (_ecrRepositories != null && _ecrRepositories.ECRRepositories != null)
                {
                    List<string> choices = _ecrRepositories.ECRRepositories.Select(repository => repository.RepositoryName).ToList();
                    _selectECRRepositoryDropdown.choices = choices;
                    foreach (Repository repo in _ecrRepositories.ECRRepositories)
                    {
                        if (!_ecrRepoNameUriMap.ContainsKey(repo.RepositoryName))
                        {
                            _ecrRepoNameUriMap.Add(repo.RepositoryName, repo.RepositoryUri);
                        }
                    }
                    if (!string.IsNullOrEmpty(_stateManager.ContainerECRRepositoryName) && choices.Contains(_stateManager.ContainerECRRepositoryName))
                    {
                        _selectECRRepositoryDropdown.value = _stateManager.ContainerECRRepositoryName;
                        return;
                    }
                }
            }
            _selectECRRepositoryDropdown.value = null;
        }
    }
}
