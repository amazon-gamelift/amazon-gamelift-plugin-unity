// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using System;

namespace AmazonGameLift.Editor
{
    public class ContainersUserInputValidation
    {
        public static readonly Regex PORT_RANGE_REGEX = new Regex("^[0-9]+-[0-9]+$");
        public static readonly Regex POSITIVE_INTEGER_REGEX = new Regex("^[0-9]+$");
        public static readonly Regex GAME_NAME_REGEX = new Regex("^[a-zA-Z0-9-]{1,94}$");
        public static readonly Regex CONTAINER_IMAGE_TAG_REGEX = new Regex("^[a-zA-Z0-9._-]{1,300}$");
        public static readonly Regex DOCKER_IMAGE_ID_REGEX = new Regex("^(?:(?=[^:/]{1,253})(?!-)[a-zA-Z0-9-]{1,63}(?<!-)(?:.(?!-)[a-zA-Z0-9-]{1,63}(?<!-))*(?::[0-9]{1,5})?/)?((?![._-])(?:[a-z0-9._-]*)(?<![._-])(?:/(?![._-])[a-z0-9._-]*(?<![._-]))*)(?::(?![.-])[a-zA-Z0-9_.-]{1,128})?$");

        public static readonly string DEFAULT_MEMORY_LIMIT = "4000";
        public static readonly string DEFAULT_VCPU_LIMIT = "2";
        public static readonly string DEFAULT_IMAGE_TAG = "unity-gamelift-plugin";
        public static readonly string DEFAULT_GAME_NAME = "MyGame";
        public static readonly string DEFAULT_PORT_RANGE = "33430-33440";

        private readonly Dictionary<string, string> _ecrRepoNameUriMap = new Dictionary<string, string>();

        private IReadOnlyDictionary<ContainersUserInputType, Label> _errorMessageMappings;
        private IReadOnlyDictionary<ContainersUserInputType, VisualElement> _inputMappings;
        private IReadOnlyDictionary<ContainersUserInputType, Regex> _regexMappings;

        private List<ContainersUserInputType> _activeInputs;

        public Action OnValidationEvent;


        public ContainersUserInputValidation(IReadOnlyDictionary<ContainersUserInputType, Label> errorMessageMappings, IReadOnlyDictionary<ContainersUserInputType, VisualElement> inputMappings)
        {
            _errorMessageMappings = errorMessageMappings;
            _inputMappings = inputMappings;
            _activeInputs = new List<ContainersUserInputType>();
            _regexMappings = GetRegexMappings();
        }

        public void SetActiveInputs(List<ContainersUserInputType> inputTypes)
        {
            _activeInputs.Clear();
            _activeInputs.AddRange(inputTypes);
        }

        public void RegisterValidationCallbacks()
        {
            foreach (ContainersUserInputType inputType in _inputMappings.Keys)
            {
                var input = _inputMappings.GetValueOrDefault(inputType);
                if (inputType == ContainersUserInputType.GameServerExecutableInput ||
                    inputType == ContainersUserInputType.GameServerFolderInput)
                {
                    ((TextField)input).RegisterValueChangedCallback(evt =>
                   {
                       ValidateInput(inputType);
                       OnValidationEvent?.Invoke();
                   });
                }
                else
                {
                    input.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        ValidateInput(inputType);
                        OnValidationEvent?.Invoke();
                    });
                }
            }
        }

        public bool AllInputsValid()
        {
            foreach (ContainersUserInputType inputType in _activeInputs)
            {
                if (!IsInputValid(inputType))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsInputValid(ContainersUserInputType inputType)
        {
            VisualElement input = _inputMappings.GetValueOrDefault(inputType);
            switch (inputType)
            {
                //dropdowns
                case ContainersUserInputType.EcrRepositoryDropdown:
                case ContainersUserInputType.EcrImageDropdown:
                    DropdownField dropdown = (DropdownField)input;
                    return dropdown.value != null;
                // file checking
                case ContainersUserInputType.GameServerFolderInput:
                    TextField containerGameServerBuildInput = (TextField)input;
                    return !string.IsNullOrEmpty(containerGameServerBuildInput.value) &&
                        Directory.Exists(containerGameServerBuildInput.value);
                case ContainersUserInputType.GameServerExecutableInput:
                    TextField containerGameServerExecutableInput = (TextField)input;
                    return !string.IsNullOrEmpty(containerGameServerExecutableInput.value) &&
                        File.Exists(containerGameServerExecutableInput.value);
                // regex checking
                case ContainersUserInputType.DockerImageInput:
                case ContainersUserInputType.ContainerImageTagInput:
                case ContainersUserInputType.ConnectionPortRangeInput:
                case ContainersUserInputType.MemoryLimitInput:
                case ContainersUserInputType.VcpuLimitInput:
                case ContainersUserInputType.GameNameInput:
                    TextField textInput = (TextField)input;
                    Regex inputRegex = _regexMappings.GetValueOrDefault(inputType);
                    return !string.IsNullOrEmpty(textInput.value) && inputRegex.Match(textInput.value).Success;
                default:
                    return false;
            }
        }

        public void ValidateInput(ContainersUserInputType inputType)
        {
            Label errorLabel = _errorMessageMappings.GetValueOrDefault(inputType);
            StatefulInput.ShowHide(errorLabel, !IsInputValid(inputType));
        }

        private Dictionary<ContainersUserInputType, Regex> GetRegexMappings()
        {
            var regexMappings = new Dictionary<ContainersUserInputType, Regex>
            {
                { ContainersUserInputType.ConnectionPortRangeInput, PORT_RANGE_REGEX },
                { ContainersUserInputType.ContainerImageTagInput, CONTAINER_IMAGE_TAG_REGEX },
                { ContainersUserInputType.DockerImageInput, DOCKER_IMAGE_ID_REGEX },
                { ContainersUserInputType.GameNameInput, GAME_NAME_REGEX },
                { ContainersUserInputType.MemoryLimitInput, POSITIVE_INTEGER_REGEX },
                { ContainersUserInputType.VcpuLimitInput, POSITIVE_INTEGER_REGEX }
            };
            return regexMappings;
        }
    }

    public enum ContainersUserInputType
    {
        EcrRepositoryDropdown,
        EcrImageDropdown,
        DockerImageInput,
        GameServerFolderInput,
        GameServerExecutableInput,
        GameNameInput,
        ConnectionPortRangeInput,
        MemoryLimitInput,
        VcpuLimitInput,
        ContainerImageTagInput,
    }
}
