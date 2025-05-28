// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class TextProviderTests
    {
        [Test]
        public void Get_WhenNullKey_ThrowsException()
        {
            TextProvider underTest = TextProviderFactory.Create();

            Assert.Throws<ArgumentNullException>(() => underTest.Get(null));
        }

        [Test]
        public void Get_WhenUnknownKey_IsKey()
        {
            string key = new Guid().ToString();
            TextProvider underTest = TextProviderFactory.Create();

            string message = underTest.Get(key);

            Assert.AreEqual(key, message);
        }

        [Test]
        public void GetError_WhenNullKey_IsNotNull()
        {
            TextProvider underTest = TextProviderFactory.Create();

            string message = underTest.GetError(null);

            Assert.IsNotNull(message);
        }

        [Test]
        public void GetError_WhenUnknownCode_IsNotNull()
        {
            string errorCode = new Guid().ToString();
            TextProvider underTest = TextProviderFactory.Create();

            string message = underTest.GetError(errorCode);

            Assert.IsNotNull(message);
        }
    }
}
