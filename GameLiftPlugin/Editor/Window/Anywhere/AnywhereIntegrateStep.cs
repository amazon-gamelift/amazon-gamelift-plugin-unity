// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class AnywhereIntegrateStep : ProgressBarStepComponent
    {

        public AnywhereIntegrateStep(VisualElement container, StateManager stateManager) : base(container, stateManager, "EditorWindow/Components/Anywhere/AnywhereIntegrateStep")
        {
            new DeploymentStepTemplate.Builder(Strings.AnywherePageIntegrateTitle, Strings.AnywherePageIntegrateDescription)
                 .WithHelpLinks(
                     new DeploymentStepTemplateLink(Urls.AnywherePageIntegrateServerLink, Strings.AnywherePageIntegrateServerLink),
                     new DeploymentStepTemplateLink(Urls.AnywherePageIntegrateClientLink, Strings.AnywherePageIntegrateClientLink))
                 .WithoutBaseButtons()
                 .WithNoContent()
                 .Build(container);
            UpdateGUI();
        }

        protected sealed override Task StartOrResumeStep()
        {
            CompleteStep();
            return Task.CompletedTask;
        }

        protected sealed override void ResetStep() { }
    }
}
