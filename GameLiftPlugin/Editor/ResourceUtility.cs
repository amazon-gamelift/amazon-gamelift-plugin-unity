// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class ResourceUtility
    {
        private static IReadOnlyDictionary<MessageType, GUIStyle> s_cachedMessageStyles;
        private static GUIStyle s_cachedConfiguredStyle;
        private static GUIStyle s_cachedNotConfiguredStyle;
        private static GUIStyle s_cachedRadioButtonStyle;
        private static GUIStyle s_cachedHyperLinkStyle;
        private static GUIStyle s_cachedInfoLabelStyle;
        private static GUIStyle s_cachedTabActiveStyle;
        private static bool s_cachedProSkin;

        public static string GetImagePathForCurrentTheme(string assetName)
        {
            string skinPath = EditorGUIUtility.isProSkin ? "Dark" : "Light";
            return string.Format("Images/{0}/{1}", skinPath, assetName);
        }

        public static string GetImagePath(string assetName)
        {
            return string.Format("Images/{0}", assetName);
        }

        /// <summary>
        /// Call this only from OnGUI().
        /// </summary>
        public static IReadOnlyDictionary<MessageType, GUIStyle> GetMessageStyles()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedMessageStyles != null)
            {
                return s_cachedMessageStyles;
            }

            // EditorStyles are not ready when Unity starts.
            var styles = new Dictionary<MessageType, GUIStyle>
            {
                [MessageType.None] = new GUIStyle(EditorStyles.label)
            };

            var info = new GUIStyle
            {
                wordWrap = true
            };
            info.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(0x64, 0xD6, 0x97, 0xFF)
                : new Color32(0x00, 0x46, 0x37, 0xFF);
            styles[MessageType.Info] = info;

            var warning = new GUIStyle
            {
                wordWrap = true
            };
            warning.normal.textColor = new Color32(255, 255, 0, 255);
            styles[MessageType.Warning] = warning;

            var error = new GUIStyle
            {
                wordWrap = true
            };
            error.normal.textColor = new Color32(255, 0, 0, 255);
            styles[MessageType.Error] = error;
            s_cachedMessageStyles = styles;
            return styles;
        }

        public static GUIStyle GetInfoLabelStyle()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedInfoLabelStyle != null)
            {
                return s_cachedInfoLabelStyle;
            }

            var style = new GUIStyle();
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(0x9D, 0x9D, 0x9D, 0xFF)
                : new Color32(0x30, 0x30, 0x30, 0xFF);
            style.fontSize = 11;
            style.wordWrap = true;
            s_cachedInfoLabelStyle = style;
            return style;
        }

        /// <summary>
        /// Call this only from OnGUI().
        /// </summary>
        public static GUIStyle GetRadioButtonStyle()
        {
            if (s_cachedRadioButtonStyle != null)
            {
                return s_cachedRadioButtonStyle;
            }

            // EditorStyles are not ready when Unity starts.
            var style = new GUIStyle(EditorStyles.radioButton);
            style.normal.textColor = new Color32(0x83, 0x83, 0x83, 0xFF);
            s_cachedRadioButtonStyle = style;
            return style;
        }

        public static GUIStyle GetHyperLinkStyle()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedHyperLinkStyle != null)
            {
                return s_cachedHyperLinkStyle;
            }

            var style = new GUIStyle
            {
                wordWrap = false,
                stretchWidth = false
            };
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(0x4B, 0x7E, 0xFF, 0xFF)
                : new Color32(0x1B, 0x53, 0xE0, 0xFF);
            s_cachedHyperLinkStyle = style;
            return style;
        }

        public static GUIStyle GetConfiguredStyle()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedConfiguredStyle != null)
            {
                return s_cachedConfiguredStyle;
            }

            var configuredStyle = new GUIStyle(EditorStyles.largeLabel);
            configuredStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(0x64, 0xD6, 0x97, 0xFF)
                : new Color32(0x00, 0x46, 0x37, 0xFF);
            s_cachedConfiguredStyle = configuredStyle;
            return configuredStyle;
        }

        public static GUIStyle GetNotConfiguredStyle()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedNotConfiguredStyle != null)
            {
                return s_cachedNotConfiguredStyle;
            }

            var notConfiguredStyle = new GUIStyle(EditorStyles.largeLabel);
            notConfiguredStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(0xFF, 0x94, 0x00, 0xFF)
                : new Color32(0xB1, 0x0B, 0x0B, 0xFF);
            s_cachedNotConfiguredStyle = notConfiguredStyle;
            return notConfiguredStyle;
        }

        public static GUIStyle GetTextFilterNormalStyle()
        {
            ClearCacheIfSkinChanged();
            Color32 selectedColor = GetTextFilterSelectedBackColor();
            Texture2D selectedBackTexture = GetSmallTexture(selectedColor);
            Color32 normalColor = GetTextFilterBackColor();
            Texture2D normalBackTexture = GetSmallTexture(normalColor);

            var normalStyle = new GUIStyle(EditorStyles.label); // Cannot use during initialization
            normalStyle.normal.background = normalBackTexture;
            normalStyle.active.background = selectedBackTexture;
            return normalStyle;
        }

        public static GUIStyle GetTextFilterSelectedStyle()
        {
            ClearCacheIfSkinChanged();
            Color32 selectedColor = GetTextFilterSelectedBackColor();
            Texture2D selectedBackTexture = GetSmallTexture(selectedColor);

            var selectedStyle = new GUIStyle(EditorStyles.label); // Cannot use during initialization
            selectedStyle.normal.background = selectedBackTexture;
            selectedStyle.active.background = selectedBackTexture;
            selectedStyle.normal.textColor = Color.white;
            return selectedStyle;
        }

        public static GUIStyle GetTabActiveStyle()
        {
            ClearCacheIfSkinChanged();

            if (s_cachedTabActiveStyle != null)
            {
                return s_cachedTabActiveStyle;
            }

            var style = new GUIStyle(EditorStyles.toolbarButton);
            style.normal.textColor = new Color32(0xFF, 0xAA, 0x00, 0xFF); // Amazon Orange
            style.hover.textColor = new Color32(0xFF, 0xAA, 0x00, 0xFF); // Amazon Orange
            style.fontStyle = FontStyle.Bold;
            s_cachedTabActiveStyle = style;
            return style;
        }

        public static GUIStyle GetTabNormalStyle()
        {
            return EditorStyles.toolbarButton;
        }

        private static Texture2D GetSmallTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Color32 GetTextFilterSelectedBackColor()
        {
            return new Color32(0x33, 0x33, 0xCC, 0xFF);
        }

        private static Color32 GetTextFilterBackColor()
        {
            return EditorGUIUtility.isProSkin ? new Color32(0x51, 0x51, 0x51, 0xFF) : new Color32(0xDF, 0xDF, 0xDF, 0xFF);
        }

        private static void ClearCacheIfSkinChanged()
        {
            if (s_cachedProSkin == EditorGUIUtility.isProSkin)
            {
                return;
            }

            s_cachedProSkin = EditorGUIUtility.isProSkin;
            s_cachedConfiguredStyle = null;
            s_cachedHyperLinkStyle = null;
            s_cachedInfoLabelStyle = null;
            s_cachedMessageStyles = null;
            s_cachedNotConfiguredStyle = null;
            s_cachedRadioButtonStyle = null;
            s_cachedTabActiveStyle = null;
        }
    }
}
