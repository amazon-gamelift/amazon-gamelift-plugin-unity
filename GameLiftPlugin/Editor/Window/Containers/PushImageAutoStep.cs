// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.IO;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using System.Diagnostics;
using Amazon.Runtime.Internal.Util;


namespace AmazonGameLift.Editor
{
    /// <summary>
    /// The component that's used when docker is installed. We generate and execute the push script,
    /// and proceed to the next step automatically
    /// </summary>
    public class PushImageAutoStep : ContainerStepComponent
    {
        private const int periodMs = 5 * 1000; // check result of command execution the every 5 seconds
        private const int timeoutMs = 10 * 60 * 1000; // 10 minutes timeout
        private string _scriptLoggingPath;
        private readonly CoreApi _coreApi;
        private bool _isStepCompleted = false;
        private string _errorMessage = null;
        private Button _proceedButton;
        private Button _tryAgainButton;
        private Button _viewLogButton;
        private UnityLogger _logger;
        private GameLiftSynchronizationContext _mainThreadContext;
        private DeploymentStepTemplate _stepContent;

        private VisualElement ButtonContainer => _stepContent.ButtonContainer;

        public PushImageAutoStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/Containers/PushImageAutoStep")
        {
            TextProvider textProvider = TextProviderFactory.Create();
            _logger  = UnityLoggerFactory.Create(textProvider);
            _coreApi = CoreApi.SharedInstance;

            _stepContent = new DeploymentStepTemplate.Builder(Strings.ContainerPushImageAutoStepTitle, Strings.ContainerPushImageAutoStepDescription)
                .WithHelpLinks(new DeploymentStepTemplateLink(Urls.ECRUserGuide, Strings.ContainerLinksEcrUserGuideLabel))
                .WithBaseButtons()
                .Build(container);

            _proceedButton = ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonProceed);
            _tryAgainButton = ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonTryAgain);
            _viewLogButton = ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonViewLogs);

            _proceedButton.RegisterCallback<ClickEvent>(_ => {}); // NO-OP
            _tryAgainButton.RegisterCallback<ClickEvent>(_ => { base.ResetAndTryStart(); });
            _viewLogButton.RegisterCallback<ClickEvent>(_ => { Process.Start($"\"{_scriptLoggingPath}\""); });

            Hide(ButtonContainer);
            Hide(_proceedButton);
            Hide(_tryAgainButton);
            Hide(_viewLogButton);
            Hide(_stepContent.ContentContainer);

            _isStepCompleted = _stateManager.IsContainerPushedToECR;
            
            // Some Unity functions needs to be executed in main thread (called from async functions)
            _mainThreadContext = GameLiftSynchronizationContext.Current;
        }

        private void PopulateTagSection()
        {
            Show(_stepContent.ContentContainer);
            _stepContent.ContentContainer.Q<Label>("ImageTagValue").text = DashIfEmpty(_stateManager.ContainerImageTag);
        }

        protected void SaveImageTagAndCompleteStep()
        {
            PopulateTagSection();
            _stateManager.IsContainerPushedToECR = true;
            _stateManager.ContainerECRImageId = _stateManager.ContainerImageTag;
            _stateManager.ContainerECRImageUri = _stateManager.ContainerECRRepositoryUri + ":" + _stateManager.ContainerImageTag;

            _isStepCompleted = true;

            base.CompleteStep();
        }

        private void FailStep(string errorText)
        {
            _errorMessage = errorText;
            base.EncounteredException(
                statusBoxType: StatusBox.StatusBoxType.Error,
                text: errorText,
                externalButtonText: "Show logs",
                externalButtonLink: _scriptLoggingPath,
                externalTargetType: StatusBox.StatusBoxExternalTargetType.File
                );
            Show(ButtonContainer);
            Show(_tryAgainButton);
            Show(_viewLogButton);
            Hide(_proceedButton);
        }

        private void UpdateToInProgress()
        {
            Show(ButtonContainer);
            Hide(_tryAgainButton);
            Hide(_proceedButton);
            Show(_viewLogButton);

            _stateManager.IsContainerPushedToECR = false;

            PopulateTagSection();
        }

        protected sealed override void ResetStep()
        {
            Hide(ButtonContainer);
            Hide(_proceedButton);
            Hide(_tryAgainButton);
            Hide(_viewLogButton);
            Hide(_stepContent.ContentContainer);
            _isStepCompleted = false;
            _errorMessage = null;
            _stateManager.IsContainerPushedToECR = false;
            _stateManager.ContainerECRImageId = null;
        }

        protected sealed override async Task StartOrResumeStep()
        {
            bool stopEarly = false;
            if (_errorMessage != null)
            {
                _mainThreadContext.Send(_ => FailStep(_errorMessage), null);
                stopEarly = true;
            }

            if (_isStepCompleted)
            {
                _mainThreadContext.Send(_ => SaveImageTagAndCompleteStep(), null);
                stopEarly = true;
            }

            if (stopEarly)
            {
                return;
            }

            try
            {
                StartProcess();
            }
            catch (Exception e)
            {
                _mainThreadContext.LogError($"Failed to push image to Amazon ECR due to unexpected exception: {e}.");
                _mainThreadContext.Send(_ => FailStep($"Failed to push image to Amazon ECR due to unexpected exception: {e.Message}.\nSee Console for full exception."), null);
                throw e;
            }

            await WaitForProcessToComplete();
        }

        private void StartProcess() {
            var fileWrapper = new FileWrapper();
            string containersPath = PathConverter.SharedInstance.GetContainersAbsolutePath();

            // Prepare output directory
            string containersOutputDirectory = Path.Combine(containersPath, Paths.ContainersOutputFolderName);
            if (!fileWrapper.DirectoryExists(containersOutputDirectory))
            {
                fileWrapper.CreateDirectory(containersOutputDirectory);
            }

            _scriptLoggingPath = Path.Combine(containersOutputDirectory, "PushExistingImageToECRScriptOutput.txt");
            string scriptPath = Path.Combine(containersOutputDirectory, "PushExistingImageToECRScript.ps1");

            // Update UI to be in progress
            _mainThreadContext.Send(_ => UpdateToInProgress(), null);

            // Prepare script and write to output folder
            string commandTemplate = fileWrapper.ReadAllText(Path.Combine(containersPath, Paths.ContainerPushImageScriptFileName));
            fileWrapper.WriteAllText(scriptPath, GetPreparedCommand(commandTemplate));
            _logger.Log($"Generated push script at {scriptPath}", UnityEngine.LogType.Log);

            // Execute push script
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = $"powershell -File \'{scriptPath}\' 2>&1"
                + $"| tee -filePath \'{_scriptLoggingPath}\'";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
        }

        private async Task WaitForProcessToComplete()
        {
            // Monitor execution status
            for (int timeMs = 0; timeMs < timeoutMs; timeMs += periodMs)
            {
                await Task.Delay(periodMs);
                try
                {
                    string log = new FileWrapper().ReadAllText(_scriptLoggingPath);

                    // Detecting failure/success based on logged messages from the script
                    if (log.Contains("has failed."))
                    {
                        _mainThreadContext.Send(_ =>
                            FailStep($"Failed to push image to Amazon ECR due to execution failure. Please check the logs for details. Location: {_scriptLoggingPath}"), null);
                        return;
                    }
                    if (log.Contains("Docker image successfully pushed to Amazon ECR."))
                    {
                        _mainThreadContext.Send(_ => SaveImageTagAndCompleteStep(), null);
                        return;
                    }
                }
                catch (IOException) { /* Catch IO exception in case we open the file while it being written */ }
            }
            _mainThreadContext.Send(_ => FailStep($"Failed to push image to Amazon ECR due to execution timed out. Please check the logs for details. Location: {_scriptLoggingPath}"), null);
        }
    }
}
