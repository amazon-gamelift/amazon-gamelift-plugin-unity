﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.GameLift.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class RegisterComputeInput : StatefulInput
    {
        private ComputeStatus _computeState;

        private readonly TextField _computeNameInput;
        private readonly Button _registerButton;
        private readonly Button _replaceComputeButton;
        private readonly Button _cancelReplaceButton;
        private readonly VisualElement _computeStatus;
        private readonly VisualElement _container;
        private readonly StatusIndicator _statusIndicator;
        private readonly StateManager _stateManager;
        private StatusBox _registerComputeStatusBox;

        private const string _primaryButtonClassName = "button--primary";
        private const string _defaultComputeName = "ComputerName-ProfileName";
        private const string _defaultIpAddress = "127.0.0.1";
        private string _computeName;
        private string _ipAddress;

        private Action _onComputeChanged;

        public RegisterComputeInput(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/RegisterComputeInput");
            container.Add(uxml.Instantiate());
            _container = container;

            _stateManager = stateManager;
            _computeNameInput = container.Q<TextField>("AnywherePageComputeNameInput");
            _computeStatus = container.Q("AnywherePageComputeStatus");
            _registerButton = container.Q<Button>("AnywherePageComputeRegisterButton");
            _replaceComputeButton = container.Q<Button>("AnywherePageComputeReplaceComputeButton");
            _cancelReplaceButton = container.Q<Button>("AnywherePageComputeCancelReplaceButton");
            _statusIndicator = container.Q<StatusIndicator>();
            LocalizeText();

            _computeState = !string.IsNullOrWhiteSpace(_stateManager.ComputeName) &&
                            !string.IsNullOrWhiteSpace(_stateManager.IpAddress)
                ? ComputeStatus.Registered
                : ComputeStatus.NotRegistered;

            _stateManager.OnUserProfileUpdated += UpdateGUI;

            SetupConfigSettings();
            RegisterCallbacks();
            SetupStatusBox();

            _stateManager.OnFleetChanged += SetValidCompute;
            _stateManager.OnFleetChanged += UpdateGUI; 

            UpdateGUI();
        }

        private void RegisterCallbacks()
        {
            _registerButton.RegisterCallback<ClickEvent>(_ => OnRegisterComputeButtonClicked());
            _replaceComputeButton.RegisterCallback<ClickEvent>(_ => OnReplaceComputeButtonClicked());
            _cancelReplaceButton.RegisterCallback<ClickEvent>(_ => OnCancelReplaceButtonClicked());

            _computeNameInput.value = _computeName;
            _computeNameInput.RegisterValueChangedCallback(_ => UpdateComputeTextFields(_computeNameInput));
        }

        private async void OnRegisterComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.NotRegistered or ComputeStatus.Registering)
            {
                var previousComputeName = _stateManager.ComputeName;
                var registerResponse = await _stateManager.ComputeManager.RegisterFleetCompute(_computeName,
                    _stateManager.AnywhereFleetId, _stateManager.AnywhereFleetLocation, _ipAddress);
                if (registerResponse.Success)
                {
                    _computeState = ComputeStatus.Registered;
                    _stateManager.ComputeName = registerResponse.ComputeName;
                    _stateManager.IpAddress = registerResponse.IpAddress;
                    _stateManager.WebSocketUrl = registerResponse.WebSocketUrl;

                    if (!string.IsNullOrWhiteSpace(previousComputeName) && previousComputeName != _computeName)
                    {
                        var deregisterResponse =
                            await _stateManager.ComputeManager.DeregisterCompute(previousComputeName,
                                _stateManager.AnywhereFleetId);
                                
                        if (!deregisterResponse)
                        {
                            Debug.LogError(new TextProvider().GetError(ErrorCode.DeregisterComputeFailed));
                        }
                    }
                }
                else
                {
                    _registerComputeStatusBox.Show(StatusBox.StatusBoxType.Error,
                        Strings.AnywherePageStatusBoxDefaultComputeErrorText,
                        registerResponse.ErrorMessage);
                }
            }

            UpdateGUI();
        }

        private async void SetValidCompute()
        {
            var computes = await _stateManager.ComputeManager.ListCompute(_stateManager.AnywhereFleetId);
            if (computes.Success)
            {
                var matchingCompute =
                    computes.ComputeList.FirstOrDefault(compute =>
                        compute.ComputeName == _stateManager.ComputeName);
                if (matchingCompute != null)
                {
                    SetupComputeAfterSwitch(matchingCompute);
                }
                else if (computes.ComputeList.Count > 0)
                {
                    var firstCompute = computes.ComputeList.First();
                    SetupComputeAfterSwitch(firstCompute);
                }
                else
                {
                    _computeState = ComputeStatus.NotRegistered;
                    _stateManager.ComputeName = "";
                    _computeNameInput.value = _defaultComputeName;
                }
            }
            else
            {
                Debug.LogError(computes.ErrorMessage);
            }

            UpdateGUI();
        }

        private void SetupComputeAfterSwitch(Compute compute)
        {
            _computeState = ComputeStatus.Registered;
            _stateManager.ComputeName = compute.ComputeName;
            _stateManager.IpAddress = compute.IpAddress;
            _stateManager.WebSocketUrl = compute.GameLiftServiceSdkEndpoint;
            _computeNameInput.value = compute.ComputeName;
        }

        private void OnReplaceComputeButtonClicked()
        {
            if (_computeState is ComputeStatus.Registered)
            {
                _computeState = ComputeStatus.Registering;
            }

            UpdateGUI();
        }

        private void OnCancelReplaceButtonClicked()
        {
            if (_computeState is ComputeStatus.Registering)
            {
                _computeState = ComputeStatus.Registered;
            }

            _computeNameInput.value = _stateManager.ComputeName;
            
            UpdateGUI();
        }

        private void UpdateComputeTextFields(TextField computeTextField)
        {
            var computeTextNameValid = computeTextField.value.Length >= 1;
            _computeName = computeTextField.value;

            _registerButton.SetEnabled(computeTextNameValid);
        }

        private void SetupConfigSettings()
        {
            _computeName = !string.IsNullOrWhiteSpace(_stateManager.ComputeName) ? _stateManager.ComputeName : _defaultComputeName;
            _ipAddress = !string.IsNullOrWhiteSpace(_stateManager.IpAddress) ? _stateManager.IpAddress : _defaultIpAddress;
        }

        private List<VisualElement> GetComputeVisualElements() =>
            new List<VisualElement>()
            {
                _registerButton,
                _cancelReplaceButton,
                _computeStatus,
                _replaceComputeButton
            };

        private List<VisualElement> GetVisibleItemsByState()
        {
            return _computeState switch
            {
                ComputeStatus.Disabled => new List<VisualElement>(),
                ComputeStatus.NotRegistered => new List<VisualElement>() { _registerButton, },
                ComputeStatus.Registering => new List<VisualElement>() { _registerButton, _cancelReplaceButton },
                ComputeStatus.Registered => new List<VisualElement>() { _computeStatus, _replaceComputeButton },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SetupStatusBox()
        {
            _registerComputeStatusBox = _container.Q<StatusBox>("AnywherePageComputeStatusBox");
        }

        private void CheckReadOnly()
        {
            if (_computeState == ComputeStatus.Registered)
            {
                _computeNameInput.isReadOnly = true;
                _computeNameInput.SetEnabled(false);
            }
            else
            {
                _computeNameInput.isReadOnly = false;
                _computeNameInput.SetEnabled(true);
            }
        }

        protected sealed override void UpdateGUI()
        {
            var elements = GetVisibleItemsByState();
            foreach (var element in GetComputeVisualElements())
            {
                if (elements.Contains(element))
                {
                    Show(element);
                }
                else
                {
                    Hide(element);
                }
            }

            _container.SetEnabled(_stateManager.IsBootstrapped &&
                                  !string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId));

            if (!string.IsNullOrWhiteSpace(_stateManager.ComputeName))
            {
                var textProvider = new TextProvider();
                _statusIndicator.Set(State.Success,
                    textProvider.Get(Strings.AnywherePageComputeStatusRegistered));
            }

            if (string.IsNullOrWhiteSpace(_stateManager.AnywhereFleetId))
            {
                _registerButton.RemoveFromClassList(_primaryButtonClassName);
            }
            else
            {
                _registerButton.AddToClassList(_primaryButtonClassName);
            }

            CheckReadOnly();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("AnywherePageComputeNameLabel", Strings.AnywherePageComputeNameLabel);
            l.SetElementText("AnywherePageComputeIPLabel", Strings.AnywherePageComputeIPLabel);
            l.SetElementText("AnywherePageComputeIPDescription", _defaultIpAddress);
            l.SetElementText("AnywherePageComputeStatusLabel", Strings.AnywherePageComputeStatusLabel);
            l.SetElementText("AnywherePageComputeRegisterButton", Strings.AnywherePageComputeRegisterButton);
            l.SetElementText("AnywherePageComputeReplaceComputeButton",
                Strings.AnywherePageComputeReplaceComputeButton);
            l.SetElementText("AnywherePageComputeCancelReplaceButton", Strings.AnywherePageComputeCancelReplaceButton);
        }

        public ComputeStatus getComputeStatus() 
        {
            return _computeState;
        }
    }
}
