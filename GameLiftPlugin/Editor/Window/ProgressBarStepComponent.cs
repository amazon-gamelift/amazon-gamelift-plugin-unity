// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using static AmazonGameLift.Editor.StatusBox;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    public abstract class ProgressBarStepComponent : StatefulInput
    {
        private VisualElement _progressBarElement;
        private VerticalProgressBar _progressBar;
        private StatusBox _statusBox;

        protected readonly VisualElement _container;
        protected readonly StateManager _stateManager;
        protected ProgressBarStepComponent _nextStep = null;
        protected ProgressBarStepComponent _prevStep = null;

        protected bool HasNextStep => _nextStep != null;
        protected bool HasPrevStep => _prevStep != null;
        protected VerticalProgressState ProgressState => _progressBar.State;
        public FlowProgress Progress => ProgressFlowContainer.ConvertVerticalProgresToFlowProgress(_progressBar.State);

        /**
         * Starts or resumes running the step's processing to reach Completed status.
         * If the step was already in progress, it should resume processing.
         * If the step was already completed, it should call 'CompleteStep' as if it just completed.
         * If the step had hit an exception, it should call 'EncounteredException' as if it just encountered the exception.
         */
        protected abstract Task StartOrResumeStep();

        /**
         * Clear all state associated with the step such that on reload the step would be considered NotStarted.
         */
        protected abstract void ResetStep();

        protected override void UpdateGUI() { }

        public ProgressBarStepComponent(VisualElement container, StateManager stateManager, string uxmlPath)
        {
            _container = container;
            _stateManager = stateManager;

            InitializeUxml(uxmlPath);
        }

        public void SetNextStep(ProgressBarStepComponent nextStep)
        {
            _nextStep = nextStep;
            _progressBar.Set(_progressBar.State, HasNextStep);
        }

        public void SetPrevStep(ProgressBarStepComponent prevStep)
        {
            _prevStep = prevStep;
        }

        public void TryStart()
        {
            _statusBox.Close();
            if (Progress == FlowProgress.Completed)
            {
                throw new InvalidOperationException($"Unexpected progress state {ProgressState} for starting a step.");
            }

            _progressBar.Set(VerticalProgressState.InProgress);

            StartOrResumeStep();
        }

        /**
         * Recursively resets state of this step and it's following steps
         */
        public void Reset()
        {
            _statusBox.Close();

            ResetStep();

            if (HasNextStep)
            {
                _nextStep.Reset();
            }

            _progressBar.Set(VerticalProgressState.NotStarted);
        }

        /**
         * Calls Reset() and then TryStart()
         */
        public void ResetAndTryStart()
        {
            Reset();
            TryStart();
        }

        /**
         * Modifies a Completed step to InProgress and resets its next steps
         */
        protected void EditCompleted()
        {
            if (Progress != FlowProgress.Completed)
            {
                throw new InvalidOperationException($"Unexpected progress state {ProgressState} for editing completed step.");
            }

            _statusBox.Close();
            _nextStep?.Reset();

            _progressBar.Set(VerticalProgressState.InProgress);
        }

        /**
         * Updates the progress to completed and starts the next step.
         * Should only be called by the step when it has finished processing or on reloading state.
         * It's expected the state is either InProgress or NotStarted (when resuming).
         */
        protected void CompleteStep()
        {
            if (Progress == FlowProgress.Completed || ProgressState == VerticalProgressState.InProgressError)
            {
                throw new InvalidOperationException($"Unexpected progress state {ProgressState} for completing a step.");
            }

            _progressBar.Set(_progressBar.State == VerticalProgressState.InProgress
                ? VerticalProgressState.Completed
                : VerticalProgressState.CompletedWarning);

            if (HasNextStep)
            {
                _nextStep.TryStart();
            }
        }

        /**
         * Displays the encountered exception in the step's StatusBox and marks the step as in progress with a warning or error.
         * Choosing Warning and Error for the status box type determines which in progress state is used.
         * Can't be called for any status type other than warning or error.
         * It's expected the state is either InProgress or NotStarted (when resuming).
         */
        protected void EncounteredException(StatusBoxType statusBoxType, string text, string additionalText = null,
            string externalButtonLink = null, string externalButtonText = null,
            StatusBoxExternalTargetType externalTargetType = StatusBoxExternalTargetType.Link)
        {
            if (statusBoxType != StatusBoxType.Error && statusBoxType != StatusBoxType.Warning)
            {
                throw new InvalidOperationException($"{statusBoxType} is not a supported status box type to encounter as an exception.");
            }
            _statusBox.Show(statusBoxType, text, additionalText, externalButtonLink, externalButtonText, externalTargetType);

            // Throwing exception after displaying the status so that the status box can support investigating what went wrong.
            if (Progress == FlowProgress.Completed)
            {
                throw new InvalidOperationException($"Unexpected progress state {ProgressState} for encountering an exception.");
            }
            _progressBar.Set(statusBoxType == StatusBoxType.Error
                ? VerticalProgressState.InProgressError
                : VerticalProgressState.InProgressWarning);
        }

        private void InitializeUxml(string uxmlPath)
        {
            var mVisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>(uxmlPath);
            var uxml = mVisualTreeAsset.Instantiate();
            _container.Add(uxml);
            _progressBar = _container.Q<VerticalProgressBar>();
            _statusBox = _container.Q<StatusBox>();

            // Initialize state of UXML
            _progressBar?.Set(VerticalProgressState.NotStarted, false);
            _statusBox.Close();
        }
    }
}
