// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsUpdate
    {
        private readonly CoreApi _coreApi;
        private readonly ILogger _logger;
        private readonly TextProvider _textProvider;
        private readonly Status _status = new Status();
        private int _selectedProfileIndex;
        private string _currentAccessKeyId;
        private string _currentSecretKey;

        public IReadStatus Status => _status;

        public RegionBootstrap RegionBootstrap { get; }

        public bool CanUpdate =>
            SelectedProfileIndex >= 0 && _selectedProfileIndex < AllProlfileNames.Length
            && !string.IsNullOrEmpty(AccessKeyId)
            && !string.IsNullOrEmpty(SecretKey)
            && (SelectedProfileIndex != CurrentProfileIndex || CanUpdateCurrentProfile || RegionBootstrap.CanSave);

        public bool CanUpdateCurrentProfile =>
            CurrentProfileIndex >= 0
            && (AccessKeyId != _currentAccessKeyId || SecretKey != _currentSecretKey);

        public string CurrentProfileName { get; private set; }

        public int CurrentProfileIndex { get; private set; } = -1;

        public string[] AllProlfileNames { get; private set; } = new string[0];

        public string AccessKeyId { get; set; }

        public string SecretKey { get; set; }

        /// <summary>
        /// When changed, loads <see cref="AccessKeyId"/> and <see cref="SecretKey"/>.
        /// </summary>
        public int SelectedProfileIndex
        {
            get => _selectedProfileIndex;
            set
            {
                if (_selectedProfileIndex == value)
                {
                    return;
                }

                _selectedProfileIndex = value;
                Load();
            }
        }

        public AwsCredentialsUpdate(TextProvider textProvider, RegionBootstrap regionBootstrap,
            CoreApi coreApi, ILogger logger)
        {
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            RegionBootstrap = regionBootstrap ?? throw new ArgumentNullException(nameof(regionBootstrap));
        }

        /// <summary>
        /// Loads <see cref="AccessKeyId"/> and <see cref="SecretKey"/>.
        /// </summary>
        public void Load()
        {
            AccessKeyId = null;
            SecretKey = null;

            if (_selectedProfileIndex < 0 || _selectedProfileIndex >= AllProlfileNames.Length)
            {
                return;
            }

            string profileName = AllProlfileNames[_selectedProfileIndex];
            RetriveAwsCredentialsResponse response = _coreApi.RetrieveAwsCredentials(profileName);

            if (!response.Success)
            {
                SetErrorStatus(response.ErrorCode);
                return;
            }

            AccessKeyId = response.AccessKey;
            SecretKey = response.SecretKey;

            if (_selectedProfileIndex == CurrentProfileIndex)
            {
                _currentAccessKeyId = AccessKeyId;
                _currentSecretKey = SecretKey;
            }
        }

        /// <summary>
        /// Sets the profile by <see cref="SelectedProfileIndex"/> as current, saves its credentials.
        /// </summary>
        public void Update()
        {
            if (!CanUpdate)
            {
                _logger.Log(DevStrings.OperationInvalid, LogType.Error);
                return;
            }

            string profileName = AllProlfileNames[_selectedProfileIndex];
            Response response = _coreApi.UpdateAwsCredentials(profileName, AccessKeyId, SecretKey);

            if (!response.Success)
            {
                SetErrorStatus(response.ErrorCode);
                _logger.LogResponseError(response);
                return;
            }

            Response writeResponse = _coreApi.PutSetting(SettingsKeys.CurrentProfileName, profileName);

            if (!writeResponse.Success)
            {
                SetErrorStatus(writeResponse.ErrorCode);
                _logger.LogResponseError(writeResponse);
                return;
            }

            (bool success, string errorCode) = RegionBootstrap.Save();

            if (!success && errorCode != null)
            {
                string messageFormat = _textProvider.Get(Strings.StatusRegionUpdateFailedWithError);
                string error = _textProvider.GetError(errorCode);
                error = string.Format(messageFormat, error);
                _status.SetMessage(error, MessageType.Error);
            }
            else
            {
                _status.SetMessage(_textProvider.Get(Strings.StatusProfileUpdated), MessageType.Info);
            }

            CurrentProfileIndex = SelectedProfileIndex;
            CurrentProfileName = profileName;
            _currentAccessKeyId = AccessKeyId;
            _currentSecretKey = SecretKey;
            _status.IsDisplayed = true;
        }

        /// <summary>
        /// Loads profile names, sets <see cref="AllProlfileNames"/>.
        /// If any profiles are loaded and one of them is selected in settings,
        /// sets <see cref="SelectedProfileIndex"/> to match it.
        /// </summary>
        /// <returns></returns>
        public virtual void Refresh()
        {
            RegionBootstrap.Refresh();
            SelectedProfileIndex = -1;
            CurrentProfileName = null;
            _currentAccessKeyId = null;
            _currentSecretKey = null;
            AllProlfileNames = Array.Empty<string>();
            GetProfilesResponse response = _coreApi.ListCredentialsProfiles();

            if (!response.Success)
            {
                _logger.LogResponseError(response);
                return;
            }

            AllProlfileNames = response.Profiles.ToArray();

            if (AllProlfileNames.Length == 0)
            {
                return;
            }

            GetSettingResponse getCurrentResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!getCurrentResponse.Success)
            {
                return;
            }

            int currentIndex = Array.IndexOf(AllProlfileNames, getCurrentResponse.Value);

            if (currentIndex >= 0)
            {
                SelectedProfileIndex = currentIndex;
                CurrentProfileIndex = currentIndex;
                CurrentProfileName = AllProlfileNames[CurrentProfileIndex];
                Load();
            }
        }

        private void SetErrorStatus(string errorCode = null)
        {
            string message = _textProvider.GetError(errorCode);
            _status.SetMessage(message, MessageType.Error);
            _status.IsDisplayed = true;
        }
    }
}
