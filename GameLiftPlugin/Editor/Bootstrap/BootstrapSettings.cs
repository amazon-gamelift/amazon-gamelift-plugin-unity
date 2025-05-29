// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// A view model for <see cref="BootstrapWindow"/>.
    /// </summary>
    internal class BootstrapSettings
    {
        public const int CreationMode = 0;
        public const int SelectionMode = 1;
        public const int NoneLifeCyclePolicyIndex = 0;
        private const int DefaultLifeCyclePolicyIndex = 0;
        private readonly Status _status = new Status();
        private readonly BucketUrlFormatter _bucketUrlFormatter = new BucketUrlFormatter();
        private readonly BucketPolicy[] _lifecyclePolicies;
        private readonly TextProvider _textProvider;
        private readonly IBucketNameFormatter _bucketFormatter;
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;
        private readonly CoreApi _coreApi;
        private readonly BootstrapUtility _bootstrapUtility;
        private List<string> _existingBuckets = new List<string>();

        public IReadStatus Status => _status;

        public IReadOnlyList<string> ExistingBuckets => _existingBuckets;

        public string[] AllLifecyclePolicyNames { get; }

        /// <summary>
        /// Is set by <see cref="Refresh"/>, <see cref="RefreshBucketName"/> or <see cref="RefreshCurrentBucket"/>.
        /// </summary>
        public string CurrentRegion { get; private set; }

        /// <summary>
        /// Updated in <see cref="CreateBucket"/> or <see cref="SaveSelectedBucket"/>.
        /// </summary>
        public string CurrentBucketName { get; private set; }

        public string CurrentBucketUrl { get; private set; }

        public bool HasCurrentBucket { get; private set; }

        /// <summary>
        /// Generated from <see cref="GameName"/>.
        /// </summary>
        public string BucketName { get; set; }

        public int LifeCyclePolicyIndex { get; set; }

        public bool CanCreate => !string.IsNullOrEmpty(BucketName);

        public bool IsBucketListLoaded { get; private set; }

        public bool CanSaveSelectedBucket =>
            _existingBuckets.Count != 0
            && CurrentRegion != null
            && !string.IsNullOrEmpty(BucketName);

        public int SelectedMode { get; set; }

        public BootstrapSettings(IEnumerable<BucketPolicy> lifecyclePolicies,
            IEnumerable<string> lifecyclePolicyNames, TextProvider textProvider,
            IBucketNameFormatter bucketFormatter, ILogger logger, StateManager stateManager, CoreApi coreApi = null,
            BootstrapUtility bootstrapUtility = null)
        {
            if (lifecyclePolicies is null)
            {
                throw new ArgumentNullException(nameof(lifecyclePolicies));
            }

            if (lifecyclePolicyNames is null)
            {
                throw new ArgumentNullException(nameof(lifecyclePolicyNames));
            }

            _lifecyclePolicies = lifecyclePolicies.ToArray();
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _bucketFormatter = bucketFormatter ?? throw new ArgumentNullException(nameof(bucketFormatter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager;
            _coreApi = coreApi ?? CoreApi.SharedInstance;
            _bootstrapUtility = bootstrapUtility ?? BootstrapUtility.SharedInstance;
            LifeCyclePolicyIndex = DefaultLifeCyclePolicyIndex;
            AllLifecyclePolicyNames = lifecyclePolicyNames.ToArray();
        }

        public void SelectBucket(string name)
        {
            if (BucketName == name)
            {
                return;
            }

            BucketName = name;

            // Reset if we select a new valid bucket
            _status.IsDisplayed &= string.IsNullOrEmpty(BucketName);
        }

        public Response CreateBucket(string bucketName)
        {
            BucketName = bucketName;
            if (!CanCreate)
            {
                var emptyBucketNameResponse = new Response()
                {
                    ErrorCode = CoreErrorCode.BucketNameCanNotBeEmpty,
                    ErrorMessage = _textProvider.GetError(CoreErrorCode.BucketNameCanNotBeEmpty)
                };
                OnBucketCreationFailure(emptyBucketNameResponse);
                return emptyBucketNameResponse;
            }

            GetBootstrapDataResponse bootstrapResponse = _bootstrapUtility.GetBootstrapData();

            if (!bootstrapResponse.Success)
            {
                OnBucketCreationFailure(bootstrapResponse);
                return bootstrapResponse;
            }

            CreateBucketResponse createResponse =
                _coreApi.CreateBucket(bootstrapResponse.Profile, bootstrapResponse.Region, BucketName);

            if (createResponse.Success)
            {
                OnBucketCreated(bootstrapResponse.Profile, bootstrapResponse.Region, BucketName);
                return createResponse;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(createResponse.ErrorMessage))
                {
                    createResponse.ErrorMessage = _textProvider.GetError(createResponse.ErrorCode);
                }
                OnBucketCreationFailure(createResponse);
                return createResponse;
            }
        }

        public void RefreshCurrentBucket()
        {
            CurrentBucketName = _stateManager.BucketName;
            bool isRegionValid = _coreApi.IsValidRegion(_stateManager.Region);
            CurrentRegion = isRegionValid ? _stateManager.Region : null;
            HasCurrentBucket = !string.IsNullOrEmpty(CurrentBucketName) && isRegionValid;

            if (HasCurrentBucket)
            {
                CurrentBucketUrl = _bucketUrlFormatter.Format(CurrentBucketName, CurrentRegion);
                return;
            }

            CurrentBucketName = null;
            CurrentBucketUrl = null;

            if (!isRegionValid)
            {
                CurrentRegion = null;
            }
        }

        public void RefreshBucketName()
        {
            BucketName = null;

            CurrentRegion = _stateManager.Region;

            if (string.IsNullOrWhiteSpace(CurrentRegion) || !_coreApi.IsValidRegion(CurrentRegion))
            {
                SetErrorStatus(Strings.StatusGetRegionFailed);
                return;
            }

            var profileName = _stateManager.ProfileName;

            if (string.IsNullOrWhiteSpace(profileName))
            {
                SetErrorStatus(Strings.StatusGetProfileFailed);
                return;
            }

            RetrieveAccountIdByCredentialsResponse accountIdResponse = _coreApi.RetrieveAccountId(profileName);

            if (!accountIdResponse.Success)
            {
                SetErrorStatus(Strings.StatusBootstrapFailedTemplate, accountIdResponse);
                _logger.LogResponseError(accountIdResponse);
                return;
            }

            BucketName = _bucketFormatter.FormatBucketName(accountIdResponse.AccountId, CurrentRegion);
        }

        private void SetInfoStatus(string statusKey)
        {
            SetStatus(statusKey, MessageType.Info);
        }

        private void SetErrorStatus(string statusKey)
        {
            SetStatus(statusKey, MessageType.Error);
        }

        private void SetErrorStatus(string statusKey, Response errorResponse)
        {
            string errorTemplate = _textProvider.Get(statusKey);
            string message = string.Format(errorTemplate, _textProvider.GetError(errorResponse.ErrorCode),
                errorResponse.ErrorMessage);
            _status.SetMessage(message, MessageType.Error);
            _status.IsDisplayed = true;
        }

        private void SetStatus(string statusKey, MessageType messageType)
        {
            _status.SetMessage(_textProvider.Get(statusKey), messageType);
            _status.IsDisplayed = true;
        }

        private void OnBucketCreated(string profileName, string region, string bucketName)
        {
            BucketPolicy policy = _lifecyclePolicies[LifeCyclePolicyIndex];

            if (policy != BucketPolicy.None)
            {
                PutLifecycleConfigurationResponse putPolicyResponse =
                    _coreApi.PutBucketLifecycleConfiguration(profileName, region, bucketName, policy);

                if (!putPolicyResponse.Success)
                {
                    _logger.LogResponseError(putPolicyResponse);
                }
            }

            SetInfoStatus(Strings.StatusBootstrapComplete);
            RefreshCurrentBucket();
        }

        private void OnBucketCreationFailure(Response response)
        {
            SetErrorStatus(Strings.StatusBootstrapFailedTemplate, response);
        }
    }
}
