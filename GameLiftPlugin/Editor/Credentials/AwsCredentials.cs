// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;

namespace AmazonGameLift.Editor
{
    internal class AwsCredentials
    {
        private const int NewProfileMode = 0;
        private const int SelectProfileMode = 1;
        private readonly CoreApi _coreApi;
        private int _selectedMode;
        private int _confirmedMode = -1;

        public bool IsNewProfileMode => SelectedMode == NewProfileMode;

        public int SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                SetUpSelectedMode();
            }
        }

        public AwsCredentialsCreation Creation { get; private set; }
        public AwsCredentialsUpdate Update { get; private set; }
        public bool CanSelect { get; private set; }

        public AwsCredentials(TextProvider textProvider, ILogger logger, CoreApi coreApi = null)
        {
            _coreApi = coreApi ?? CoreApi.SharedInstance;
            var regionBootstrap = new RegionBootstrap(_coreApi);
            Creation = new AwsCredentialsCreation(textProvider, regionBootstrap, _coreApi, logger);
            Update = new AwsCredentialsUpdate(textProvider, regionBootstrap, _coreApi, logger);
            Creation.OnCreated += OnCreated;
        }

        internal AwsCredentials(AwsCredentialsCreation creation, AwsCredentialsUpdate update, CoreApi coreApi = null)
        {
            _coreApi = coreApi ?? CoreApi.SharedInstance;
            Creation = creation ?? throw new ArgumentNullException(nameof(creation));
            Update = update ?? throw new ArgumentNullException(nameof(update));
            Creation.OnCreated += OnCreated;
        }

        public void SetUp()
        {
            Refresh();
            SelectedMode = CanSelect ? SelectProfileMode : NewProfileMode;
        }

        public void Refresh()
        {
            GetProfilesResponse response = _coreApi.ListCredentialsProfiles();
            CanSelect = response.Success && response.Profiles.Any();
        }

        private void SetUpSelectedMode()
        {
            if (_confirmedMode == SelectedMode)
            {
                return;
            }

            _confirmedMode = SelectedMode;
            Refresh();

            if (IsNewProfileMode)
            {
                Creation.Refresh();
            }
            else
            {
                Update.Refresh();
            }
        }

        private void OnCreated()
        {
            Refresh();
        }
    }
}
