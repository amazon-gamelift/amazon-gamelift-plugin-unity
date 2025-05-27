// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLift.Editor; 
using AmazonGameLiftPlugin.Core;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using YamlDotNet.Serialization;

namespace AmazonGameLift.Editor
{
    public class StateManager
    {
        public CoreApi CoreApi { get; }

        public GameLiftFleetManager FleetManager { get; set; }
        public GameLiftCGDManager CGDManager { get; set; }
        public GameLiftComputeManager ComputeManager { get; set; }

        public IAmazonGameLiftWrapper GameLiftWrapper { get; private set; }
        public IAmazonGameLiftWrapperFactory AmazonGameLiftWrapperFactory { get; }
        
        private UserProfile _selectedProfile = new UserProfile();
        public List<UserProfile> _allProfiles;
        private readonly ISerializer _serializer = new SerializerBuilder().Build();
        private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();
        public UserProfile SelectedProfile => _selectedProfile;

        private string _selectedRadioButton;
        public string SelectedRadioButton
        {
            get => _selectedRadioButton;
            set
            {
                _selectedRadioButton = value;
                OnProfileRadioButtonChanged?.Invoke();
            }
        }
        public string LastOpenTab
        {
            get => CoreApi.GetSetting(SettingsKeys.LastOpenTab).Value;
            set
            {
                CoreApi.PutSetting(SettingsKeys.LastOpenTab, value);
            }
        }

        #region Profile Settings

        public virtual string ProfileName => _selectedProfile?.Name;

        public virtual string Region
        {
            get => _selectedProfile.Region;
            set
            {
                _selectedProfile.Region = value;
                SaveProfiles();
                CoreApi.PutSetting(SettingsKeys.CurrentRegion, value);
            }
        }

        public string BucketName
        {
            get => _selectedProfile.BucketName;
            set
            {
                _selectedProfile.BucketName = value;
                SaveProfiles();
                CoreApi.PutSetting(SettingsKeys.CurrentBucketName, value);
            }
        }

        #endregion

        #region Anywhere Settings

        public string AnywhereFleetName
        {
            get => _selectedProfile.AnywhereFleetName;
            set
            {
                _selectedProfile.AnywhereFleetName = value;
                SaveProfiles();
            }
        }

        public string AnywhereFleetId
        {
            get => _selectedProfile.AnywhereFleetId;
            set
            {
                _selectedProfile.AnywhereFleetId = value;
                SaveProfiles();
                OnFleetChanged?.Invoke();
            }
        }

        public string AnywhereFleetLocation
        {
            get => _selectedProfile.AnywhereFleetLocation;
            set
            {
                _selectedProfile.AnywhereFleetLocation = value;
                SaveProfiles();
            }
        }

        public string ComputeName
        {
            get => _selectedProfile.ComputeName;
            set
            {
                _selectedProfile.ComputeName = value;
                SaveProfiles();
                OnComputeChanged?.Invoke();
            }
        }


        public string IpAddress
        {
            get => _selectedProfile.IpAddress;
            set
            {
                _selectedProfile.IpAddress = value;
                SaveProfiles();
            }
        }


        public string WebSocketUrl
        {
            get => _selectedProfile.WebSocketUrl;
            set
            {
                _selectedProfile.WebSocketUrl = value;
                SaveProfiles();
            }
        }

        #endregion

        #region Container Settings

        public bool ContainerDeploymentInProgress
        {
            get => _selectedProfile.ContainerDeploymentInProgress;
            set
            {
                _selectedProfile.ContainerDeploymentInProgress = value;
                SaveProfiles();
            }
        }

        public bool ContainersDeploymentComplete
        {
            get => _selectedProfile.ContainersDeploymentComplete;
            set
            {
                _selectedProfile.ContainersDeploymentComplete = value;
                SaveProfiles();
                OnContainersDeploymentStatusChanged?.Invoke();
            }
        }

        public bool IsCGDDeploying
        {
            get => _selectedProfile.IsCGDDeploying;
            set
            {
                _selectedProfile.IsCGDDeploying = value;
                SaveProfiles();
            }
        }

        public bool IsCGDDeployed
        {
            get => _selectedProfile.IsCGDDeployed;
            set
            {
                _selectedProfile.IsCGDDeployed = value;
                SaveProfiles();
            }
        }

        public ContainerScenarios ContainerQuestionnaireScenario
        {
            get => _selectedProfile.ContainerQuestionnaireScenario;
            set
            {
                _selectedProfile.ContainerQuestionnaireScenario = value;
                SaveProfiles();
            }
        }

        public DeploymentScenarios ContainerDeploymentScenario
        {
            get => _selectedProfile.ContainerDeploymentScenario;
            set
            {
                _selectedProfile.ContainerDeploymentScenario = value;
                SaveProfiles();
            }
        }

        public string ContainerGameServerBuildPath
        {
            get => _selectedProfile.ContainerGameServerBuildPath;
            set
            {
                _selectedProfile.ContainerGameServerBuildPath = value;
                SaveProfiles();
            }
        }

        public string ContainerGameServerExecutable
        {
            get => _selectedProfile.ContainerGameServerExecutable;
            set
            {
                _selectedProfile.ContainerGameServerExecutable = value;
                SaveProfiles();
            }
        }
        
        public string ContainerDockerImageId
        {
            get => _selectedProfile.ContainerDockerImageId;
            set
            {
                _selectedProfile.ContainerDockerImageId = value;
                SaveProfiles();
            }
        } 
        
        public string ContainerECRRepositoryName
        {
            get => _selectedProfile.ContainerECRRepositoryName;
            set
            {
                _selectedProfile.ContainerECRRepositoryName = value;
                SaveProfiles();
            }
        }

        public string ContainerECRRepositoryUri
        {
            get => _selectedProfile.ContainerECRRepositoryUri;
            set
            {
                _selectedProfile.ContainerECRRepositoryUri = value;
                SaveProfiles();
            }
        }

        public string ContainerECRImageId
        {
            get => _selectedProfile.ContainerECRImageId;
            set
            {
                _selectedProfile.ContainerECRImageId = value;
                SaveProfiles();
            }
        }

        public string ContainerECRImageUri
        {
            get => _selectedProfile.ContainerECRImageUri;
            set
            {
                _selectedProfile.ContainerECRImageUri = value;
                SaveProfiles();
            }
        }

        public string ContainerPortRange
        {
            get => _selectedProfile.ContainerPortRange;
            set
            {
                _selectedProfile.ContainerPortRange = value;
                SaveProfiles();
            }
        }

        public string ContainerGameName
        {
            get => _selectedProfile.ContainerGameName;
            set
            {
                _selectedProfile.ContainerGameName = value;
                SaveProfiles();
            }
        }

        public string ContainerTotalMemory
        {
            get => _selectedProfile.ContainerTotalMemory;
            set
            {
                _selectedProfile.ContainerTotalMemory = value;
                SaveProfiles();
            }
        }

        public string ContainerTotalVcpu
        {
            get => _selectedProfile.ContainerTotalVcpu;
            set
            {
                _selectedProfile.ContainerTotalVcpu = value;
                SaveProfiles();
            }
        }

        public string ContainerImageTag
        {
            get => _selectedProfile.ContainerImageTag;
            set
            {
                _selectedProfile.ContainerImageTag = value;
                SaveProfiles();
            }
        }

        public bool IsContainerImageBuilding
        {
            get => _selectedProfile.IsContainerImageBuilding;
            set
            {
                _selectedProfile.IsContainerImageBuilding = value;
                SaveProfiles();
            }
        }

        public bool IsContainerImageBuilt
        {
            get => _selectedProfile.IsContainerImageBuilt;
            set
            {
                _selectedProfile.IsContainerImageBuilt = value;
                SaveProfiles();
            }
        }

        public bool IsECRRepoCreated
        {
            get => _selectedProfile.IsECRRepoCreated;
            set
            {
                _selectedProfile.IsECRRepoCreated = value;
                SaveProfiles();
            }
        }

        public bool IsContainerPushedToECR
        {
            get => _selectedProfile.IsContainerPushedToECR;
            set
            {
                _selectedProfile.IsContainerPushedToECR = value;
                SaveProfiles();
            }
        }

        #endregion

        #region Managed EC2 Settings

        public DeploymentScenarios DeploymentScenario
        {
            get => _selectedProfile.DeploymentScenario;
            set
            {
                _selectedProfile.DeploymentScenario = value;
                SaveProfiles();
            }
        }

        public string DeploymentGameName
        {
            get => _selectedProfile.DeploymentGameName;
            set
            {
                _selectedProfile.DeploymentGameName = value;
                SaveProfiles();
            }
        }

        public string ManagedEC2FleetName
        {
            get => _selectedProfile.ManagedEC2FleetName;
            set
            {
                _selectedProfile.ManagedEC2FleetName = value;
                SaveProfiles();
            }
        }

        public string BuildName
        {
            get => _selectedProfile.BuildName;
            set
            {
                _selectedProfile.BuildName = value;
                SaveProfiles();
            }
        }

        public string LaunchParameters
        {
            get => _selectedProfile.LaunchParameters;
            set
            {
                _selectedProfile.LaunchParameters = value;
                SaveProfiles();
            }
        }

        public string BuildOperatingSystem
        {
            get => _selectedProfile.BuildOperatingSystem;
            set
            {
                _selectedProfile.BuildOperatingSystem = value;
                SaveProfiles();
            }
        }

        public string DeploymentBuildFilePath
        {
            get => _selectedProfile.DeploymentBuildFilePath;
            set
            {
                _selectedProfile.DeploymentBuildFilePath = value;
                SaveProfiles();
            }
        }

        public string DeploymentBuildFolderPath
        {
            get => _selectedProfile.DeploymentBuildFolderPath;
            set
            {
                _selectedProfile.DeploymentBuildFolderPath = value;
                SaveProfiles();
            }
        }

        #endregion

        public IReadOnlyList<string> AllProfiles => CoreApi.ListCredentialsProfiles().Profiles.ToList();

        public bool IsBootstrapped() => !string.IsNullOrWhiteSpace(_selectedProfile?.Name) &&
                                      !string.IsNullOrWhiteSpace(_selectedProfile?.Region) &&
                                      !string.IsNullOrWhiteSpace(_selectedProfile?.BucketName);

        public bool IsInContainersRegion() => ContainersRegions.isContainersRegion(_selectedProfile?.Region);

        public bool IsBootstrapped(UserProfile profile)
        {
            return !string.IsNullOrWhiteSpace(profile.Name) &&
                !string.IsNullOrWhiteSpace(profile.Region) &&
                !string.IsNullOrWhiteSpace(profile.BucketName);
        }

        public Action OnUserProfileUpdated { get; set; }
        public Action OnFleetChanged { get; set; }
        public Action OnComputeChanged { get; set; }
        public Action OnClientSettingsChanged { get; set; }
        public Action OnAddAnotherProfile { get; set; }
        public Action OnContainerQuestionnaireScenarioChanged { get; set; }
        public Action OnContainersDeploymentStatusChanged { get; set; }
        public Action OnProfileRadioButtonChanged { get; set; }

        public StateManager(CoreApi coreApi)
        {
            CoreApi = coreApi;
            AmazonGameLiftWrapperFactory = new AmazonGameLiftWrapperFactory(coreApi);
            RefreshProfiles();
            SyncProfileStores();
            SetProfile(coreApi.GetSetting(SettingsKeys.CurrentProfileName).Value);
        }

        public void SetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName) || profileName == ProfileName) return;
            _selectedProfile = _allProfiles.FirstOrDefault(profile => profile.Name == profileName);
            if (_selectedProfile == null)
            {
                _selectedProfile = new UserProfile()
                {
                    Name = profileName,
                };
                _allProfiles.Add(_selectedProfile);
                SaveProfiles();
            }

            CoreApi.PutSetting(SettingsKeys.CurrentProfileName, profileName);
            var credentials = CoreApi.RetrieveAwsCredentials(profileName);
            Region = credentials.Region;
            BucketName = _selectedProfile.BucketName;
            GameLiftWrapper = AmazonGameLiftWrapperFactory.Get(ProfileName);
            FleetManager = new GameLiftFleetManager(GameLiftWrapper);
            CGDManager = new GameLiftCGDManager(GameLiftWrapper);
            ComputeManager = new GameLiftComputeManager(GameLiftWrapper);
            OnUserProfileUpdated?.Invoke();
        }

        public void RefreshProfiles()
        {
            var profilesResponse = CoreApi.GetSetting(SettingsKeys.UserProfiles);
            if (!profilesResponse.Success || string.IsNullOrWhiteSpace(profilesResponse.Value))
            {
                _allProfiles = new List<UserProfile>();
            }
            else
            {
                try
                {
                    _allProfiles = _deserializer.Deserialize<List<UserProfile>>(profilesResponse.Value);
                }
                catch (Exception _)
                {
                    _allProfiles = new List<UserProfile>();
                }
            }
        }

        public PutSettingResponse SaveProfiles()
        {
            var profiles = _serializer.Serialize(_allProfiles);
            return CoreApi.PutSetting(SettingsKeys.UserProfiles, profiles);
        }

        public void SetBucketBootstrap(string bucketName)
        {
            BucketName = bucketName;
            OnUserProfileUpdated?.Invoke();
        }

        public UserProfile getProfileByName(string name)
        {
            UserProfile _fullProfile = _allProfiles.FirstOrDefault(profile => profile.Name == name);
            return _fullProfile;
        }

        public void AddAnotherProfile()
        {
            OnAddAnotherProfile?.Invoke();
        }

        public void SyncProfileStores()
        {
            foreach (string credentialProfile in AllProfiles)
            {
                if (_allProfiles.FirstOrDefault(profile => profile.Name == credentialProfile) == null)
                {
                    UserProfile newProfile = new UserProfile();
                    newProfile.Name = credentialProfile;
                    _allProfiles.Add(newProfile);
                }
            }
        }
    }
}