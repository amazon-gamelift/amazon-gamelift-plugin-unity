// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AmazonGameLift.Runtime;

namespace AmazonGameLift.Editor
{
    internal class ContainersQuestionnaire : StatefulInput
    {
        private bool _enabled;
        private readonly VisualElement _container;
        private readonly VisualElement _UseExistingEcrQuestionRadioGroup;
        private readonly VisualElement _DockerOrEcrQuestionRadioGroup;
        private readonly VisualElement _DoesContainerImageExistRadioGroup;
        private readonly RadioButton _uxmlFieldContainerImageExist;
        private readonly RadioButton _uxmlFieldContainerImageNotExist;
        private readonly RadioButton _uxmlFieldUseExistingEcrRepo;
        private readonly RadioButton _uxmlFieldNotUseExistingEcrRepo;
        private readonly RadioButton _uxmlFieldImageInDocker;
        private readonly RadioButton _uxmlFieldImageInEcr;

        private readonly StateManager _stateManager;

        public ContainersQuestionnaire(VisualElement container, StateManager stateManager, bool enabled)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/Containers/ContainersQuestionnaire");
            container.Add(uxml.Instantiate());

            _container = container;
            _stateManager = stateManager;
            _enabled = enabled;

            // Get a reference to the radio buttons from UXML.
            _uxmlFieldContainerImageExist = container.Q<RadioButton>("ContainerImageExist");
            _uxmlFieldContainerImageNotExist = container.Q<RadioButton>("ContainerImageNotExist");
            _uxmlFieldUseExistingEcrRepo = container.Q<RadioButton>("UseExistingEcrRepo");
            _uxmlFieldNotUseExistingEcrRepo = container.Q<RadioButton>("NotUseExistingEcrRepo");
            _uxmlFieldImageInDocker = container.Q<RadioButton>("ImageInDocker");
            _uxmlFieldImageInEcr = container.Q<RadioButton>("ImageInEcr");

            _UseExistingEcrQuestionRadioGroup = container.Q("UseExistingEcrQuestionRadioGroup");
            _DockerOrEcrQuestionRadioGroup = container.Q("DockerOrEcrQuestionRadioGroup");
            _DoesContainerImageExistRadioGroup = container.Q("DoesContainerImageExistRadioGroup");

            SetupRadioButtons();

            _UseExistingEcrQuestionRadioGroup = container.Q("UseExistingEcrQuestionRadioGroup");
            _DockerOrEcrQuestionRadioGroup = container.Q("DockerOrEcrQuestionRadioGroup");
            _DoesContainerImageExistRadioGroup = container.Q("DoesContainerImageExistRadioGroup");

            SetEnabled(enabled);

            stateManager.OnUserProfileUpdated += UpdateGUI;
            LocalizeText();
            UpdateGUI();
        }


        protected sealed override void UpdateGUI()
        {
            var deploymentScenario = _stateManager.ContainerQuestionnaireScenario;
            switch (deploymentScenario)
            {
                case ContainerScenarios.NoContainerImageNoExistingEcrRepo:
                    _uxmlFieldContainerImageNotExist.value = true;
                    _uxmlFieldNotUseExistingEcrRepo.value = true;
                    Show(_UseExistingEcrQuestionRadioGroup);
                    Hide(_DockerOrEcrQuestionRadioGroup);
                    break;
                case ContainerScenarios.NoContainerImageUseExistingEcrRepo:
                    _uxmlFieldContainerImageNotExist.value = true;
                    _uxmlFieldUseExistingEcrRepo.value = true;
                    Show(_UseExistingEcrQuestionRadioGroup);
                    Hide(_DockerOrEcrQuestionRadioGroup);
                    break;
                case ContainerScenarios.HaveContainerImageInDocker:
                    _uxmlFieldContainerImageExist.value = true;
                    _uxmlFieldImageInDocker.value = true;
                    Hide(_UseExistingEcrQuestionRadioGroup);
                    Show(_DockerOrEcrQuestionRadioGroup);
                    break;
                case ContainerScenarios.HaveContainerImageInEcr:
                    _uxmlFieldContainerImageExist.value = true;
                    _uxmlFieldImageInEcr.value = true;
                    Hide(_UseExistingEcrQuestionRadioGroup);
                    Show(_DockerOrEcrQuestionRadioGroup);
                    break;
            }
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            _DockerOrEcrQuestionRadioGroup.SetEnabled(value);
            _UseExistingEcrQuestionRadioGroup.SetEnabled(value);
            _DoesContainerImageExistRadioGroup.SetEnabled(value);
        }

        private void SetupRadioButtons()
        {
            _uxmlFieldContainerImageExist.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue)
                {
                    _stateManager.ContainerQuestionnaireScenario = _uxmlFieldImageInDocker.value ?
                        ContainerScenarios.HaveContainerImageInDocker :
                        ContainerScenarios.HaveContainerImageInEcr;
                }
                else
                {
                    _stateManager.ContainerQuestionnaireScenario = _uxmlFieldUseExistingEcrRepo.value ?
                        ContainerScenarios.NoContainerImageUseExistingEcrRepo :
                        ContainerScenarios.NoContainerImageNoExistingEcrRepo;
                }
                _stateManager.OnContainerQuestionnaireScenarioChanged?.Invoke();
                UpdateGUI();
            });

            _uxmlFieldUseExistingEcrRepo.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue)
                {
                    _stateManager.ContainerQuestionnaireScenario = ContainerScenarios.NoContainerImageUseExistingEcrRepo;
                }
                else
                {
                    _stateManager.ContainerQuestionnaireScenario = ContainerScenarios.NoContainerImageNoExistingEcrRepo;

                }
                _stateManager.OnContainerQuestionnaireScenarioChanged?.Invoke();
            });

            _uxmlFieldImageInDocker.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (_uxmlFieldImageInDocker.value)
                {
                    _stateManager.ContainerQuestionnaireScenario = ContainerScenarios.HaveContainerImageInDocker;
                }
                else
                {
                    _stateManager.ContainerQuestionnaireScenario = ContainerScenarios.HaveContainerImageInEcr;
                }
                _stateManager.OnContainerQuestionnaireScenarioChanged?.Invoke();
            });
        }
        public void PopulateContent()
        {
            UpdateGUI();
        }


        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("ContainerQuestionnaireDoesContainerImageExist", Strings.ContainerQuestionnaireDoesContainerImageExist);
            l.SetElementText("ContainerQuestionnaireUseExistingRepo", Strings.ContainerQuestionnaireUseExistingRepo);
            l.SetElementText("ContainerQuestionnaireWhereItLive", Strings.ContainerQuestionnaireWhereItLive);
            l.SetElementText("ContainerImageExist", Strings.ContainerQuestionnaireYes);
            l.SetElementText("ContainerImageNotExist", Strings.ContainerQuestionnaireNo);
            l.SetElementText("UseExistingEcrRepo", Strings.ContainerQuestionnaireYes);
            l.SetElementText("NotUseExistingEcrRepo", Strings.ContainerQuestionnaireNo);
        }
    }
}
