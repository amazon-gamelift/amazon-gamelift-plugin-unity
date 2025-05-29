// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Draws an image. When running, switches the textures periodically to create animation.
    /// The client EditorWindow needs to have this code:
    /// private void OnInspectorUpdate()
    /// {
    ///     if (_imageSequenceDrawer.IsRunning)
    ///     {
    ///         Repaint();
    ///     }
    /// }
    /// </summary>
    internal abstract class ImageSequenceDrawer
    {
        private readonly IReadOnlyList<Texture2D> _sequence;
        private readonly Func<double> _getTime;
        private readonly double _delay;
        private int _currentIndex;
        private double _previousDrawTime;

        public bool IsRunning { get; private set; }

        public ImageSequenceDrawer(IReadOnlyList<Texture2D> sequence, float framesPerSecond, Func<double> getTime)
        {
            _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
            _getTime = getTime ?? throw new ArgumentNullException(nameof(getTime));
            _delay = framesPerSecond > 0 ? 1f / framesPerSecond : float.MaxValue;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Draw()
        {
            Draw(_sequence[_currentIndex]);

            if (!IsRunning)
            {
                return;
            }

            double time = _getTime();

            if (time - _previousDrawTime < _delay)
            {
                return;
            }

            _previousDrawTime = time;
            _currentIndex = (_currentIndex + 1) % _sequence.Count;
        }

        protected abstract void Draw(Texture2D texture);
    }
}
