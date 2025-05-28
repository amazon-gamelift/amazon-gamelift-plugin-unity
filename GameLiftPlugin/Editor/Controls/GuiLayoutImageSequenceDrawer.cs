// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal sealed class GuiLayoutImageSequenceDrawer : ImageSequenceDrawer
    {
        private readonly float _height;
        private readonly float _width;

        public GuiLayoutImageSequenceDrawer(float height, float width,
            IReadOnlyList<Texture2D> sequence, float framesPerSecond, Func<double> getTime)
            : base(sequence, framesPerSecond, getTime)
        {
            _height = height;
            _width = width;
        }

        protected override void Draw(Texture2D texture)
        {
            GUILayout.Label(texture, GUILayout.Height(_height), GUILayout.Width(_width));
        }
    }
}
