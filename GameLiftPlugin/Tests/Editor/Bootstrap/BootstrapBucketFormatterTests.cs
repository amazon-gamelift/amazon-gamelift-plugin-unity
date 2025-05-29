// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BootstrapBucketFormatterTests
    {
        private static readonly Tuple<string, string>[] s_bucketKeyTestCases = new Tuple<string, string>[]
        {
            new Tuple<string, string>("us-east1", "us-east1"),
            new Tuple<string, string>("GameName", "gamename"),
            new Tuple<string, string>("!0@#$%%G$amEN^^a&m**()_e+0~!@##$", "0gamename0"),
            new Tuple<string, string>("ыыgame-nameыы", "game-name"),
            new Tuple<string, string>("ZKIA47PNZKGFNJMPHEVZ", "zkia47pnzkgfnjmphevz")
        };

        private readonly BootstrapBucketFormatter _bucketFormatter = new BootstrapBucketFormatter();

        [Test]
        public void FormatBucketName_WhenNullAccountId_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.FormatBucketName(null, "Any"));
        }

        [Test]
        public void FormatBucketName_WhenNullRegion_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.FormatBucketName("Any", null));
        }

        [Test]
        public void FormatBucketKey_WhenNullValue_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.FormatBucketKey(null));
        }

        [Test]
        public void ValidateBucketKey_WhenNullValue_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.ValidateBucketKey(null));
        }

        [Test]
        [TestCaseSource(nameof(s_bucketKeyTestCases))]
        public void FormatBucketKey_WhenNonEmptyString_ReturnsValidKey(Tuple<string, string> testCase)
        {
            string expected = testCase.Item2;
            string input = testCase.Item1;

            string output = _bucketFormatter.FormatBucketKey(input);

            Assert.IsNotNull(output);
            Assert.AreEqual(expected, output);
        }

        [Test, Sequential]
        public void ValidateBucketKey_WhenValidKey_ReturnsTrue(
            [Values("g-n", "gamename", "0game-name0", "1example", "zkia47pnzkgfnjmphevz")] string input)
        {
            bool output = _bucketFormatter.ValidateBucketKey(input);

            Assert.IsNotNull(output);
            Assert.AreEqual(true, output);
        }

        [Test, Sequential]
        public void ValidateBucketKey_WhenInvalidKey_ReturnsFalse(
            [Values("", "-", "@", "Ab", "aB", "AB", "ё", "-gamename", "0gamename0.", "1example-", "zkia47pnzkGfnjmphevz", "Game Name")] string input)
        {
            bool output = _bucketFormatter.ValidateBucketKey(input);

            Assert.IsNotNull(output);
            Assert.AreEqual(false, output);
        }
    }
}
