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
    internal class AwsCredentialsCreation
    {
        private readonly CoreApi _coreApi;
        private readonly ILogger _logger;
        private readonly TextProvider _textProvider;
        private readonly Status _status = new Status();

        public IReadStatus Status => _status;

        public RegionBootstrap RegionBootstrap { get; }

        public bool CanCreate =>
          !string.IsNullOrEmpty(ProfileName)
          && !string.IsNullOrEmpty(AccessKeyId)
          && !string.IsNullOrEmpty(SecretKey)
          && RegionBootstrap.CanSave;

        public string CurrentProfileName { get; private set; }

        public string ProfileName { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretKey { get; set; }

        public event Action OnCreated;

        public AwsCredentialsCreation(TextProvider textProvider, RegionBootstrap regionBootstrap,
            CoreApi coreApi, ILogger logger)
        {
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            RegionBootstrap = regionBootstrap ?? throw new ArgumentNullException(nameof(regionBootstrap));
        }

        public virtual void Refresh()
        {
            _status.IsDisplayed = false;
            RegionBootstrap.Refresh();
            CurrentProfileName = null;

            GetProfilesResponse response = _coreApi.ListCredentialsProfiles();

            if (!response.Success)
            {
                _logger.LogResponseError(response);
                return;
            }

            string[] allProlfileNames = response.Profiles.ToArray();

            if (allProlfileNames.Length == 0)
            {
                return;
            }

            GetSettingResponse getCurrentResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!getCurrentResponse.Success)
            {
                return;
            }

            int currentIndex = Array.IndexOf(allProlfileNames, getCurrentResponse.Value);

            if (currentIndex >= 0)
            {
                CurrentProfileName = allProlfileNames[currentIndex];
            }
        }

        public void Create()
        {
            if (!CanCreate)
            {
                _logger.Log(DevStrings.OperationInvalid, LogType.Error);
                return;
            }

            Response response = _coreApi.SaveAwsCredentials(ProfileName, AccessKeyId, SecretKey, RegionBootstrap.AllRegions[RegionBootstrap.RegionIndex]);

            if (!response.Success)
            {
                SetErrorStatus(response.ErrorCode);
                _logger.LogResponseError(response);
                return;
            }

            Response writeResponse = _coreApi.PutSetting(SettingsKeys.CurrentProfileName, ProfileName);

            if (!writeResponse.Success)
            {
                SetErrorStatus(writeResponse.ErrorCode);
                _logger.LogResponseError(writeResponse);
                return;
            }

            CurrentProfileName = ProfileName;
            RegionBootstrap.Save();
            _status.SetMessage(_textProvider.Get(Strings.StatusProfileCreated), MessageType.Info);
            _status.IsDisplayed = true;
            OnCreated?.Invoke();
        }

        private void SetErrorStatus(string errorCode = null)
        {
            string message = _textProvider.GetError(errorCode);
            _status.SetMessage(message, MessageType.Error);
            _status.IsDisplayed = true;
        }
    }
}
