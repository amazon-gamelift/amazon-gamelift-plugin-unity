// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class VerticalProgressBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<VerticalProgressBar> { }

        private static readonly Dictionary<VerticalProgressState, string> StateToIndicatorClassName = new()
        {
            { VerticalProgressState.NotStarted, "progress-indicator--not-started" },
            { VerticalProgressState.InProgress, "progress-indicator--in-progress" },
            { VerticalProgressState.InProgressWarning, "progress-indicator--warning" },
            { VerticalProgressState.InProgressError, "progress-indicator--failure" },
            { VerticalProgressState.Completed, "progress-indicator--successful" },
            { VerticalProgressState.CompletedWarning, "progress-indicator--warning" },
        };

        private static readonly Dictionary<VerticalProgressState, string> StateToBarClassName = new()
        {
            { VerticalProgressState.NotStarted, "progress-bar--empty" },
            { VerticalProgressState.InProgress, "progress-bar--empty" },
            { VerticalProgressState.InProgressWarning, "progress-bar--empty" },
            { VerticalProgressState.InProgressError, "progress-bar--empty" },
            { VerticalProgressState.Completed, "progress-bar--full" },
            { VerticalProgressState.CompletedWarning, "progress-bar--full" },
        };

        private static readonly string HiddenClassName = "hidden";

        private VisualElement _indicator;
        private VisualElement _progressbar;
        private bool _hasNextStep;

        public VerticalProgressState State { get; private set; }

        public VerticalProgressBar()
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/VerticalProgressBar");
            uxml.CloneTree(this);
            _indicator = this.Q<VisualElement>("Indicator");
            _progressbar = this.Q<VisualElement>("ProgressBar");
        }

        public void Set(VerticalProgressState state, bool? hasNextStep = null)
        {
            Reset();
            State = state;
            if (hasNextStep.HasValue)
            {
                _hasNextStep = hasNextStep.Value;
            }

            _indicator.AddToClassList(StateToIndicatorClassName[state]);
            if (_hasNextStep)
            {
                _progressbar.AddToClassList(StateToBarClassName[state]);
            }
            else
            {
                _progressbar.AddToClassList(HiddenClassName);
            }
        }

        private void Reset()
        {
            foreach (var className in StateToIndicatorClassName.Values)
            {
                _indicator.RemoveFromClassList(className);
            }
            foreach (var className in StateToBarClassName.Values)
            {
                _progressbar.RemoveFromClassList(className);
            }
            _progressbar.RemoveFromClassList(HiddenClassName);
        }
    }

    public enum VerticalProgressState
    {
        NotStarted,
        InProgress,
        InProgressWarning,
        InProgressError,
        Completed,
        CompletedWarning,
    }
}