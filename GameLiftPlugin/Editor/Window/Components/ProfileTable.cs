// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ProfileTable : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ProfileTable, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlBoolAttributeDescription m_Bool = new UxmlBoolAttributeDescription { name = "showRadioButtons", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as ProfileTable;

                ate.showRadioButtons = m_Bool.GetValueFromBag(bag, cc);

                ate.UpdateGUI();
            }
        }

        private bool showRadioButtons { get; set; }

        private readonly StateManager _stateManager;
        private readonly TextProvider _textProvider;

        private VisualElement _tableParent => this.Q<VisualElement>("table__parent");
        private VisualElement table__header => this.Q<VisualElement>("table__header");
        private Label _addAnotherProfile => this.Q<Label>("add__another__profile");
        public ProfileTable()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/ProfileTable");
            asset.CloneTree(this);

            _stateManager = EditorWindow.GetWindow<GameLiftPlugin>().StateManager;
            _stateManager.OnUserProfileUpdated += UpdateGUI;
            _textProvider = TextProviderFactory.Create();
        }

        public void UpdateGUI()
        {
            List<VisualElement> _allMembers = _tableParent.Children().ToList();
            foreach (VisualElement child in _allMembers)
            {
                if (child.name != "table__header")
                {
                    _tableParent.Remove(child);
                }
            }

            IReadOnlyList<string> _allProfiles = _stateManager.AllProfiles;

            foreach (string profile in _allProfiles)
            {
                UserProfile _fullProfile = _stateManager.getProfileByName(profile);

                if (showRadioButtons)
                {
                    VisualElement _tableMember = CreateTableMember(showRadioButtons, _fullProfile);
                    _tableParent.Add(_tableMember);
                }
                else
                {
                    if (ProfileIsSelected(_fullProfile))
                    {
                        VisualElement _tableMember = CreateTableMember(showRadioButtons, _fullProfile);
                        _tableParent.Add(_tableMember);
                    }
                }
            }

            if (showRadioButtons)
            {
                VisualElement _tableMember = new VisualElement();
                _tableMember.AddToClassList("table__labels__footer");
                VisualElement _nameContainer = new VisualElement();
                _nameContainer.AddToClassList("table__labels__container");
                Label _nameLabel = new Label("+ Add another profile");
                _nameLabel.AddToClassList("table__labels__footer__text");
                _nameLabel.name = "add__another__profile";
                _nameContainer.Add(_nameLabel);
                _tableMember.Add(_nameContainer);

                _tableParent.Add(_tableMember);
            }

            SetupButtonCallbacks();
        }

        private void SetupButtonCallbacks()
        {
            if (showRadioButtons)
            {
                _addAnotherProfile.RegisterCallback<ClickEvent>(_ => _stateManager.OnAddAnotherProfile());
            }
        }

        private bool ProfileIsSelected(UserProfile _fullProfile)
        {
            return _fullProfile.Name == _stateManager.ProfileName;
        }

        private VisualElement CreateTableMember(bool _withRadioButton, UserProfile _fullProfile)
        {
            VisualElement _tableMember = new VisualElement();
            _tableMember.AddToClassList("table__labels__member");

            VisualElement _buttonContainer = new VisualElement();
            _buttonContainer.AddToClassList("table__button__container");

            if (showRadioButtons)
            {
                RadioButton _selectButton = new RadioButton();
                if (ProfileIsSelected(_fullProfile))
                {
                    _selectButton.value = true;
                }
                _selectButton.name = _fullProfile.Name;
                _selectButton.RegisterCallback<ClickEvent>(_ => _stateManager.SelectedRadioButton = _selectButton.name);
                _buttonContainer.Add(_selectButton);
            }

            _tableMember.Add(_buttonContainer);

            VisualElement _nameContainer = new VisualElement();
            _nameContainer.AddToClassList("table__labels__container");

            Label _nameLabel;

            if (ProfileIsSelected(_fullProfile))
            {
                _nameLabel = new Label(_fullProfile.Name + " [ selected ]");
            }
            else
            {
                _nameLabel = new Label(_fullProfile.Name);
            }

            _nameLabel.AddToClassList("table__labels__text");
            _nameContainer.Add(_nameLabel);
            _tableMember.Add(_nameContainer);

            VisualElement _regionContainer = new VisualElement();
            _regionContainer.AddToClassList("table__labels__container");
            Label _regionLabel = new Label(_fullProfile.Region);
            _regionLabel.AddToClassList("table__labels__text");
            _regionContainer.Add(_regionLabel);
            _tableMember.Add(_regionContainer);

            VisualElement _bucketContainer = new VisualElement();
            _bucketContainer.AddToClassList("table__labels__container__long");
            Label _bucketLabel = new Label(_fullProfile.BucketName != null ? _fullProfile.BucketName : "-");
            _bucketLabel.AddToClassList("table__labels__text");
            _bucketContainer.Add(_bucketLabel);
            _tableMember.Add(_bucketContainer);

            VisualElement _statusContainer = new VisualElement();
            _statusContainer.AddToClassList("table__labels__container");
            StatusIndicator _statusIndicator = new StatusIndicator();
            _statusIndicator.AddToClassList("status-indicator--small");
            if (_stateManager.IsBootstrapped(_fullProfile))
            {
                _statusIndicator.Set(State.Success, _textProvider.Get(Strings.BootstrapStatusActive));
            }
            else
            {
                _statusIndicator.Set(State.Inactive, _textProvider.Get(Strings.BootstrapStatusInactive));
            }
            _statusContainer.Add(_statusIndicator);
            _tableMember.Add(_statusContainer);

            return _tableMember;
        }
    }
}