// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLift.Editor;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using UnityEngine;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class ImageLoaderTests
    {
        private readonly Texture2D _testTexture = new Texture2D(1, 1);

        [Test, Sequential]
        public void LoadImageSequence_WhenCorrectRange_LoadsAllImages(
            [Values(-2, -1, 0, 1, 2, 0)] int first, [Values(0, 2, 3, 3, 3, 0)] int last)
        {
            string assetNameBase = "TestBase";
            string assetNamePattern = "{0} {1}";
            var mock = new Mock<ImageLoader>();

            for (int i = first; i <= last; i++)
            {
                string assetName = string.Format(assetNamePattern, assetNameBase, i);
                string imagePath = ResourceUtility.GetImagePath(assetName);

                mock.Protected()
                    .Setup<Texture2D>("Load", imagePath)
                    .Returns(_testTexture)
                    .Verifiable();
            }

            ImageLoader imageLoader = mock.Object;
            IReadOnlyList<Texture2D> images = imageLoader.LoadImageSequence(assetNameBase, first, last);

            mock.Verify();
            Assert.IsNotNull(images);
            Assert.AreEqual(last - first + 1, images.Count);

            foreach (Texture2D item in images)
            {
                Assert.AreEqual(_testTexture, item);
            }
        }

        [Test, Sequential]
        public void LoadImageSequence_WhenrIncorrectRange_LoadsZeroImages(
            [Values(-2, -1, 0, 1, 2)] int first, [Values(-3, -2, -1, 0, 0)] int last)
        {
            string assetNameBase = "TestBase";
            var mock = new Mock<ImageLoader>();
            mock.Protected()
                .Setup<Texture2D>("Load", ItExpr.IsAny<string>())
                .Throws<NotImplementedException>()
                .Verifiable();

            ImageLoader imageLoader = mock.Object;
            IReadOnlyList<Texture2D> images = imageLoader.LoadImageSequence(assetNameBase, first, last);

            Assert.IsNotNull(images);
            Assert.AreEqual(0, images.Count);
        }

        [Test]
        public void LoadImage_WhenItExistsInThemedPath_ImageIsExpected()
        {
            string assetName = string.Empty;
            string imagePath = ResourceUtility.GetImagePath(assetName);
            string imagePathForTheme = ResourceUtility.GetImagePathForCurrentTheme(assetName);

            var mock = new Mock<ImageLoader>();
            mock.Protected()
                .Setup<Texture2D>("Load", imagePath)
                .Returns<Texture2D>(null)
                .Verifiable();

            mock.Protected()
                .Setup<Texture2D>("Load", imagePathForTheme)
                .Returns(_testTexture)
                .Verifiable();

            ImageLoader imageLoader = mock.Object;
            Texture2D image = imageLoader.LoadImage(assetName);

            mock.Verify();
            Assert.IsNotNull(image);
            Assert.AreEqual(_testTexture, image);
        }

        [Test]
        public void LoadImage_WhenItExistsInNonThemedPath_ImageIsExpected()
        {
            string assetName = string.Empty;
            string imagePath = ResourceUtility.GetImagePath(assetName);

            var mock = new Mock<ImageLoader>();
            mock.Protected()
                .Setup<Texture2D>("Load", imagePath)
                .Returns(_testTexture)
                .Verifiable();

            ImageLoader imageLoader = mock.Object;
            Texture2D image = imageLoader.LoadImage(assetName);

            mock.Verify();
            Assert.IsNotNull(image);
            Assert.AreEqual(_testTexture, image);
        }

        [Test]
        public void LoadImage_WhenPathIsNull_ThrowsException()
        {
            string assetName = null;
            var imageLoader = new ImageLoader();
            Assert.Throws<ArgumentNullException>(() => imageLoader.LoadImage(assetName));
        }
    }
}
