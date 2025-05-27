// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AmazonGameLift.Editor
{
    public class CreateECRRepoStep : ContainerStepComponent
    {
        private const string RepositoryName = "gamelift-unity-plugin";
        private readonly CoreApi _coreApi;
        private string _errorMessage = null;
        private VisualElement _repoNameField;

        public CreateECRRepoStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/Containers/CreateECRRepoStep")
        {
            _coreApi = CoreApi.SharedInstance;

            var elementLocalizer = new ElementLocalizer(_container);
            elementLocalizer.SetElementText("RepoNameValue", RepositoryName);

            container.Q<VisualElement>("ECRUserGuideLinkParent")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.ECRUserGuide));

            _repoNameField = container.Q<VisualElement>("RepoNameField");

            Hide(container.Q<VisualElement>("ButtonsWhenFailed"));
            Hide(_repoNameField);

            container.Q<Button>("ProceedButton").RegisterCallback<ClickEvent>(_ => { base.CompleteStep(); });
            container.Q<Button>("TryAgainButton").RegisterCallback<ClickEvent>(_ => { base.TryStart(); });
        }
        
        protected sealed override Task StartOrResumeStep()
        {
            bool stopEarly = false;
            Show(_repoNameField);
            if (_errorMessage != null)
            {
                EncounteredException(_errorMessage);
                stopEarly = true;
            }

            if (_stateManager.IsECRRepoCreated && _stateManager.ContainerECRRepositoryUri != null)
            {
                SetRepoUriAndComplete(_stateManager.ContainerECRRepositoryUri);
                stopEarly = true;
            }

            if (stopEarly)
            {
                return Task.CompletedTask;
            }

            // First see if repository already exists
            var describeResponse = _coreApi.DescribeECRRepositories(_stateManager.ProfileName, _stateManager.Region, new List<string> { RepositoryName });
            if (describeResponse.Success && describeResponse.ECRRepositories.Count() > 0)
            {
                SetRepoUriAndComplete(describeResponse.ECRRepositories.First().RepositoryUri);
                return Task.CompletedTask;
            }

            var response = _coreApi.CreateRepository(_stateManager.ProfileName, _stateManager.Region, RepositoryName);
            if (response.Success)
            {
                SetRepoUriAndComplete(response.RepositoryUri);
            }
            else
            {
                EncounteredException(response.ErrorMessage);
            }
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep()
        {
            _stateManager.ContainerECRRepositoryUri = null;
            _stateManager.ContainerECRRepositoryName = null;
            _errorMessage = null;
            Hide(_container.Q<VisualElement>("ButtonsWhenFailed"));
        }

        private void SetRepoUriAndComplete(string ECRRepositoryUri)
        {
            _stateManager.ContainerECRRepositoryUri = ECRRepositoryUri;
            _stateManager.ContainerECRRepositoryName = RepositoryName;
            _stateManager.IsECRRepoCreated = true;
            Hide(_container.Q<VisualElement>("ButtonsWhenFailed"));

            base.CompleteStep();
        }

        private void EncounteredException(string errorMessage)
        {
            _errorMessage = errorMessage;

            base.EncounteredException(StatusBox.StatusBoxType.Error, errorMessage);
            Show(_container.Q("ButtonsWhenFailed"));
        }
    }
}