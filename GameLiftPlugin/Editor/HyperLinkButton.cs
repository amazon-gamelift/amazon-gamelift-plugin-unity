// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    [Serializable]
    internal class HyperLinkButton
    {
        private readonly string _label;
        private readonly GUIStyle _linkStyle;

        public string Url { get; set; }

        public HyperLinkButton(string label, string url, GUIStyle linkStyle)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            _linkStyle = linkStyle ?? throw new ArgumentNullException(nameof(linkStyle));
        }

        public void Draw()
        {
            using (new GUILayout.VerticalScope())
            {
                if (GUILayout.Button(_label, _linkStyle))
                {
                    Application.OpenURL(Url);
                }

                Rect position = GUILayoutUtility.GetLastRect();

                Handles.BeginGUI();
                {
                    Color defaultColor = Handles.color;
                    Handles.color = _linkStyle.normal.textColor;
                    var p1 = new Vector3(position.xMin, position.yMax + 2f);
                    var p2 = new Vector3(position.xMax, position.yMax + 2f);
                    Handles.DrawLine(p1, p2);
                    Handles.color = defaultColor;
                }
                Handles.EndGUI();

                GUILayout.Space(2f);
            }
        }
    }
}
