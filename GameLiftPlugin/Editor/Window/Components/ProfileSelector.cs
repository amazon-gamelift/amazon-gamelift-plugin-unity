// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ProfileSelector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProfileSelector> 
        {
        }

        private readonly StateManager _stateManager;
        private readonly TextProvider _textProvider;

        private DropdownField _dropdown => this.Q<DropdownField>("Dropdown");
        private Label _bucketName => this.Q<Label>("BucketName");
        private Label _region => this.Q<Label>("Region");
        private StatusIndicator _statusIndicator => this.Q<StatusIndicator>();

        public ProfileSelector()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/ProfileSelector");
            asset.CloneTree(this);

            LocalizeText();

            _stateManager = EditorWindow.GetWindow<GameLiftPlugin>().StateManager;
            _stateManager.OnUserProfileUpdated += UpdateGUI;
            _textProvider = TextProviderFactory.Create();

            _dropdown.RegisterValueChangedCallback(value => { _stateManager.SetProfile(value.newValue); });
            UpdateGUI();
        }
 
        private void UpdateGUI()
        {
            _dropdown.choices = _stateManager.AllProfiles.ToList();
            _dropdown.SetValueWithoutNotify(_stateManager.ProfileName);
            _region.text = _stateManager.Region;
            if (_stateManager.IsBootstrapped())
            {
                _bucketName.text = _stateManager.BucketName;
                _statusIndicator.Set(State.Success, _textProvider.Get(Strings.BootstrapStatusActive));
            }
            else
            {
                _bucketName.text = _textProvider.Get(Strings.BootstrapStatusNoBucketCreated);
                _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.BootstrapStatusInactive));
            }
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(this);
            l.SetElementText("DropdownLabel", Strings.ProfileSelectorDropdownLabel);
            l.SetElementText("BucketNameLabel", Strings.ProfileSelectorBucketNameLabel);
            l.SetElementText("RegionLabel", Strings.ProfileSelectorRegionLabel);
            l.SetElementText("BootstrapStatusLabel", Strings.ProfileSelectorStatusLabel);
        }
    }
}