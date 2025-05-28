using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusIndicator : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StatusIndicator> { }

        private const long SPIN_DELAY_MILLIS = 10;
        private const string SpinClassName = "status-indicator--spin";
        private Label _label => this.Q<Label>();
        private VisualElement _imageContainer => this.Q<VisualElement>("image-container");
        private State _state;

        private readonly Dictionary<State, string> _stateClassNames = new()
        {
            { State.Inactive, "status-indicator--inactive" },
            { State.InProgress, "status-indicator--in-progress" },
            { State.Success, "status-indicator--success" },
            { State.Failed, "status-indicator--failed" },
        };

        public StatusIndicator()
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusIndicator");
            uxml.CloneTree(this);
            AddToClassList("status-indicator");
            AddToClassList("separator");
            AddToClassList("separator--horizontal");
            AddToClassList("separator--centered");
            _imageContainer.RegisterCallback<TransitionEndEvent>(evt =>
            {
                if (_state == State.InProgress)
                {
                    RemoveSpin();
                    AddSpin();
                }
            });
        }

        public void Set(State state, string text)
        {
            Reset();
            _state = state;
            _label.text = text;
            AddToClassList(_stateClassNames[state]);
            RemoveSpin();
            if (state == State.InProgress)
            {
                AddSpin();
            }
        }
        
        private void AddSpin()
        {
            schedule.Execute(() => { _imageContainer.AddToClassList(SpinClassName); }).StartingIn(SPIN_DELAY_MILLIS);
        }
        
        private void RemoveSpin()
        {
            _imageContainer.RemoveFromClassList(SpinClassName);
        }

        private void Reset()
        {
            foreach (var className in _stateClassNames.Values)
            {
                RemoveFromClassList(className);
            }
        }
    }

    public enum State
    {
        Inactive,
        InProgress,
        Success,
        Failed
    }
}