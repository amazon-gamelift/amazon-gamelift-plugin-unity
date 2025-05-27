// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2Deployment
    {
        private readonly EC2DeploymentSettings _deploymentSettings;
        
        internal ManagedEC2Deployment(EC2DeploymentSettings deploymentSettings)
        {
            _deploymentSettings = deploymentSettings;
        }
        
        public void UpdateModelFromParameters(ManagedEC2FleetParameters parameters)
        {
            _deploymentSettings.GameName = parameters.GameName;
            _deploymentSettings.FleetName = parameters.FleetName;
            _deploymentSettings.BuildName = parameters.BuildName;
            _deploymentSettings.LaunchParameters = parameters.LaunchParameters;
            _deploymentSettings.BuildFolderPath = parameters.GameServerFolder;
            _deploymentSettings.BuildFilePath = parameters.GameServerFile;
            _deploymentSettings.BuildOperatingSystem = parameters.OperatingSystem;
        }
        
        public void StartDeployment()
        {
            _deploymentSettings.Save();
            _deploymentSettings.StartDeployment(ConfirmChanges).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
            });
        }

        public async Task DeleteDeployment()
        {
            await _deploymentSettings.DeleteDeployment();
        }

        private Task<bool> ConfirmChanges(ConfirmChangesRequest request)
        {
            var stackUpdateModelFactory = new StackUpdateModelFactory(new ChangeSetUrlFormatter());

            StackUpdateDialog dialog = EditorWindow.GetWindow<StackUpdateDialog>();
            return dialog.SetUp(stackUpdateModelFactory.Create(request));
        }
    }
}