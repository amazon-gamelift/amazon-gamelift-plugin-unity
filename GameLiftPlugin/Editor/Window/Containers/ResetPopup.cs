// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class ResetPopup : EditorWindow
    {
        private static VisualTreeAsset m_VisualTreeAsset;
        private VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();
        public Action OnConfirm;
        private const float PopupWidth = 850f;
        private const float PopupHeight = 320f;
        private StatusBox _statusBox;

        public void OnEnable()
        {
            _root = rootVisualElement;
            m_VisualTreeAsset = UnityEngine.Resources.Load<VisualTreeAsset>("EditorWindow/Components/Containers/ResetPopup");
            _root.Add(m_VisualTreeAsset.Instantiate());
            titleContent = new GUIContent("Reset Fleet Deployment");
            maxSize = new Vector2(PopupWidth, PopupHeight);
            minSize = maxSize;
        }

        public void Init(String region)
        {
            _statusBox = _root.Q<StatusBox>("ResetPopupStatusBox");
            _statusBox.Show(StatusBox.StatusBoxType.Warning,
                "You can delete the deployed resource stack from the AWS CloudFormation console. " +
                "Look for a stack name with the following pattern: GameLiftPluginForUnity-{GameName}-Containers.", 
                null, string.Format(Urls.AwsCloudFormationStacksTemplate, region), "AWS Console");
            _statusBox.HideCloseButton();

            var cancelButton = _root.Q<Button>("ResetPopupCancelButton");
            cancelButton.RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            var resetButton = _root.Q<Button>("ResetPopupResetButton");
            resetButton.RegisterCallback<ClickEvent>(_ =>
            {
                OnConfirm?.Invoke();
                Close();
            });
        }
    }
}