// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BucketUrlFormatterTests
    {
        private readonly BucketUrlFormatter _bucketFormatter = new BucketUrlFormatter();

        [Test]
        public void Format_WhenNullBucketName_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.Format(null, "any"));
        }

        [Test]
        public void Format_WhenNullRegion_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _bucketFormatter.Format("any", null));
        }

        [Test]
        public void Format_WhenLongBucketName_ThrowsException()
        {
            const string bucket = "any_long_bucket_name_is_invalid_if_it_is_longer_than__63_symbols";
            Assert.Throws<ArgumentException>(() => _bucketFormatter.Format(bucket, "any"));
        }

        [Test]
        public void Format_WhenValidArguments_ReturnsString()
        {
            string bucketName = "NonEmpty";
            string region = "NonEmpty";

            string output = _bucketFormatter.Format(bucketName, region);

            Assert.IsNotEmpty(output);
        }
    }
}
