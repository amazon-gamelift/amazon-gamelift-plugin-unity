// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using BucketErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    internal class AwsUserProfilesPage
    {
        private List<TextField> AccountDetailTextFields = new();

        private readonly AwsCredentialsUpdate _awsCredentialsUpdateModel;
        private readonly BootstrapSettings _bootstrapSettings;
        private readonly ElementLocalizer _elementLocalizer;

        private VisualElement _currentElement;

        private const string hiddenClassName = "hidden";
        private readonly StateManager _stateManager;
        private readonly VisualElement _container;
        private readonly UserProfileCreation _userProfileCreation;
        private readonly HelpfulResources _helpfulResources;
        private readonly List<VisualElement> _allMenus;
        private readonly VisualElement _noAccountMenu;
        private readonly VisualElement _createMenu;
        private readonly VisualElement _profilesTableMenu;
        private readonly Button _bootstrapButton;
        private readonly Button _selectProfileButton;
        private readonly Button _openCredentialsFileButton;
        private TextField _configFilePathInput;
        private StatusBox _statusBox;

        public AwsUserProfilesPage(VisualElement container, StateManager stateManager)
        {
            var awsCredentials = AwsCredentialsFactory.Create();
            _awsCredentialsUpdateModel = awsCredentials.Update;

            _container = container;
            var mVisualTreeAsset = Resources.Load<VisualTreeAsset>("EditorWindow/Pages/AwsUserProfilesPage");
            var uxml = mVisualTreeAsset.Instantiate();

            _container.Add(uxml);
            _elementLocalizer = new ElementLocalizer(_container);
            SetupStatusBoxes();
            LocalizeText();

            _noAccountMenu = _container.Q<VisualElement>("UserProfilePageNoAccountMenu");
            _createMenu = _container.Q<VisualElement>("UserProfilePageCreateMenu");
            _profilesTableMenu = _container.Q<VisualElement>("UserProfilesTableMenu");
            _allMenus = new List<VisualElement>()
            {
                _noAccountMenu,
                _createMenu,
                _profilesTableMenu,
            };
            AccountDetailTextFields = _createMenu.Query<TextField>().ToList();

            _stateManager = stateManager;
            _stateManager.OnUserProfileUpdated += () =>
            {
                _awsCredentialsUpdateModel.Refresh();
                _awsCredentialsUpdateModel.Update();
            };

            var createProfileContainer = _container.Q("UserProfilePageCreateMenu");
            _userProfileCreation = new UserProfileCreation(createProfileContainer, _stateManager);
            _userProfileCreation.OnProfileCreated += () =>
            {
                ShowProfileMenu(_profilesTableMenu);
            };
            _bootstrapSettings = BootstrapSettingsFactory.Create(_stateManager);

            var helpfulResourcesContainer = _container.Q<Foldout>("HelpfulResourcesFoldout");
            _helpfulResources = new HelpfulResources(helpfulResourcesContainer);
            helpfulResourcesContainer.value = false;

            RefreshProfiles();

            container.Q<DropdownField>("UserProfilePageAccountNewProfileRegionDropdown").choices =
                _stateManager.CoreApi.ListAvailableRegions().ToList();

            _configFilePathInput = _container.Q<TextField>("UserProfilePageAwsConfigurationFileInput");
            _configFilePathInput.SetEnabled(false);

            _selectProfileButton = _container.Q<Button>(Strings.UserProfilePageSetProfileButton);

            _bootstrapButton = _container.Q<Button>(Strings.UserProfilePageBootstrapButton);

            _openCredentialsFileButton = _container.Q<Button>("UserProfilePageAwsConfigurationFileButton");

            UpdateGui();

            _stateManager.OnUserProfileUpdated += UpdateGui;

            _stateManager.OnUserProfileUpdated += EnableDisableButtons;

            _stateManager.OnAddAnotherProfile += () => ShowProfileMenu(_createMenu);

            _stateManager.OnProfileRadioButtonChanged += EnableDisableButtons;

            ChooseProfileMenu();
            SetupButtonCallbacks();
            EnableDisableButtons();
        }

        private void UpdateGui()
        {
            if (!_stateManager.IsBootstrapped())
            {
                _statusBox.Show(StatusBox.StatusBoxType.Warning, Strings.UserProfilePageStatusBoxWarningText);
            }
            else
            {
                _statusBox.Close();
            }

            GetCredentialsFileResponse response = _stateManager.CoreApi.GetCredentialsFile();
            if (!response.Success)
            {
                UnityEngine.Debug.LogError(_elementLocalizer.GetError(response.ErrorCode));
                _openCredentialsFileButton.SetEnabled(false);
                return;
            }
            _configFilePathInput.value = response.FilePath;
            _openCredentialsFileButton.SetEnabled(true);
        }

        private void LocalizeText()
        {
            var strings = new[]
            {
                Strings.UserProfilePageTitle,
                Strings.UserProfilePageDescription,

                Strings.UserProfilePageAccountCardNoAccountTitle,
                Strings.UserProfilePageAccountCardNoAccountDescription,
                Strings.UserProfilePageAccountCardNoAccountButtonLabel,

                Strings.UserProfilePageAccountCardHasAccountTitle,
                Strings.UserProfilePageAccountCardHasAccountDescription,
                Strings.UserProfilePageAccountCardHasAccountButton,

                Strings.UserProfilePageTableTitle,
                Strings.UserProfilePageTableDescription,
                Strings.UserProfilePageAwsConfigurationFileLabel,
                Strings.UserProfilePageBootstrapButton,
                Strings.UserProfilePageCompletedBootstrapHelpLink,
                Strings.UserProfilePageSetProfileButton
            };
            foreach (var s in strings)
            {
                _elementLocalizer.SetElementText(s, s);
            }
            _elementLocalizer.SetElementTooltip(Strings.UserProfilePageAwsConfigurationFileLabel,
                Strings.UserProfilePageAwsConfigurationFileTooltip);
        }

        private void SetupButtonCallbacks()
        {
            _container.Q<Button>("UserProfilePageAccountCardNoAccountButton")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.CreateAwsAccountLearnMore));
            _container.Q<VisualElement>("UserProfilePageAccountNewProfileHelpLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsIamDocumentation));

            _container.Q<Button>("UserProfilePageAccountCardHasAccountButton").RegisterCallback<ClickEvent>(_ =>
            {
                var targetWizard = _container.Q<VisualElement>("UserProfilePageCreateMenu");
                ShowProfileMenu(targetWizard);
            });
            _container.Q<Button>("UserProfilePageAccountNewProfileCancelButton").RegisterCallback<ClickEvent>(_ =>
            {
                _userProfileCreation.Reset();
                ChooseProfileMenu();
            });

            _selectProfileButton.RegisterCallback<ClickEvent>(_ =>
            {
                _stateManager.SetProfile(_stateManager.SelectedRadioButton);
            });

            _bootstrapButton.RegisterCallback<ClickEvent>(_ =>
            {
                _bootstrapSettings.RefreshBucketName();
                OpenS3Popup(_bootstrapSettings.BucketName);
            });
            _container.Q<VisualElement>("WhatIsBootstrappingLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.S3BootstrapHelp));

            _openCredentialsFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                Process.Start($"\"{_configFilePathInput.value}\"");
            });
        }

        private void EnableDisableButtons()
        {
            var selectedProfile = _stateManager.SelectedProfile;
            var selectedButton = _stateManager.SelectedRadioButton;
            _bootstrapButton.SetEnabled(selectedProfile != null && selectedProfile.Name == selectedButton && !_stateManager.IsBootstrapped(selectedProfile));
            _selectProfileButton.SetEnabled(selectedProfile != null && selectedButton != null && selectedProfile.Name != selectedButton);
        }

        private void ChooseProfileMenu()
        {
            if (_stateManager.AllProfiles.Count == 0)
            {
                ShowProfileMenu(_noAccountMenu);
            }
            else if (_stateManager.SelectedProfile == null)
            {
                ShowProfileMenu(_createMenu);
            }
            else
            {
                ShowProfileMenu(_profilesTableMenu);
            }
        }

        private void ShowProfileMenu(VisualElement targetMenu)
        {
            _allMenus.ForEach(menu => menu.AddToClassList(hiddenClassName));
            if (targetMenu != null)
            {
                targetMenu.RemoveFromClassList(hiddenClassName);
            }
        }

        private void RefreshProfiles()
        {
            _awsCredentialsUpdateModel.Refresh();
        }

        private void BootstrapAccount(string bucketName)
        {
            var bucketResponse = _bootstrapSettings.CreateBucket(bucketName);
            if (bucketResponse.Success || bucketResponse.ErrorCode == BucketErrorCode.BucketNameAlreadyExists)
            {
                _stateManager.SetBucketBootstrap(bucketName);
                _statusBox.Show(StatusBox.StatusBoxType.Success, Strings.UserProfilePageStatusBoxSuccessText);
            }
            else
            {
                _statusBox.Show(StatusBox.StatusBoxType.Error, Strings.UserProfilePageBootstrapErrorText,
                    bucketResponse.ErrorMessage, Urls.AwsS3Console, Strings.ViewS3LogsStatusBoxUrlTextButton);
            }
        }

        private void OpenS3Popup(string bucketName)
        {
            var popup = ScriptableObject.CreateInstance<GameLiftPluginBucketPopup>();
            popup.Init(bucketName);
            popup.OnConfirm += BootstrapAccount;
            popup.ShowModalUtility();
        }

        private void SetupStatusBoxes()
        {
            _statusBox = _container.Q<StatusBox>("UserProfilePageStatusBox");
        }
    }
}