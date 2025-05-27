// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ConnectToFleetInput : ProgressBarStepComponent
    {
        private TextField _fleetNameInput;
        private DropdownField _fleetNameDropdownContainer;
        private VisualElement _fleetCreateContainer;
        private VisualElement _fleetConnectContainer;
        private VisualElement _fleetId;
        private Label _fleetIdText;
        private VisualElement _fleetStatus;
        private StatusIndicator _statusIndicator;
        private GameLiftFleetManager _fleetManager => _stateManager.FleetManager;
        private Button _cancelButton;
        private Button _viewFleetOnConsoleButton;
        private Button _createNewFleetButton;
        private Button _modifyFleetButton;

        private FleetStatus _fleetState = FleetStatus.NotCreated;
        private List<FleetAttributes> _fleetAttributes = new();
        private bool _isCompleted = false;

        public ConnectToFleetInput(VisualElement container, StateManager stateManager) : base (container, stateManager, "EditorWindow/Components/ConnectToFleetInput")
        {
            new DeploymentStepTemplate.Builder(Strings.AnywherePageConnectFleetTitle, Strings.AnywherePageConnectFleetDescription)
                 .WithoutBaseButtons()
                 .Build(container);

            AssignUiElements(container);
            RegisterCallBacks(container);
            LocalizeText();
        }

        protected sealed override Task StartOrResumeStep()
        {
            SetupPage();
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() {
            ResetState();
            SetupPage();
        }

        private void ResetState()
        {
            _isCompleted = false;
            _fleetState = FleetStatus.NotCreated;
            _stateManager.AnywhereFleetName = null;
            _stateManager.AnywhereFleetId = null;
            _stateManager.AnywhereFleetLocation = null;
            _stateManager.ComputeName = "";
        }

        private void FleetSelected()
        {
            SetFleetState();
            if (!_isCompleted)
            {

                _isCompleted = true;
                CompleteStep();
            }
        }

        private async Task OnAnywhereCreateConfirmClicked(string fleetName)
        {
            if (_isCompleted)
            {
                var _fleetStateBeforeReset = _fleetState;
                ResetState();
                _fleetState = _fleetStateBeforeReset;
                EditCompleted();
            } else
            {
                TryStart();
            }

            if (_fleetManager != null && _fleetState is FleetStatus.NotCreated or FleetStatus.Creating)
            {
                var customLocationResponse = await _fleetManager.CreateCustomLocationIfNotExists();
                if (!customLocationResponse.Success)
                {
                    EncounteredException(StatusBox.StatusBoxType.Error,
                        Strings.AnywherePageStatusBoxDefaultFleetErrorText, customLocationResponse.ErrorMessage);
                    return;
                }

                var createFleetResponse = await _fleetManager.CreateFleet(fleetName, customLocationResponse.Location)!;
                if (createFleetResponse.Success)
                {
                    _stateManager.AnywhereFleetName = createFleetResponse.FleetName;
                    _stateManager.AnywhereFleetId = createFleetResponse.FleetId;
                    _stateManager.AnywhereFleetLocation = customLocationResponse.Location;
                    _stateManager.ComputeName = "";

                    await UpdateFleetMenu();
                    FleetSelected();

                    // Reset and start next step because a new Compute will need to be created for the new fleet
                    _nextStep?.ResetAndTryStart();
                }
                else
                {
                    EncounteredException(StatusBox.StatusBoxType.Error,
                        Strings.AnywherePageStatusBoxDefaultFleetErrorText, createFleetResponse.ErrorMessage);
                }
            }

            UpdateGUI();
        }

        private async Task OnModifyFleetClicked()
        {
            Reset();
            if (_fleetState is FleetStatus.Selected or FleetStatus.Selecting)
            {
                await UpdateFleetMenu();
                _fleetState = FleetStatus.Creating;
            }

            UpdateGUI();
        }

        private async void OnSelectFleetDropdown(string fleetSelection)
        {
            // Need to reset any error state when selecting a new fleet
            if (!_isCompleted)
            {
                TryStart();
            }
  
            var items = fleetSelection.Split(" (");
            var fleetName = items[0];
            var fleetId = items[1].Remove(items[1].Length - 1, 1);
            var selectedFleet =
                _fleetAttributes.FirstOrDefault(fleet => fleet.Name == fleetName && fleet.FleetId == fleetId);
            if (selectedFleet != null)
            {
                var fleetLocationResponse = await _fleetManager.FindFirstFleetLocation(selectedFleet.FleetId);
                _fleetIdText.text = selectedFleet.FleetId;
                _stateManager.AnywhereFleetName = selectedFleet.Name;
                _stateManager.AnywhereFleetId = selectedFleet.FleetId;
                _stateManager.AnywhereFleetLocation = fleetLocationResponse.Location;
                FleetSelected();
            }

            SetupPage();
        }

        private void RegisterCallBacks(VisualElement container)
        {
            container.Q<Button>("AnywherePageCreateFleetButton").RegisterCallback<ClickEvent>(async _ =>
                await OnAnywhereCreateConfirmClicked(_fleetNameInput.text));
            _modifyFleetButton.RegisterCallback<ClickEvent>(async _ =>
                await OnModifyFleetClicked());
            _createNewFleetButton.RegisterCallback<ClickEvent>(_ => CreateNewFleetClicked());
            _viewFleetOnConsoleButton.RegisterCallback<ClickEvent>(_ =>
                Application.OpenURL(string.Format(Urls.AwsGameLiftFleetViewTemplate, _stateManager.Region,
                _stateManager.AnywhereFleetId)));
            _fleetNameDropdownContainer.RegisterValueChangedCallback(evt => OnSelectFleetDropdown(evt.newValue));
            _cancelButton.RegisterCallback<ClickEvent>(_ => SetupPage());
        }

        private void AssignUiElements(VisualElement container)
        {
            _fleetNameInput = container.Q<TextField>("AnywherePageCreateFleetNameInput");
            _fleetNameDropdownContainer = container.Q<DropdownField>("AnywherePageConnectFleetNameDropdown");
            _fleetId = container.Q("AnywherePageConnectFleetID");
            _fleetIdText = container.Q<Label>("AnywherePageConnectFleetIDDisplay");
            _fleetStatus = container.Q("AnywherePageConnectFleetStatus");
            _fleetCreateContainer = container.Q("AnywherePageCreateFleet");
            _fleetConnectContainer = container.Q("AnywherePageConnectFleet");
            _cancelButton = container.Q<Button>("AnywherePageCreateFleetCancelButton");
            _viewFleetOnConsoleButton = container.Q<Button>("AnywherePageConnectFleetViewOnConsoleButton");
            _createNewFleetButton = container.Q<Button>("AnywherePageCreateNewFleetButton");
            _modifyFleetButton = container.Q<Button>("AnywherePageModifyFleetButton");
            _statusIndicator = container.Q<StatusIndicator>();
        }

        private async Task UpdateFleetMenu()
        {
            if (_stateManager.GameLiftWrapper != null)
            {
                _fleetAttributes = await _fleetManager.DescribeFleetAttributes(ComputeType.ANYWHERE);
                if (_fleetAttributes == null)
                {
                    _fleetAttributes = new List<FleetAttributes>();
                }

                var textProvider = new TextProvider();

                _fleetNameDropdownContainer.choices =
                    _fleetAttributes.Select(fleet => $"{fleet.Name} ({fleet.FleetId})").ToList();
                if (string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId))
                {
                    _fleetNameDropdownContainer.SetValueWithoutNotify(
                        textProvider.Get(Strings.AnywherePageConnectFleetDefault));
                }
                else
                {
                    _fleetNameDropdownContainer.value =
                        $"{_stateManager.AnywhereFleetName} ({_stateManager.AnywhereFleetId})";
                }

                _fleetIdText.text = _stateManager.AnywhereFleetId;

                var fleet = _fleetAttributes.FirstOrDefault(fleet => fleet.FleetId == _stateManager.AnywhereFleetId);
                if (fleet != null)
                {
                    if (fleet.Status == Amazon.GameLift.FleetStatus.ERROR)
                    {
                        _statusIndicator.Set(State.Failed,
                            textProvider.Get(Strings.AnywherePageConnectFleetStatusError));
                    }
                    else
                    {
                        _statusIndicator.Set(State.Success,
                            textProvider.Get(Strings.AnywherePageConnectFleetStatusActive));
                        FleetSelected();
                    }
                }
            }
        }

        private void SetFleetState()
        {
            if (_fleetAttributes.Count == 0)
            {
                _fleetState = FleetStatus.NotCreated;
            }
            else
            {
                var fleet = _fleetAttributes.FirstOrDefault(fleet => fleet.FleetId == _stateManager.AnywhereFleetId);

                _fleetState = fleet == null ? FleetStatus.Selecting : FleetStatus.Selected;
            }
        }

        private async void SetupPage()
        {
            await UpdateFleetMenu();
            SetFleetState();
            UpdateGUI();
        }

        private List<VisualElement> GetFleetVisualElements() => new List<VisualElement>()
        {
            _fleetNameInput,
            _fleetCreateContainer,
            _fleetNameDropdownContainer,
            _cancelButton,
            _fleetConnectContainer,
            _fleetId,
            _fleetStatus,
            _viewFleetOnConsoleButton,
            _modifyFleetButton,
            _createNewFleetButton
        };

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _fleetState switch
            {
                FleetStatus.NotCreated => new List<VisualElement>() { _fleetNameInput, _fleetCreateContainer },
                FleetStatus.Creating => new List<VisualElement>()
                {
                    _fleetNameInput, _cancelButton, _fleetCreateContainer
                },
                FleetStatus.Selecting => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetConnectContainer, _createNewFleetButton
                },
                FleetStatus.Selected => new List<VisualElement>()
                {
                    _fleetNameDropdownContainer, _fleetId, _fleetStatus, _fleetConnectContainer, _viewFleetOnConsoleButton, _modifyFleetButton
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected sealed override void UpdateGUI()
        {
            var visibleElements = GetVisibleItemsByState();
            foreach (var element in GetFleetVisualElements())
            {
                if (visibleElements.Contains(element))
                {
                    Show(element);

                    // Disable fleet dropdown if step is complete
                    if (_fleetState == FleetStatus.Selected && element == _fleetNameDropdownContainer)
                    {
                        element.SetEnabled(false);
                    }
                    else
                    {
                        element.SetEnabled(true);
                    }
                    
                }
                else
                {
                    Hide(element);
                }
            }

            _container.SetEnabled(_stateManager.IsBootstrapped());
        }

        private void CreateNewFleetClicked()
        {
            _fleetState = FleetStatus.NotCreated;
            UpdateGUI();
        }

        public enum FleetStatus
        {
            NotCreated,
            Creating,
            Selecting,
            Selected
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var strings = new[]
            {
                Strings.AnywherePageCreateFleetNameLabel,
                Strings.AnywherePageCreateFleetNameHint,
                Strings.AnywherePageConnectFleetName,
                Strings.AnywherePageConnectFleetNameLabel,
                Strings.AnywherePageConnectFleetIDLabel,
                Strings.AnywherePageConnectFleetStatusLabel,
                Strings.AnywherePageModifyFleetButton,
                Strings.AnywherePageConnectFleetViewOnConsoleButton
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }
    }
}
