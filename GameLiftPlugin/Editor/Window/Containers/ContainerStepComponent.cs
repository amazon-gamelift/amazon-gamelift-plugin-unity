// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public abstract class ContainerStepComponent : ProgressBarStepComponent
    {
        public ContainerStepComponent(VisualElement container, StateManager stateManager, string uxmlPath) : base(container, stateManager, uxmlPath) { }

        // Utils for command preparation
        protected string GetPreparedCommand(string commandFormat)
        {
            string registryUri = _stateManager.ContainerECRRepositoryUri.Split("/")[0];
            string result = commandFormat;
            result = ReplaceIfNewValueExists(result, "{{REGION}}", _stateManager.Region);
            result = ReplaceIfNewValueExists(result, "{{PROFILE_NAME}}", _stateManager.ProfileName);
            result = ReplaceIfNewValueExists(result, "{{ECR_REGISTRY_URL}}", registryUri);
            result = ReplaceIfNewValueExists(result, "{{REPO_NAME}}", _stateManager.ContainerECRRepositoryName);
            result = ReplaceIfNewValueExists(result, "{{ECR_REPO_URI}}", _stateManager.ContainerECRRepositoryUri);
            result = ReplaceIfNewValueExists(result, "{{IMAGE_ID}}", _stateManager.ContainerDockerImageId);
            result = ReplaceIfNewValueExists(result, "{{IMAGE_TAG}}", _stateManager.ContainerImageTag);
            return result;
        }

        protected string ReplaceIfNewValueExists(string Text, string OldValue, string NewValue)
        {
            if (!String.IsNullOrEmpty(NewValue))
            {
                return Text.Replace(OldValue, NewValue);
            }
            else
            {
                return Text;
            }
        }

        protected string DashIfEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? value : "-";
        }
    }
}