// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{

    /// <summary>
    /// Displays a message about a result of some operation like deployment, bootstrapping etc.
    /// </summary>
    internal sealed class StatusLabel
    {
        public void Draw(string message, MessageType messageType)
        {
            IReadOnlyDictionary<MessageType, GUIStyle> styles = ResourceUtility.GetMessageStyles();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(message, styles[messageType]);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
