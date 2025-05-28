// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal static class EditorWindowExtensions
    {
        public static void SetConstantSize(this EditorWindow window, Vector2 size)
        {
            if (!window)
            {
                return;
            }

            window.minSize = size;
            window.maxSize = size;
        }
    }
}
