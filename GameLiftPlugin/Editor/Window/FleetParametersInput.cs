using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using OperatingSystem = Amazon.GameLift.OperatingSystem;

namespace AmazonGameLift.Editor
{
    public class FleetParametersInput
    {
        private static readonly Dictionary<string, OperatingSystem> OSMappings = new()
        {
            { "Amazon Linux 2 (AL2)", OperatingSystem.AMAZON_LINUX_2 },
            { "Amazon Linux 2023 (AL2023)", OperatingSystem.AMAZON_LINUX_2023 },
            { "Windows Server 2016", OperatingSystem.WINDOWS_2016 },
        };

        private readonly ManagedEC2FleetParameters _parameters;
        private readonly VisualElement _container;
        private readonly TextField _gameNameInput;
        private readonly TextField _fleetNameInput;
        private readonly TextField _buildNameInput;
        private readonly TextField _launchParamsInput;
        private readonly TextField _serverFolderInput;
        private readonly TextField _serverFileInput;
        private readonly DropdownField _osDropdown;
        private readonly Button _serverFolderButton;
        private readonly Button _serverFileButton;

        public Action<ManagedEC2FleetParameters> OnValueChanged;

        public FleetParametersInput(VisualElement container, ManagedEC2FleetParameters parameters)
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/DeploymentParameters");
            container.Add(uxml.Instantiate());
            _container = container;
            _parameters = parameters;

            _gameNameInput = SetupInput("ManagedEC2ParametersGameNameInput", parameters.GameName,
                value => parameters.GameName = value);
            _fleetNameInput = SetupInput("ManagedEC2ParametersFleetNameInput", parameters.FleetName,
                value => parameters.FleetName = value);
            _buildNameInput = SetupInput("ManagedEC2ParametersBuildNameInput", parameters.BuildName,
                value => parameters.BuildName = value);
            _launchParamsInput = SetupInput("ManagedEC2ParametersLaunchParametersInput", parameters.LaunchParameters,
                value => parameters.LaunchParameters = value);
            _serverFolderInput = SetupInput("ManagedEC2ParametersGameServerFolderInput", parameters.GameServerFolder,
                value => parameters.GameServerFolder = value);
            _serverFileInput = SetupInput("ManagedEC2ParametersGameServerFileInput", parameters.GameServerFile,
                value => parameters.GameServerFile = value);

            _osDropdown = container.Q<DropdownField>("ManagedEC2ParametersOperatingSystemInput");
            _osDropdown.choices = OSMappings.Keys.ToList();
            _osDropdown.index = OSMappings.Values.ToList().IndexOf(parameters.OperatingSystem);
            _osDropdown.RegisterValueChangedCallback(e =>
            {
                _parameters.OperatingSystem = OSMappings[e.newValue];
                OnValueChanged(_parameters);
            });

            _serverFolderButton = container.Q<Button>("ManagedEC2ParametersGameServerFolderButton");
            _serverFolderButton.RegisterCallback<ClickEvent>(_ =>
            {
                var popupTitle = new TextProvider().Get(Strings.ManagedEC2ParametersGameServerFolderPath);
                var value = EditorUtility.OpenFolderPanel(popupTitle, Application.dataPath,
                    _parameters.GameServerFolder);
                _parameters.GameServerFolder = value;
                _serverFolderInput.value = value;
                OnValueChanged(_parameters);
            });

            _serverFileButton = container.Q<Button>("ManagedEC2ParametersGameServerFileButton");
            _serverFileButton.RegisterCallback<ClickEvent>(_ =>
            {
                var popupTitle = new TextProvider().Get(Strings.ManagedEC2ParametersGameServerFilePath);
                var value = EditorUtility.OpenFilePanel(popupTitle, _parameters.GameServerFolder, "" );
                _parameters.GameServerFile = value;
                _serverFileInput.value = value;
                OnValueChanged(_parameters);
            });

            LocalizeText();
        }

        private TextField SetupInput(string inputName, string initialValue, Action<string> onChangeEvent)
        {
            var input = _container.Q<TextField>(inputName);
            input.value = initialValue;
            input.RegisterValueChangedCallback(e =>
            {
                onChangeEvent(e.newValue);
                OnValueChanged(_parameters);
            });
            return input;
        }

        public void SetEnabled(bool value)
        {
            _gameNameInput.SetEnabled(value);
            _fleetNameInput.SetEnabled(value);
            _buildNameInput.SetEnabled(value);
            _launchParamsInput.SetEnabled(value);
            _serverFolderInput.SetEnabled(value);
            _serverFileInput.SetEnabled(value);
            _osDropdown.SetEnabled(value);
            _serverFolderButton.SetEnabled(value);
            _serverFileButton.SetEnabled(value);
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(_container);
            l.SetElementText("ManagedEC2ParametersGameNameLabel", Strings.ManagedEC2ParametersGameNameLabel);
            l.SetElementText("ManagedEC2ParametersFleetNameLabel", Strings.ManagedEC2ParametersFleetNameLabel);
            l.SetElementText("ManagedEC2ParametersBuildNameLabel", Strings.ManagedEC2ParametersBuildNameLabel);
            l.SetElementText("ManagedEC2ParametersLaunchParametersLabel", Strings.ManagedEC2ParametersLaunchParametersLabel);
            l.SetElementText("ManagedEC2ParametersOperatingSystemLabel", Strings.ManagedEC2ParametersOperatingSystemLabel);
            l.SetElementText("ManagedEC2ParametersGameServerFolderLabel", Strings.ManagedEC2ParametersGameServerFolderLabel);
            l.SetElementText("ManagedEC2ParametersGameServerFileLabel", Strings.ManagedEC2ParametersGameServerFileLabel);
        }
    }
}
