// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal class DeploymentSettingsFactory
    {
        public static EC2DeploymentSettings Create(StateManager stateManager)
        {
            var parametersUpdater = new ScenarioParametersUpdater(CoreApi.SharedInstance, () => new ScenarioParametersEditor());
            TextProvider textProvider = TextProviderFactory.Create();
            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            return new EC2DeploymentSettings(ScenarioLocator.SharedInstance, PathConverter.SharedInstance,
                            CoreApi.SharedInstance, parametersUpdater, textProvider,
                            new DeploymentWaiter(), DeploymentIdContainerFactory.Create(), new Delay(), logger, stateManager);
        }
        public static ContainersDeploymentSettings CreateContainerDeploymentSettings(StateManager stateManager)
        {
            var parametersUpdater = new ScenarioParametersUpdater(CoreApi.SharedInstance, () => new ScenarioParametersEditor());
            TextProvider textProvider = TextProviderFactory.Create();
            UnityLogger logger = UnityLoggerFactory.Create(textProvider);
            return new ContainersDeploymentSettings(ScenarioLocator.SharedInstance, PathConverter.SharedInstance,
                            CoreApi.SharedInstance, parametersUpdater, textProvider,
                            new DeploymentWaiter(), DeploymentIdContainerFactory.Create(), new Delay(), logger, stateManager);
        }
    }
}
