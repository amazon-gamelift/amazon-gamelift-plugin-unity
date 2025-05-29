// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// A class to draw some common form field controls, with optional tooltips.
    /// </summary>
    internal class ControlDrawer
    {
        public void DrawSeparator()
        {
            GUILayout.Space(10f);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }

        public string DrawTextField(string label, string value, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.TextField(new GUIContent(label, tooltip), value);
            }

            return value;
        }

        public string DrawPasswordField(string label, string value, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.PasswordField(new GUIContent(label, tooltip), value);
            }

            return value;
        }

        public int DrawIntField(string label, int value, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.IntField(new GUIContent(label, tooltip), value);
            }

            return value;
        }

        public int DrawPopup(string label, int selectedIndex, string[] displayedOptions, string tooltip = null)
        {
            return EditorGUILayout.Popup(new GUIContent(label, tooltip), selectedIndex, displayedOptions);
        }

        public string DrawFilePathField(string label, string value, string extension, string fileDialogTitle, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.TextField(new GUIContent(label, tooltip), value);

                if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                {
                    value = EditorUtility.OpenFilePanel(fileDialogTitle, Application.dataPath, extension);
                    GUI.FocusControl(null);
                }
            }

            return value;
        }

        public string DrawFolderPathField(string label, string value, string defaultName, string folderDialogTitle, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.TextField(new GUIContent(label, tooltip), value);

                if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                {
                    value = EditorUtility.OpenFolderPanel(folderDialogTitle, Application.dataPath, defaultName);
                    GUI.FocusControl(null);
                }
            }

            return value;
        }

        public void DrawReadOnlyText(string label, string value, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(label, tooltip));
                EditorGUILayout.SelectableLabel(value, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        /// <summary>
        /// Returns total height.
        /// </summary>
        public float DrawReadOnlyTextWrapped(string label, string value, string tooltip = null)
        {
            var scope = new EditorGUILayout.HorizontalScope();

            using (scope)
            {
                EditorGUILayout.PrefixLabel(new GUIContent(label, tooltip));
                EditorGUILayout.SelectableLabel(value, EditorStyles.wordWrappedLabel);
            }

            return scope.rect.height;
        }
    }
}
