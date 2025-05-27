// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class GameLiftPluginBucketPopup : EditorWindow
    {
        private static VisualTreeAsset m_VisualTreeAsset;
        private VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();
        public Action<string> OnConfirm;
        private const float PopupWidth = 850f;
        private const float PopupHeight = 320f;
        private StateManager _stateManager;

        
        public void OnEnable()
        {
            _root = rootVisualElement;
            _stateManager = new StateManager(new CoreApi());
            m_VisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Pages/GameLiftPluginBucketPopup");
            _root.Add(m_VisualTreeAsset.Instantiate());
            titleContent = new GUIContent(_textProvider.Get(Strings.UserProfilePageBootstrapPopupWindowTitle).Replace("{{PROFILE_NAME}}", _stateManager.SelectedProfile.Name));
            maxSize = new Vector2(PopupWidth, PopupHeight);
            minSize = maxSize;
        }
        
        public void Init(string bucketName)
        {
            _root.Q<Label>(Strings.UserProfilePageBootstrapPopupDescription).text =
                _textProvider.Get(Strings.UserProfilePageBootstrapPopupDescription);
            _root.Q<Label>("UserProfilePageBootstrapPopupBucketLabel").text =
                _textProvider.Get(Strings.UserProfilePageBootstrapPopupBucketText);

            var noticeStatusBox = _root.Q<StatusBox>(Strings.UserProfilePageBootstrapPopupNoticeStatusBox);
            noticeStatusBox.Show(StatusBox.StatusBoxType.Warning, Strings.UserProfilePageBootstrapPopupNoticeStatusBox);

            var bucketNameTextField = _root.Q<TextField>(Strings.UserProfilePageBootstrapPopupBucketText);
            bucketNameTextField.value = bucketName;

            var cancelButton = _root.Q<Button>(Strings.UserProfilePageBootstrapPopupCancelButton);
            cancelButton.text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupCancelButton);
            cancelButton.RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            var continueButton = _root.Q<Button>(Strings.UserProfilePageBootstrapPopupContinueButton);
            continueButton.text = _textProvider.Get(Strings.UserProfilePageBootstrapPopupContinueButton);
            continueButton.RegisterCallback<ClickEvent>(_ =>
            {
                OnConfirm?.Invoke(bucketNameTextField.value);
                Close();
            });
        }
    }
}