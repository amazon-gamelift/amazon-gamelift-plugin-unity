// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal class ImageLoader
    {
        /// <summary>
        /// Loads all <see cref="Texture2D"/>s named "{<paramref name="assetNameBase"/>} {i}"
        /// where i is in [<paramref name="first"/>; <paramref name="last"/>].
        /// </summary>
        public IReadOnlyList<Texture2D> LoadImageSequence(string assetNameBase, int first, int last)
        {
            var sequence = new List<Texture2D>();

            for (int i = first; i <= last; i++)
            {
                string assetName = $"{assetNameBase} {i}";
                Texture2D texture = LoadImage(assetName);

                if (!texture)
                {
                    continue;
                }

                sequence.Add(texture);
            }

            return sequence;
        }

        public Texture2D LoadImage(string assetName)
        {
            if (assetName is null)
            {
                throw new ArgumentNullException(nameof(assetName));
            }

            Texture2D texture = Load(ResourceUtility.GetImagePath(assetName));

            if (texture)
            {
                return texture;
            }

            return Load(ResourceUtility.GetImagePathForCurrentTheme(assetName));
        }

        protected virtual Texture2D Load(string assetPath)
        {
            return Resources.Load<Texture2D>(assetPath);
        }
    }
}
