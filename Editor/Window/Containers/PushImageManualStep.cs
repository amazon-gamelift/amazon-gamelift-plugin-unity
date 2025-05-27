// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// The component that's used when docker is not installed. Prepared commands will be displayed, and
    /// users will need to push and proceed to the next step manually.
    /// </summary>
    public class PushImageManualStep : ContainerStepComponent
    {
        private const string LoginCommand = "aws ecr get-login-password --region {{REGION}} --profile {{PROFILE_NAME}} | docker login --username AWS --password-stdin {{ECR_REGISTRY_URL}}";
        private const string BuildCommand = "docker build -t {{IMAGE_TAG}} .";
        private const string ImageExistsTaggingCommand = "docker tag {{IMAGE_ID}} {{ECR_REPO_URI}}:{{IMAGE_TAG}}";
        private const string PushCommand = "docker push {{ECR_REPO_URI}}:{{IMAGE_TAG}}";
        private readonly Foldout _commandFoldout;
        private readonly VisualElement _continueSection;
        private readonly CoreApi _coreApi;
        private bool _isStepCompleted = false;

        public PushImageManualStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/Containers/PushImageManualStep")
        {
            _coreApi = CoreApi.SharedInstance;

            container.Q<Button>("ContinueButton").RegisterCallback<ClickEvent>(_ => { CompleteStep(); });

            _isStepCompleted = _stateManager.IsContainerPushedToECR;

            _commandFoldout = _container.Q<Foldout>("CommandFoldout");
            Hide(_commandFoldout);

            _continueSection = _container.Q("ContinueSection");
            Hide(_continueSection);

            LocalizeText();
        }

        protected new void CompleteStep()
        {
            _stateManager.IsContainerPushedToECR = true;
            _isStepCompleted = true;
            _stateManager.ContainerECRImageId = _stateManager.ContainerImageTag;
            _stateManager.ContainerECRImageUri = _stateManager.ContainerECRRepositoryUri + ":" + _stateManager.ContainerImageTag;
            Hide(_continueSection);
            _commandFoldout.value = false;
            base.CompleteStep();
        }

        protected sealed override void ResetStep()
        {
            _stateManager.IsContainerPushedToECR = false;
            _isStepCompleted = false;
            Hide(_continueSection);
            Hide(_commandFoldout);
        }

        protected sealed override Task StartOrResumeStep()
        {
            if (_isStepCompleted)
            {
                CompleteStep();
                return Task.CompletedTask;
            }

            CopyCommandField loginCommand = _container.Q<CopyCommandField>("LoginCommand");
            loginCommand.UpdateText(GetPreparedCommand(LoginCommand));
            CopyCommandField buildCommand = _container.Q<CopyCommandField>("BuildCommand");
            buildCommand.UpdateText(GetPreparedCommand(BuildCommand));
            CopyCommandField tagCommand = _container.Q<CopyCommandField>("TagCommand");
            tagCommand.UpdateText(GetPreparedCommand(ImageExistsTaggingCommand));
            CopyCommandField pushCommand = _container.Q<CopyCommandField>("PushCommand");
            pushCommand.UpdateText(GetPreparedCommand(PushCommand));
            Show(_continueSection);
            Show(_commandFoldout);
            _commandFoldout.value = true;

            return Task.CompletedTask;
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            var strings = new[]
            {
                Strings.ContainerPushImageManualStepTitle,
                Strings.ContainerPushImageManualStepDescription,
                Strings.ContainerPushImageManualLoginCommandLabel,
                Strings.ContainerPushImageManualBuildCommandLabel,
                Strings.ContainerPushImageManualTagCommandLabel,
                Strings.ContainerPushImageManualPushCommandLabel,
                Strings.ContainerPushImageManualStepCallToActionLabel
            };
            foreach (var s in strings)
            {
                l.SetElementText(s, s);
            }
        }
    }
}