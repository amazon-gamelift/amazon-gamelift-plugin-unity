// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class UserProfileCreation
    {
        private readonly VisualElement _container;
        private readonly VisualElement _credentialsInstructionBox;
        private readonly Button _credentialsBoxCloseButton;
        private readonly Button _createProfileButton;
        private readonly Button _cancelProfileCreateButton;
        private readonly DropdownField _regionDropDown;
        private readonly AwsCredentialsCreation _awsCredentialsCreateModel;
        private readonly List<TextField> _textFields;
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;

        public Action OnProfileCreated;

        public UserProfileCreation(VisualElement container, StateManager stateManager)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/UserProfileCreation");
            container.Add(uxml.Instantiate());

            _container = container;
            _textFields = _container.Query<TextField>().ToList();
            
            _awsCredentialsCreateModel = AwsCredentialsFactory.Create().Creation;
            _awsCredentialsCreateModel.OnCreated += () =>
            {
                stateManager.SetProfile(_awsCredentialsCreateModel.ProfileName);
                OnProfileCreated?.Invoke();
            };
            
            _createProfileButton = _container.Q<Button>("UserProfilePageAccountNewProfileCreateButton");
            _regionDropDown = _container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown");

            _credentialsInstructionBox = _container.Q<VisualElement>("CredentialsBox");
            _credentialsBoxCloseButton = _container.Q<Button>("CredentialsBoxCloseButton");

            _cancelProfileCreateButton = container.Q<Button>("UserProfilePageAccountNewProfileCancelButton");

            SetupCallBacks();
            ValidateInputs();
            LocalizeText();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var strings = new[]
            {
                Strings.UserProfilePageAccountNewProfileTitle,
                Strings.UserProfilePageAccountNewProfileDescription,
                Strings.UserProfilePageAccountNewProfileName,
                Strings.UserProfilePageAccountNewProfileAccessKeyInput,
                Strings.UserProfilePageAccountNewProfileSecretKeyLabel,
                Strings.UserProfilePageAccountNewProfileRegionLabel,
                Strings.UserProfilePageAccountNewProfileHelpLink,
                Strings.UserProfilePageAccountNewProfileCreateButton,
                Strings.UserProfilePageAccountNewProfileCancelButton,
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }

        private void ToggleHiddenText(Button button, TextField hiddenField)
        {
            var textProvider = TextProviderFactory.Create();
            hiddenField.isPasswordField = !hiddenField.isPasswordField;
            button.text = !hiddenField.isPasswordField
                ? textProvider.Get(Strings.LabelPasswordHide)
                : textProvider.Get(Strings.LabelPasswordShow);
        }

        public void Reset()
        {
            foreach (var field in _textFields)
            {
                field.value = "";
            }
        }

        private bool CreateUserProfile()
        {
            var dropdownField = _container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown");
            var credentials = _container.Query<TextField>().ToList().Select(textField => textField.value).ToList();

            if (credentials.Any(string.IsNullOrWhiteSpace))
            {
                return false;
            }

            _awsCredentialsCreateModel.ProfileName = credentials[0];
            _awsCredentialsCreateModel.AccessKeyId = credentials[1];
            _awsCredentialsCreateModel.SecretKey = credentials[2];
            _awsCredentialsCreateModel.RegionBootstrap.RegionIndex = dropdownField.index;
            _awsCredentialsCreateModel.Create();

            return true;
        }

        private void SetupCallBacks()
        {
            _container.Q<TextField>("UserProfilePageAccountNewProfileNameInput").RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            var accessKeyInput = _container.Q<TextField>("UserProfilePageAccountNewProfileAccessKeyInput");
            accessKeyInput.RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            var secretKeyInput = _container.Q<TextField>("UserProfilePageAccountNewProfileSecretKeyInput");
            secretKeyInput.RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });
            
            var accessKeyRevealButton = _container.Q<Button>("UserProfilePageAccountNewProfileAccessKeyToggleReveal");
            accessKeyRevealButton.RegisterCallback<ClickEvent>(_ =>
            {
                ToggleHiddenText(accessKeyRevealButton, accessKeyInput);
            });
            
            var secretKeyRevealButton = _container.Q<Button>("UserProfilePageAccountNewProfileSecretKeyToggleReveal");
            secretKeyRevealButton.RegisterCallback<ClickEvent>(_ =>
            {
                ToggleHiddenText(secretKeyRevealButton, secretKeyInput);
            });
            
            _regionDropDown.RegisterValueChangedCallback(_ =>
            {
                ValidateInputs();
            });

            _createProfileButton.RegisterCallback<ClickEvent>(_ =>
            {
                _credentialsInstructionBox.RemoveFromClassList("hidden");
                var result = CreateUserProfile();
                if (result)
                {
                    OnProfileCreated?.Invoke();
                }
            });

            _credentialsBoxCloseButton.RegisterCallback<ClickEvent>(_ =>
            {
                _credentialsInstructionBox.AddToClassList("hidden");
            });

            _cancelProfileCreateButton.RegisterCallback<ClickEvent>(_ =>
            {
                _credentialsInstructionBox.RemoveFromClassList("hidden");
            });
        }

        private void ValidateInputs()
        {
            var anyEmptyFields = _textFields.Any(target => string.IsNullOrWhiteSpace(target.value));
            var isRegionSelected = _regionDropDown.index >= 0 ;

            _createProfileButton.SetEnabled(!anyEmptyFields && isRegionSelected);
        }
    }
}
