// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Runtime;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    [CustomEditor(typeof(GameLiftClientSettings))]
    public sealed class GameLiftClientSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _remoteUrl;
        private SerializedProperty _region;
        private SerializedProperty _poolClientId;
        private SerializedProperty _isGameLiftAnywhere;

        private void OnEnable()
        {
            _remoteUrl = serializedObject.FindProperty(nameof(GameLiftClientSettings.ApiGatewayUrl));
            _region = serializedObject.FindProperty(nameof(GameLiftClientSettings.AwsRegion));
            _poolClientId = serializedObject.FindProperty(nameof(GameLiftClientSettings.UserPoolClientId));
            _isGameLiftAnywhere = serializedObject.FindProperty(nameof(GameLiftClientSettings.IsGameLiftAnywhere));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var targetSettings = (GameLiftClientSettings)target;
            
            EditorGUILayout.PropertyField(_isGameLiftAnywhere, new GUIContent("Use GameLift Anywhere", "Enable this to connect to the Anywhere fleet selected using the GameLift Plugin"));
            
            try
            {
                if (!targetSettings.IsGameLiftAnywhere)
                {
                    EditGameLiftMode(targetSettings);
                }
            }
            finally
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void EditGameLiftMode(GameLiftClientSettings targetSettings)
        {
            EditorGUILayout.PropertyField(_remoteUrl, new GUIContent("API Gateway Endpoint", "API Gateway URL"));
            EditorGUILayout.PropertyField(_region, new GUIContent("AWS Region", "AWS region used for communicating with Cognito and API Gateway"));
            EditorGUILayout.PropertyField(_poolClientId, new GUIContent("Cognito Client ID"));

            if (string.IsNullOrWhiteSpace(targetSettings.ApiGatewayUrl))
            {
                EditorGUILayout.HelpBox("Please set the API Gateway URL.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.AwsRegion))
            {
                EditorGUILayout.HelpBox("Please set the AWS Region.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(targetSettings.UserPoolClientId))
            {
                EditorGUILayout.HelpBox("Please set the User Pool Client ID.", MessageType.Warning);
            }
        }
    }
}
