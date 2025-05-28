// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public enum FlowProgress
    {
        NotStarted,
        InProgress,
        Completed,
    }

    public abstract class ProgressFlowContainer
    {
        public static Action SetupSteps(IList<ProgressBarStepComponent> steps)
        {
            bool encounteredIncompleteStep = false;
            Action startIncompleteStepAction = null;
            for (int i = 0; i < steps.Count; i++)
            {
                var curStep = steps[i];
                if (i < steps.Count - 1)
                {
                    curStep.SetNextStep(steps[i + 1]);
                }
                if (i > 0)
                {
                    curStep.SetPrevStep(steps[i - 1]);
                }
                    
                if (!encounteredIncompleteStep && curStep.Progress != FlowProgress.Completed)
                {
                    encounteredIncompleteStep = true;
                    startIncompleteStepAction = () => curStep.TryStart(); // Make the first incomplete step the main start function.
                }
            }
            return startIncompleteStepAction;
        }

        public static FlowProgress ConvertVerticalProgresToFlowProgress(VerticalProgressState state)
        {
            switch (state)
            {
                case VerticalProgressState.NotStarted:
                    return FlowProgress.NotStarted;
                case VerticalProgressState.InProgress:
                    return FlowProgress.InProgress;
                case VerticalProgressState.InProgressWarning:
                    return FlowProgress.InProgress;
                case VerticalProgressState.InProgressError:
                    return FlowProgress.InProgress;
                case VerticalProgressState.Completed:
                    return FlowProgress.Completed;
                case VerticalProgressState.CompletedWarning:
                    return FlowProgress.Completed;
                default:
                    return FlowProgress.NotStarted;
            }
        }
    }
}
