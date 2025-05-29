// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class AwsCredentialsUpdatePanel
    {
        private readonly HyperLinkButton _hyperLinkButton;
        private readonly ControlDrawer _controlDrawer;
        private readonly StatusLabel _statusLabel;
        private readonly TextProvider _textProvider;
        private readonly AwsCredentialsUpdate _model;
        private readonly PasswordDrawer _awsKeyPasswordDrawer;
        private readonly PasswordDrawer _awsSecretKeyPasswordDrawer;

        private readonly string _labelRegion;
        private readonly string _tooltipSelectProfile;
        private readonly string _tooltipRegion;
        private readonly string _labelUpdateButton;
        private readonly string _labelSelectProfileName;
        private readonly string _labelCurrentProfileName;

        public AwsCredentialsUpdatePanel(AwsCredentialsUpdate model, StatusLabel statusLabel, TextProvider textProvider, ControlDrawer controlDrawer)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _hyperLinkButton = new HyperLinkButton(
                _textProvider.Get(Strings.LabelCredentialsHelp),
                Urls.AwsHelpCredentials, ResourceUtility.GetHyperLinkStyle());
            _controlDrawer = controlDrawer ?? throw new ArgumentNullException(nameof(controlDrawer));
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _awsKeyPasswordDrawer = new PasswordDrawer(textProvider, controlDrawer, Strings.LabelCredentialsAccessKey, Strings.TooltipCredentialsAccessKey);
            _awsSecretKeyPasswordDrawer = new PasswordDrawer(textProvider, controlDrawer, Strings.LabelCredentialsSecretKey, Strings.TooltipCredentialsSecretKey);

            _labelUpdateButton = _textProvider.Get(Strings.LabelCredentialsUpdateButton);
            _labelSelectProfileName = _textProvider.Get(Strings.LabelCredentialsSelectProfileName);
            _labelRegion = _textProvider.Get(Strings.LabelCredentialsRegion);
            _labelCurrentProfileName = _textProvider.Get(Strings.LabelCredentialsCurrentProfileName);
            _tooltipSelectProfile = _textProvider.Get(Strings.TooltipCredentialsSelectProfile);
            _tooltipRegion = _textProvider.Get(Strings.TooltipCredentialsRegion);
        }

        public void Draw()
        {
            _model.SelectedProfileIndex = _controlDrawer.DrawPopup(
                _labelSelectProfileName, _model.SelectedProfileIndex, _model.AllProlfileNames, _tooltipSelectProfile);

            _model.AccessKeyId = _awsKeyPasswordDrawer.Draw(_model.AccessKeyId);
            _model.SecretKey = _awsSecretKeyPasswordDrawer.Draw(_model.SecretKey);
            _model.RegionBootstrap.RegionIndex = _controlDrawer.DrawPopup(
              _labelRegion, _model.RegionBootstrap.RegionIndex, _model.RegionBootstrap.AllRegions.ToArray(), _tooltipRegion);

            _controlDrawer.DrawSeparator();
            _controlDrawer.DrawReadOnlyText(_labelCurrentProfileName, _model.CurrentProfileName);
            DrawLink();
            GUILayout.Space(13f);

            using (new EditorGUI.DisabledGroupScope(!_model.CanUpdate))
            {
                if (GUILayout.Button(_labelUpdateButton))
                {
                    _model.Update();
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
