// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using UnityEditor;
using UnityEngine;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// The main backend for deployment to AWS.
    /// </summary>
    internal class DeploymentSettings
    {
        private const int StackInfoRefreshDelayMs = 2000;

        private readonly Dictionary<DeploymentScenarios, DeployerBase> _deployers =
            new Dictionary<DeploymentScenarios, DeployerBase>();

        private readonly ScenarioLocator _scenarioLocator;
        private readonly PathConverter _pathConverter;
        private readonly CoreApi _coreApi;
        private readonly ScenarioParametersUpdater _parametersUpdater;
        private readonly TextProvider _textProvider;
        private readonly DeploymentWaiter _deploymentWaiter;
        private readonly DelayedOperation _delayedStackInfoRefresh;
        private DeploymentScenarios _scenario;
        private string _gameName;
        private DeploymentStackInfo _currentStackInfo;
        private readonly IDeploymentIdContainer _currentDeploymentId;
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;
        private readonly Status _status = new Status();

        public IReadStatus Status => _status;

        #region Bootstrap parameters

        public string CurrentProfile { get; private set; }

        public string CurrentRegion { get; private set; }

        public string CurrentBucketName { get; private set; }

        public bool HasCurrentBucket { get; private set; }

        public bool IsBootstrapped => CurrentProfile != null && HasCurrentBucket;

        #endregion

        public string[] AllScenarios { get; private set; } = new string[0];

        public string ScenarioName { get; private set; }

        public string ScenarioPath { get; private set; }

        public string ScenarioDescription { get; private set; }

        public string ScenarioHelpUrl { get; private set; }

        #region Scenario parameters

        public string GameName
        {
            get => _gameName;
            set => _ = SetGameNameAsync(value);
        }

        public string BuildFolderPath { get; set; }

        public string BuildFilePath { get; set; }

        public string BuildOperatingSystem { get; set; }

        public string FleetName { get; set; }

        public string BuildName { get; set; }

        public string LaunchParameters { get; set; }

        #endregion Scenario parameters

        public DeploymentScenarios Scenario
        {
            get => _scenario;
            set
            {
                if (_scenario == value)
                {
                    return;
                }

                _scenario = value;
                RefreshScenario();
            }
        }

        public bool IsFormFilled => !string.IsNullOrWhiteSpace(GameName)
                                    && IsValidScenarioIndex
                                    && IsBuildFolderPathFilled
                                    && IsBuildFilePathFilled;

        public bool IsBuildFilePathFilled => IsValidScenarioIndex
                                             && (!_deployers[Scenario].HasGameServer ||
                                                 _coreApi.FileExists(BuildFilePath));

        public bool IsBuildFolderPathFilled => IsValidScenarioIndex
                                               && (!_deployers[Scenario].HasGameServer ||
                                                   _coreApi.FolderExists(BuildFolderPath));

        public bool IsBuildRequired =>
            IsValidScenarioIndex && _deployers[Scenario].HasGameServer;

        public bool IsValidScenarioIndex => Enum.IsDefined(typeof(DeploymentScenarios), _scenario) &&
                                            _deployers.ContainsKey(_scenario);

        public bool DoesDeploymentExist { get; private set; }

        public bool HasCurrentStack => CurrentStackInfo.Details != null;

        public DeploymentStackInfo CurrentStackInfo
        {
            get => _currentStackInfo;
            private set
            {
                _currentStackInfo = value;
                CurrentStackInfoChanged?.Invoke();
            }
        }

        public bool IsDeploymentRunning { get; private set; }

        public bool CanCancel => _deploymentWaiter.CanCancel == true;

        public bool CanEdit => !IsDeploymentRunning && IsBootstrapped && IsCurrentStackModifiable;

        public bool CanDeploy => !IsDeploymentRunning && IsBootstrapped && IsFormFilled && IsCurrentStackModifiable;

        public bool IsCurrentStackModifiable =>
            CurrentStackInfo.StackStatus == null || CurrentStackInfo.StackStatus.IsStackStatusModifiable();

        public bool CanDelete => CurrentStackInfo.StackStatus != null && CurrentStackInfo.StackStatus.IsStackStatusOperationDone();

        public event Action CurrentStackInfoChanged;

        internal DeploymentSettings(ScenarioLocator scenarioLocator, PathConverter pathConverter,
            CoreApi coreApi, ScenarioParametersUpdater parametersUpdater, TextProvider textProvider,
            DeploymentWaiter deploymentWaiter, IDeploymentIdContainer currentDeploymentId, Delay delay, ILogger logger,
            StateManager stateManager)
        {
            _scenarioLocator = scenarioLocator ?? throw new ArgumentNullException(nameof(scenarioLocator));
            _pathConverter = pathConverter ?? throw new ArgumentNullException(nameof(pathConverter));
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _parametersUpdater = parametersUpdater ?? throw new ArgumentNullException(nameof(parametersUpdater));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _deploymentWaiter = deploymentWaiter ?? throw new ArgumentNullException(nameof(deploymentWaiter));
            _currentDeploymentId = currentDeploymentId ?? throw new ArgumentNullException(nameof(currentDeploymentId));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateManager = stateManager;
            ClearCurrentStackInfo();
            _delayedStackInfoRefresh = new DelayedOperation(RefreshCurrentStackInfo, delay, StackInfoRefreshDelayMs);
        }

        public async Task SetGameNameAsync(string value)
        {
            if (_gameName == value)
            {
                return;
            }

            _gameName = value;
            ClearCurrentStackInfo();
            await _delayedStackInfoRefresh.Request();
        }

        public void Refresh()
        {
            var deployers = _scenarioLocator.GetScenarios().ToList();
            _deployers.Clear();
            for (var i = 0; i < deployers.Count; i++)
            {
                _deployers.Add((DeploymentScenarios)i, deployers[i]);
            }

            AllScenarios = _deployers.Values.Select(deployer => deployer.DisplayName).ToArray();
            CurrentBucketName = _stateManager.BucketName;
            CurrentRegion = _stateManager.Region;
            CurrentProfile = _stateManager.ProfileName;

            HasCurrentBucket = !string.IsNullOrEmpty(CurrentBucketName) && _coreApi.IsValidRegion(CurrentRegion);

            RefreshScenario();
        }

        public void RefreshCurrentStackInfo()
        {
            if (IsDeploymentRunning)
            {
                return;
            }

            if (string.IsNullOrEmpty(GameName))
            {
                ClearCurrentStackInfo();
                return;
            }

            if (string.IsNullOrEmpty(CurrentProfile))
            {
                _logger.Log(string.Format(DevStrings.FailedToDescribeStackTemplate, DevStrings.ProfileInvalid),
                    LogType.Warning);
                ClearCurrentStackInfo();
                return;
            }

            if (!_coreApi.IsValidRegion(CurrentRegion))
            {
                _logger.Log(string.Format(DevStrings.FailedToDescribeStackTemplate, DevStrings.RegionInvalid),
                    LogType.Warning);
                ClearCurrentStackInfo();
                return;
            }

            string stackName = _coreApi.GetStackName(GameName);
            DescribeStackResponse describeResponse = _coreApi.DescribeStack(CurrentProfile, CurrentRegion, stackName);

            if (!describeResponse.Success)
            {
                ClearCurrentStackInfo();
                return;
            }

            CurrentStackInfo = DeploymentStackInfoFactory.Create(_textProvider, describeResponse, CurrentRegion, ScenarioName);
        }

        public void Restore()
        {
            Scenario = DeploymentScenarios.SingleRegion; // Selects "Single-Region Fleet" Deployment Scenario by default
            BuildFilePath = null;
            BuildFolderPath = null;

            Scenario = _stateManager.DeploymentScenario;
            GameName = _stateManager.DeploymentGameName;
            BuildFolderPath = _stateManager.DeploymentBuildFolderPath;
            BuildFilePath = _stateManager.DeploymentBuildFilePath;
            LaunchParameters = _stateManager.LaunchParameters;
            BuildOperatingSystem = _stateManager.BuildOperatingSystem;
            FleetName = _stateManager.ManagedEC2FleetName;
            BuildName = _stateManager.BuildName;
        }

        public void Save()
        {
            _stateManager.DeploymentScenario = Scenario;
            _stateManager.DeploymentBuildFolderPath = BuildFolderPath;
            _stateManager.DeploymentBuildFilePath = BuildFilePath;
            _stateManager.DeploymentGameName = GameName;
            _stateManager.LaunchParameters = LaunchParameters;
            _stateManager.BuildOperatingSystem = BuildOperatingSystem;
            _stateManager.ManagedEC2FleetName = FleetName;
            _stateManager.BuildName = BuildName;
        }

        public async Task WaitForCurrentDeployment()
        {
            if (IsDeploymentRunning || !_currentDeploymentId.HasValue)
            {
                return;
            }

            try
            {
                IsDeploymentRunning = true;
                _delayedStackInfoRefresh.Cancel();
                _deploymentWaiter.InfoUpdated += OnDeploymentWaiterInfoUpdated;
                DeploymentResponse response = await _deploymentWaiter.WaitUntilDone(_currentDeploymentId.Get());
                LogWaitResponse(response);

                if (response.ErrorCode != ErrorCode.OperationCancelled)
                {
                    _currentDeploymentId.Clear();
                }
            }
            catch (Exception)
            {
                _currentDeploymentId.Clear();
                CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusExceptionThrown));
                throw;
            }
            finally
            {
                IsDeploymentRunning = false;
                _deploymentWaiter.InfoUpdated -= OnDeploymentWaiterInfoUpdated;
            }
        }

        private void LogWaitResponse(DeploymentResponse response)
        {
            if (response.Success || response.ErrorCode == ErrorCode.OperationCancelled)
            {
                return;
            }

            if (response.ErrorCode == ErrorCode.StackStatusInvalid
                || response.ErrorCode == CoreErrorCode.StackDoesNotExist)
            {
                _logger.LogResponseError(response, LogType.Log);
            }
            else
            {
                _logger.LogResponseError(response);
            }
        }

        public void CancelWaitingForDeployment()
        {
            _deploymentWaiter.CancelWaiting();
        }

        public void CancelDeployment()
        {
            if (!CanCancel)
            {
                _logger.Log(DevStrings.OperationInvalid, LogType.Warning);
                return;
            }

            Response response = _deploymentWaiter.CancelDeployment();

            if (response.Success)
            {
                RefreshCurrentStackInfo();
            }
            else
            {
                _logger.LogResponseError(response);
            }
        }

        public async Task StartDeployment(ConfirmChangesDelegate confirmChanges)
        {
            if (confirmChanges is null)
            {
                throw new ArgumentNullException(nameof(confirmChanges));
            }

            if (!IsFormFilled || !CanDeploy)
            {
                return;
            }

            string exeFilePath = null;
            DeployerBase currentDeployer = _deployers[Scenario];

            if (currentDeployer.HasGameServer)
            {
                exeFilePath = GetExeFilePathInBuildOrNull();

                if (exeFilePath == null)
                {
                    _status.IsDisplayed = true;
                    _status.SetMessage(_textProvider.Get(Strings.StatusDeploymentExePathInvalid), MessageType.Error);
                    return;
                }
            }

            if (IsDeploymentRunning)
            {
                return;
            }

            IsDeploymentRunning = true;
            _delayedStackInfoRefresh.Cancel();
            string parametersPath = _pathConverter.GetParametersFilePath(ScenarioPath);
            IReadOnlyDictionary<string, string> parameters = currentDeployer.HasGameServer
                ? PrepareParameters(exeFilePath)
                : PrepareGameParameter();
            var parameterUpdateResponse = _parametersUpdater.Update(parametersPath, parameters);
            if (!parameterUpdateResponse.Success)
            {
                _status.IsDisplayed = true;
                _status.SetMessage(parameterUpdateResponse.ErrorMessage, MessageType.Error);
                _logger.LogResponseError(parameterUpdateResponse);
                return;
            }
            CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusDeploymentStarting));
            string stackName = _coreApi.GetStackName(GameName);
            var deploymentId = new DeploymentId(CurrentProfile, CurrentRegion, stackName, currentDeployer.DisplayName);
            _currentDeploymentId.Set(deploymentId);

            try
            {
                DeploymentResponse response = await currentDeployer.StartDeployment(ScenarioPath, BuildFolderPath,
                    GameName, isDevelopmentBuild: EditorUserBuildSettings.development, confirmChanges);

                if (!response.Success)
                {
                    if (response.ErrorCode != ErrorCode.OperationCancelled)
                    {
                        _logger.LogResponseError(response);
                        string messageTemplate = _textProvider.Get(Strings.StatusDeploymentFailure);
                        string message = string.Format(messageTemplate, _textProvider.GetError(response.ErrorCode));
                        _status.SetMessage(message, MessageType.Error);
                        _status.IsDisplayed = true;
                    }

                    return;
                }

                _deploymentWaiter.InfoUpdated += OnDeploymentWaiterInfoUpdated;
                response = await _deploymentWaiter.WaitUntilDone(deploymentId);
                LogWaitResponse(response);

                if (response.ErrorCode != ErrorCode.OperationCancelled)
                {
                    _currentDeploymentId.Clear();
                }
            }
            catch (Exception ex)
            {
                _currentDeploymentId.Clear();
                _logger.LogException(ex);
                string messageTemplate = _textProvider.Get(Strings.StatusDeploymentFailure);
                string message = string.Format(messageTemplate, ex.Message);
                _status.SetMessage(message, MessageType.Error);
                _status.IsDisplayed = true;
                throw;
            }
            finally
            {
                IsDeploymentRunning = false;
                _deploymentWaiter.InfoUpdated -= OnDeploymentWaiterInfoUpdated;
                RefreshCurrentStackInfo();
            }
        }

        public async Task DeleteDeployment()
        {
            var stackName = _coreApi.GetStackName(_gameName);
            _coreApi.DeleteStack(CurrentProfile, CurrentRegion, stackName);
            _currentDeploymentId.Set(new DeploymentId(CurrentProfile, CurrentRegion, stackName, _deployers[Scenario].DisplayName));
            await WaitForCurrentDeployment();
        }

        private void OnDeploymentWaiterInfoUpdated(DeploymentInfo info)
        {
            CurrentStackInfo = DeploymentStackInfoFactory.Create(_textProvider, info);
        }

        private IReadOnlyDictionary<string, string> PrepareGameParameter()
        {
            return new Dictionary<string, string>
            {
                { ScenarioParameterKeys.GameName, GameName }
            };
        }

        private IReadOnlyDictionary<string, string> PrepareParameters(string exeFilePathInBuild)
        {
            string launchPath = _coreApi.GetServerGamePath(exeFilePathInBuild);
            var parameters = new Dictionary<string, string>
            {
                { ScenarioParameterKeys.GameName, GameName },
                { ScenarioParameterKeys.LaunchPath, launchPath },
                { ScenarioParameterKeys.BuildOperatingSystem, BuildOperatingSystem },
                { ScenarioParameterKeys.FleetName, FleetName },
                { ScenarioParameterKeys.BuildName, BuildName },
            };
            if (!string.IsNullOrWhiteSpace(LaunchParameters))
            {
                parameters.Add(ScenarioParameterKeys.LaunchParameters, LaunchParameters);
            }

            return parameters;
        }

        private string GetExeFilePathInBuildOrNull()
        {
            int index = BuildFilePath.IndexOf(BuildFolderPath);

            if (index < 0)
            {
                return null;
            }

            return BuildFilePath.Remove(0, BuildFolderPath.Length).TrimStart('\\', '/');
        }

        private void RefreshScenario()
        {
            if (!IsValidScenarioIndex)
            {
                return;
            }

            DeployerBase deployer = _deployers[Scenario];
            ScenarioName = deployer.DisplayName;
            ScenarioDescription = deployer.Description;
            ScenarioHelpUrl = deployer.HelpUrl;
            ScenarioPath = _pathConverter.GetScenarioAbsolutePath(deployer.ScenarioFolder);

            if (!deployer.HasGameServer)
            {
                BuildFolderPath = null;
                BuildFilePath = null;
            }

            RefreshCurrentStackInfo();
        }

        private void ClearCurrentStackInfo()
        {
            CurrentStackInfo = new DeploymentStackInfo(_textProvider.Get(Strings.StatusNothingDeployed));
        }
    }
}