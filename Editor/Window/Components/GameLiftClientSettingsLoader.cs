// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using AmazonGameLift.Runtime;

namespace AmazonGameLift.Editor
{
    public class GameLiftClientSettingsLoader
    {
        private StatusBox _statusBox;
        private TextProvider _textProvider;

        public GameLiftClientSettingsLoader(StatusBox statusBox = null)
        {
            this._statusBox = statusBox;
            this._textProvider = TextProviderFactory.Create();
        }

        public GameLiftClientSettings LoadAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameLiftClientSettings");

            _statusBox?.Close();

            if (guids.Length <= 0 || string.IsNullOrWhiteSpace(guids[0]))
            {
                _statusBox?.Show(StatusBox.StatusBoxType.Error, _textProvider.GetError(ErrorCode.GameLiftClientSettingsNotFoundText));
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            if (guids.Length > 1)
            {
                _statusBox?.Show(StatusBox.StatusBoxType.Warning,
                    String.Format(_textProvider.GetError(ErrorCode.GameLiftClientSettingsMoreThanOneFoundTemplate), assetPath));
            }

            return AssetDatabase.LoadAssetAtPath<GameLiftClientSettings>(assetPath);
        }
    }
}