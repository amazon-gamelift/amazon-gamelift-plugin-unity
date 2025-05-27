// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ManagedEC2IntegrateStep : ProgressBarStepComponent
    {

        private Button _selectDeploymentScenarioButton;

        public ManagedEC2IntegrateStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/ManagedEC2/ManagedEC2IntegrateStep")
        {
            new DeploymentStepTemplate.Builder(Strings.ManagedEC2IntegrateTitle, Strings.ManagedEC2IntegrateDescription)
                 .WithoutBaseButtons()
                 .Build(container);
            UpdateGUI();

            _selectDeploymentScenarioButton = container.Q<Button>("SelectDeploymentScenarioButton");

            _selectDeploymentScenarioButton.RegisterCallback<ClickEvent>(_ => SelectDeploymentScenarioClicked());
        }

        protected sealed override Task StartOrResumeStep()
        {
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() { }

        private void SelectDeploymentScenarioClicked()
        {
            _selectDeploymentScenarioButton.AddToClassList("hidden");
            TryStart();
            CompleteStep();
        }
    }
}
