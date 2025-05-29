// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Provides an <see cref="ImageSequenceDrawer"/> to draw a spinner UI animation.
    /// </summary>
    internal static class SpinnerDrawerFactory
    {
        private static IReadOnlyList<Texture2D> s_sequence;

        public static ImageSequenceDrawer Create(float size)
        {
            if (s_sequence == null)
            {
                var imageLoader = new ImageLoader();
                s_sequence = imageLoader.LoadImageSequence(AssetNames.SpinnerIcon, first: 1, last: 4);
            }

            return new GuiLayoutImageSequenceDrawer(size, size, s_sequence, framesPerSecond: 10f, () => EditorApplication.timeSinceStartup);
        }
    }
}
