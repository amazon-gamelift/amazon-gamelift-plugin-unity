namespace AmazonGameLift.Editor
{
    public class UserProfile
    {
        
        public string Region { get; set; }

        public string Name { get; set; }

        public string BucketName { get; set; }

        #region Anywhere Fleet Settings

        public string AnywhereFleetName { get; set; }

        public string AnywhereFleetId { get; set; }

        public string AnywhereFleetLocation { get; set; }
        
        public string ComputeName { get; set; }

        public string IpAddress { get; set; }

        public string WebSocketUrl { get; set; }

        #endregion

        #region Container Settings
        public bool ContainerDeploymentInProgress { get; set; }

        public bool ContainersDeploymentComplete { get; set; }

        public ContainerScenarios ContainerQuestionnaireScenario { get; set; }

        public DeploymentScenarios ContainerDeploymentScenario { get; set; }

        public string ContainerGameServerBuildPath { get; set; }
        public string ContainerGameServerExecutable { get; set; }
        /**
         * Image id that already existed in docker or was built by this plugin
         */
        public string ContainerDockerImageId { get; set; }
        public string ContainerECRRepositoryName { get; set; }
        public string ContainerECRRepositoryUri { get; set; }
        public string ContainerECRImageUri { get; set; }
        /**
         * Image id that already existed in ECR or was pushed by this plugin
         */
        public string ContainerECRImageId { get; set; }
        public string ContainerPortRange { get; set; }
        public string ContainerTotalMemory { get; set; }
        public string ContainerTotalVcpu { get; set; }
        public string ContainerGameName { get; set; }
        /**
         * Image tag specified by customer for pushing to ECR
         */
        public string ContainerImageTag { get; set; }
        public bool IsContainerImageBuilding { get; set; }
        public bool IsContainerImageBuilt { get; set; }
        public bool IsECRRepoCreated { get; set; }
        public bool IsContainerPushedToECR { get; set; }
        public bool IsCGDDeploying { get; set; }
        public bool IsCGDDeployed { get; set; }

        #endregion

        #region Managed EC2 Settings

        public DeploymentScenarios DeploymentScenario { get; set; }
        
        public string DeploymentGameName { get; set; }
        
        public string ManagedEC2FleetName { get; set; }
        
        public string BuildName { get; set; }
        
        public string LaunchParameters { get; set; }
        
        public string BuildOperatingSystem { get; set; }
        
        public string DeploymentBuildFilePath { get; set; }
        
        public string DeploymentBuildFolderPath { get; set; }

        #endregion
    }
}