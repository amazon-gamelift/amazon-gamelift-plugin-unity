// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;
using static AmazonGameLift.Editor.GameLiftPlugin;

namespace AmazonGameLift.Editor
{
    public class ConfigureDCIStep : ContainerStepComponent
    {
        private const string DefaultImageTag = "unity-gamelift-plugin";
        private const string DockerBuildLogFileName = "BuildDockerContainerImageOutput.txt";
        private DeploymentStepTemplate _stepContent;
        private GameLiftSynchronizationContext _mainThreadContext;
        private string _logOutputDirectory;
        private string _logFilePath;
        private string _dockerfilePath;
        private string _imageTag;
        private Button _viewDockerfileButton;
        private Button _proceedButton;
        private Button _tryAgainButton;
        private Button _viewLogButton;

        public ConfigureDCIStep(VisualElement container, StateManager stateManager): base(container, stateManager, "EditorWindow/Components/Containers/ConfigureDCIStep")
        {
            _stepContent = new DeploymentStepTemplate.Builder(Strings.ContainerConfigureDciStepTitle, Strings.ContainerConfigureDciStepDescription)
                .WithHelpLinks(
                    new DeploymentStepTemplateLink(Urls.DockerHomepage, Strings.ContainerLinksDockerDocumentationLabel),
                    new DeploymentStepTemplateLink(Urls.InstallDockerEngine, Strings.ContainerLinksDockerInstallLabel))
                .WithBaseButtons()
                .Build(container);

            _proceedButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonProceed);
            _tryAgainButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonTryAgain);
            _viewLogButton = _stepContent.ButtonContainer.Q<Button>(DeploymentStepTemplate.BaseButtonViewLogs);
            _viewDockerfileButton = _stepContent.ContentContainer.Q<Button>("ViewDockerfileButton");

            _proceedButton.RegisterCallback<ClickEvent>(_ => { SaveImageTagAndCompleteStep(); });
            _tryAgainButton.RegisterCallback<ClickEvent>(_ => { base.ResetAndTryStart(); });
            _viewLogButton.RegisterCallback<ClickEvent>(_ => { Process.Start($"\"{_logFilePath}\""); });
            _viewDockerfileButton.RegisterCallback<ClickEvent>(_ => { Process.Start($"\"{GetDockerfileFilePath()}\""); });

            Hide(_proceedButton);
            Hide(_tryAgainButton);
            Hide(_viewLogButton);

            Hide(_stepContent.ButtonContainer);
            PopulateContent();

            // Some Unity functions needs to be executed in main thread (called from async functions)
            _mainThreadContext = GameLiftSynchronizationContext.Current;
        }

        protected sealed override void ResetStep()
        {
            Hide(_stepContent.ButtonContainer);
            _stateManager.IsContainerImageBuilt = false;
            _stateManager.IsContainerImageBuilding = false;
            _stateManager.ContainerDockerImageId = null;
            Show(_viewDockerfileButton);
        }

        protected sealed override Task StartOrResumeStep()
        {
            try
            {
                InitializeArgumentsForDocker();
                PopulateContent();
                VerifyDockerArguments();

                if (_stateManager.IsContainerImageBuilt)
                {
                    _mainThreadContext.Send(_ => CompleteStep(), null);
                    return Task.CompletedTask;
                }

                if (_stateManager.IsContainerImageBuilding)
                {
                    ProcessLogFile();
                    return Task.CompletedTask;
                }

                StartDockerBuild();
            }
            catch (Exception e)
            {
                _mainThreadContext.LogError($"Failed to build docker image due to unexpected exception:\n{e}.");
                _mainThreadContext.Send(_ => FailStep(
                    $"Failed to build docker image due to unexpected exception: {e.Message}." +
                    $"\nSee Console for full exception.", false), null);
            }
            return Task.CompletedTask;
        }

        private void InitializeArgumentsForDocker()
        {
            _imageTag = DefaultImageTag;
            _logOutputDirectory = SetupOutputDirectory();
            _logFilePath = Path.Combine(_logOutputDirectory, DockerBuildLogFileName);
            _dockerfilePath = GetDockerfileFilePath();
        }

        private Process StartDockerBuild()
        {
            var baseDirectory = GetBaseDirectory(_stateManager.ContainerGameServerBuildPath);
            var buildDirectory = _stateManager.ContainerGameServerBuildPath.Substring(baseDirectory.Length);

            // Execute docker build
            Process process = new System.Diagnostics.Process();

            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler((_, _) => ProcessLogFile());

            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = $"docker build " +
                $"-f \"{_dockerfilePath}\" " +
                $"--build-arg GAME_BUILD_DIRECTORY=\"{buildDirectory}\" " +
                $"--build-arg GAME_EXECUTABLE=\"{GetRelativeExecutablePath()}\" " +
                $"-t \"{_imageTag}\" " +
                $"{baseDirectory} 2>&1 " +
                $"| tee -filePath \"{_logFilePath}\"";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            _mainThreadContext.Log($"Kicking off docker build process with following command:" +
                $"\n{process.StartInfo.Arguments}\n");

            process.Start();
            _mainThreadContext.Send(_ => UpdateToInProgress(), null);

            Hide(_viewDockerfileButton);

            return process;
        }

        private void ProcessLogFile()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    _mainThreadContext.Send(_ => TriggerWarning(
                        $"Could not find the logs for build process. The file may have been moved." +
                        $"\nExpected location: {_logFilePath}", _logOutputDirectory), null);
                    return;
                }

                var lines = File.ReadLines(_logFilePath);
                Regex errorRegex = new Regex("^#\\d+ ERROR");
                Regex writingImageRegex = new Regex("^#\\d+ writing image|exporting manifest list sha256:");
                bool hitAnError = lines.FirstOrDefault(line => errorRegex.IsMatch(line)) != null;
                bool imageWritten = lines.FirstOrDefault(line => writingImageRegex.IsMatch(line)) != null;

                if (!hitAnError && imageWritten)
                {
                    _mainThreadContext.Send(_ => SaveImageTagAndCompleteStep(), null);
                }
                else if (hitAnError && imageWritten)
                {
                    _mainThreadContext.Send(_ => TriggerWarning(
                        $"Image built with errors. Please check the logs for details." +
                        $"\nLocation: {_logFilePath}"), null);
                }
                else if (hitAnError)
                {
                    _mainThreadContext.Send(_ => FailStep(
                        $"Failed to build docker image due to execution failure. Please check" +
                        $" the logs for details.\nLocation: {_logFilePath}"), null);
                }
                else
                {
                    _mainThreadContext.Send(_ => TriggerWarning(
                        $"Unknown result of docker build process. Please check the logs for details." +
                        $"\nLocation: {_logFilePath}"), null);
                }
            }
            catch (Exception e)
            {
                _mainThreadContext.LogError($"Failed to build docker image due to unexpected exception:\n{e}.");
                _mainThreadContext.Send(_ => FailStep(
                    $"Failed to build docker image due to unexpected exception: {e.Message}." +
                    $"\nSee Console for full exception.", false), null);
            }
        }

        private void PopulateContent()
        {
            Show(_stepContent.ContentContainer);
            _stepContent.ContentContainer.Q<Label>("BuildDirectoryValue").text = DashIfEmpty(_stateManager.ContainerGameServerBuildPath);
            _stepContent.ContentContainer.Q<Label>("BuildExecutableValue").text = DashIfEmpty(GetRelativeExecutablePath());
        }

        private void SaveImageTagAndCompleteStep()
        {
            PopulateContent();
            _stateManager.IsContainerImageBuilt = true;
            _stateManager.ContainerDockerImageId = _imageTag;
            Show(_viewDockerfileButton);

            base.CompleteStep();
        }

        private void TriggerWarning(string warningMessage, string logPath = null)
        {
            base.EncounteredException(
                statusBoxType: StatusBox.StatusBoxType.Warning,
                text: warningMessage,
                externalButtonText: "Show logs",
                externalButtonLink: logPath ?? _logFilePath,
                externalTargetType: StatusBox.StatusBoxExternalTargetType.File
                );
            Show(_stepContent.ButtonContainer);
            Show(_tryAgainButton);
            // If a log path is provided, defer to the alert's button
            ShowHide(_viewLogButton, logPath == null);
            Show(_proceedButton);
            Hide(_viewDockerfileButton);

            PopulateContent();
        }

        private void FailStep(string errorMessage, bool withLogs = true)
        {
            base.EncounteredException(
                statusBoxType: StatusBox.StatusBoxType.Error,
                text: errorMessage,
                externalButtonText: withLogs ? "Show logs" : null,
                externalButtonLink: withLogs ? _logFilePath : null,
                externalTargetType: StatusBox.StatusBoxExternalTargetType.File
                );
            Show(_stepContent.ButtonContainer);
            Show(_tryAgainButton);
            ShowHide(_viewLogButton, withLogs);
            Hide(_proceedButton);
            Hide(_viewDockerfileButton);

            PopulateContent();
        }

        private void UpdateToInProgress()
        {
            Show(_stepContent.ButtonContainer);
            Hide(_tryAgainButton);
            Show(_viewLogButton);
            Hide(_proceedButton);

            _stateManager.IsContainerImageBuilt = false;
            _stateManager.IsContainerImageBuilding = true;

            PopulateContent();
        }

        /**
         * Throw exceptions if any state isn't as expected when starting this step.
         */
        private void VerifyDockerArguments()
        {
            var buildPath = _stateManager.ContainerGameServerBuildPath;
            var buildPathName = "game server build directory";
            var execPath = _stateManager.ContainerGameServerExecutable;
            var execPathName = "game server executable";
            VerifyNonEmpty(buildPath, buildPathName);
            VerifyNonEmpty(execPath, execPathName);
            VerifyAbsolutePath(buildPath, buildPathName);
            // If the executable is an absolute path, it has to be relative to the build directory
            if (Path.IsPathRooted(execPath))
            {
                VerifyPathsAreRelated(buildPath, buildPathName, execPath, execPathName);
            }
            var fullExecPath = Path.Combine(buildPath, GetRelativeExecutablePath());
            VerifyExists(fullExecPath, execPathName);
            VerifyExists(_dockerfilePath, "dockerfile");
        }

        private void VerifyNonEmpty(string path, string pathName)
            => Verify(() => path == null || path == "", $"The {pathName} is missing");

        private void VerifyAbsolutePath(string path, string pathName)
            => Verify(() => !Path.IsPathRooted(path), $"The {pathName} must be an absolute path: {path}");

        private void VerifyPathsAreRelated(string path, string pathName, string relatedPath, string relatedPathName)
            => Verify(() => !relatedPath.Contains(path), $"The {relatedPathName} '{relatedPath}' is not relative to the {pathName} '{path}'");

        private void VerifyExists(string path, string pathName)
            => Verify(() => !File.Exists(path), $"Could not find {pathName} at path '{path}'");

        private void Verify(Func<bool> check, string messageIfFailsCheck)
        {
            if (check.Invoke())
            {
                throw new InvalidOperationException(messageIfFailsCheck);
            }
        }

        private string GetRelativeExecutablePath()
        {
            var buildDirectory = _stateManager.ContainerGameServerBuildPath;
            var executablePath = _stateManager.ContainerGameServerExecutable;
            if (buildDirectory == null || executablePath == null)
            {
                return null;
            }

            /**
             * If the executable is an absolute path, split off the build directory from it's path.
             * Note, there is verification separately for whether the paths are relative to each other,
             * for now just avoid exceptions by also checking that the directory is contained in the exec path.
             */
            if (Path.IsPathRooted(executablePath) && executablePath.Contains(buildDirectory))
            {
                return "." + executablePath.Substring(buildDirectory.Length);
            }

            return executablePath;
        }

        private string SetupOutputDirectory()
        {
            var fileWrapper = new FileWrapper();
            string containersPath = PathConverter.SharedInstance.GetContainersAbsolutePath();

            // Prepare output directory
            string containersOutputDirectory = Path.Combine(containersPath, Paths.ContainersOutputFolderName);
            if (!fileWrapper.DirectoryExists(containersOutputDirectory))
            {
                fileWrapper.CreateDirectory(containersOutputDirectory);
            }
            return containersOutputDirectory;
        }

        private string GetDockerfileFilePath()
        {
            string containersPath = PathConverter.SharedInstance.GetContainersAbsolutePath();
            return Path.Combine(containersPath, Paths.ContainerDockerfileFileName);
        }

        /**
         * Returns the directory one level below the provided directory.
         * Similar to 'Path.GetDirectoryName' except for the case with trailing /.
         * Example:
         * GetBaseDirectory('/root/mydir/')      -> '/root'
         * GetBaseDirectory('/root/mydir')       -> '/root'
         * Path.GetDirectoryName('/root/mydir/') -> '/root/mydir'
         * Path.GetDirectoryName('/root/mydir')  -> '/root'
         */
        private string GetBaseDirectory(string directoryPath)
        {
            var potentialContainingDirectory = Path.GetDirectoryName(directoryPath);
            /**
             * GetDirectoryName('/root/mydir/') -> '/root/mydir'
             * GetDirectoryName('/root/mydir') -> '/root'
             * If the string changed by 1 char, we know the method removed just the /
             * and not a level of the directory. Calling once more will remove the top
             * level directory.
             * Note even if the top dir has 1 char, the first call would remove 2 char.
             * GetDirectoryName('/root/a') -> '/root'
             */
            if (potentialContainingDirectory.Length + 1 == directoryPath.Length)
            {
                return Path.GetDirectoryName(potentialContainingDirectory);
            }
            return potentialContainingDirectory;
        }
    }
}
