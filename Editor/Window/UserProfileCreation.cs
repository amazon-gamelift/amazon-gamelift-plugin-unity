﻿// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Editor.CoreAPI;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    internal class UserProfileCreation
    {
        private readonly VisualElement _container;
        private readonly AwsCredentialsCreation _awsCredentialsCreateModel;
        private readonly List<TextField> _textFields;
        private BootstrapSettings _bootstrapSettings;
        private CancellationTokenSource _refreshBucketsCancellation;
        private StateManager _stateManager;

        public Action OnProfileCreated;

        public UserProfileCreation(VisualElement container, StateManager stateManager)
        {
            _container = container;
            _stateManager = stateManager;

            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/UserProfileCreation");
            container.Add(uxml.Instantiate());
            _textFields = container.Query<TextField>().ToList();
            
            _awsCredentialsCreateModel = AwsCredentialsFactory.Create().Creation;
            _awsCredentialsCreateModel.OnCreated += () =>
            {
                _stateManager.SetProfile(_awsCredentialsCreateModel.ProfileName);
                OnProfileCreated?.Invoke();
            };

            _container.Q<Button>("UserProfilePageAccountNewProfileCreateButton").RegisterCallback<ClickEvent>(_ =>
            {
                var result = CreateUserProfile();
                if (result)
                {
                    OnProfileCreated?.Invoke();
                }
                else
                {
                    // TODO: Show error status box
                }
            });

            LocalizeText();
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("UserProfilePageAccountNewProfileTitle", Strings.UserProfilePageAccountNewProfileTitle);
            l.SetElementText("UserProfilePageAccountNewProfileName", Strings.UserProfilePageAccountNewProfileName);
            l.SetElementText("UserProfilePageAccountNewProfileAccessKeyInput", Strings.UserProfilePageAccountNewProfileAccessKeyInput);
            l.SetElementText("UserProfilePageAccountNewProfileSecretKeyLabel", Strings.UserProfilePageAccountNewProfileSecretKeyLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionLabel", Strings.UserProfilePageAccountNewProfileRegionLabel);
            l.SetElementText("UserProfilePageAccountNewProfileRegionPlaceholderDropdown", Strings.UserProfilePageAccountNewProfileRegionPlaceholderDropdown);
            l.SetElementText("UserProfilePageAccountNewProfileHelpLink", Strings.UserProfilePageAccountNewProfileHelpLink);
            l.SetElementText("UserProfilePageAccountNewProfileCreateButton", Strings.UserProfilePageAccountNewProfileCreateButton);
            l.SetElementText("UserProfilePageAccountNewProfileCancelButton", Strings.UserProfilePageAccountNewProfileCancelButton);
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
    }
}