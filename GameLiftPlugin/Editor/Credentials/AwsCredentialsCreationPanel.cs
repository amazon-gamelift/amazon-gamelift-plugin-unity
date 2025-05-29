// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsCreationPanel
    {
        private readonly HyperLinkButton _hyperLinkButton;
        private readonly ControlDrawer _controlDrawer;
        private readonly StatusLabel _statusLabel;
        private readonly TextProvider _textProvider;
        private readonly AwsCredentialsCreation _model;
        private readonly PasswordDrawer _awsKeyPasswordDrawer;
        private readonly PasswordDrawer _awsSecretKeyPasswordDrawer;

        private readonly string _labelCreateProfileName;
        private readonly string _labelCurrentProfileName;
        private readonly string _tooltipCreateProfile;
        private readonly string _tooltipRegion;
        private readonly string _labelCreateButton;
        private readonly string _labelRegion;

        public AwsCredentialsCreationPanel(AwsCredentialsCreation model, StatusLabel statusLabel,
            TextProvider textProvider, ControlDrawer controlDrawer)
        {
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _controlDrawer = controlDrawer ?? throw new ArgumentNullException(nameof(controlDrawer));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _hyperLinkButton = new HyperLinkButton(_textProvider.Get(Strings.LabelCredentialsHelp),
              Urls.AwsHelpCredentials, ResourceUtility.GetHyperLinkStyle());
            _awsKeyPasswordDrawer = new PasswordDrawer(textProvider, controlDrawer, Strings.LabelCredentialsAccessKey, Strings.TooltipCredentialsAccessKey);
            _awsSecretKeyPasswordDrawer = new PasswordDrawer(textProvider, controlDrawer, Strings.LabelCredentialsSecretKey, Strings.TooltipCredentialsSecretKey);

            _labelCreateButton = _textProvider.Get(Strings.LabelCredentialsCreateButton);
            _labelCreateProfileName = _textProvider.Get(Strings.LabelCredentialsCreateProfileName);
            _labelRegion = _textProvider.Get(Strings.LabelCredentialsRegion);
            _labelCurrentProfileName = _textProvider.Get(Strings.LabelCredentialsCurrentProfileName);
            _tooltipCreateProfile = _textProvider.Get(Strings.TooltipCredentialsCreateProfile);
            _tooltipRegion = _textProvider.Get(Strings.TooltipCredentialsRegion);
        }

        public void Draw()
        {
            _model.ProfileName = _controlDrawer.DrawTextField(_labelCreateProfileName, _model.ProfileName, _tooltipCreateProfile);
            _model.AccessKeyId = _awsKeyPasswordDrawer.Draw(_model.AccessKeyId);
            _model.SecretKey = _awsSecretKeyPasswordDrawer.Draw(_model.SecretKey);
            _model.RegionBootstrap.RegionIndex = _controlDrawer.DrawPopup(
                _labelRegion, _model.RegionBootstrap.RegionIndex, _model.RegionBootstrap.AllRegions.ToArray(), _tooltipRegion);

            _controlDrawer.DrawSeparator();
            _controlDrawer.DrawReadOnlyText(_labelCurrentProfileName, _model.CurrentProfileName);
            DrawLink();
            GUILayout.Space(13f);

            using (new EditorGUI.DisabledGroupScope(!_model.CanCreate))
            {
                if (GUILayout.Button(_labelCreateButton))
                {
                    _model.Create();
                }
            }

            if (_model.Status.IsDisplayed)
            {
                _statusLabel.Draw(_model.Status.Message, _model.Status.Type);
            }
        }

        public void CleanUp()
        {
            _awsKeyPasswordDrawer.Hide();
            _awsSecretKeyPasswordDrawer.Hide();
        }

        private void DrawLink()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);
                _hyperLinkButton.Draw();
            }
        }
    }
}
