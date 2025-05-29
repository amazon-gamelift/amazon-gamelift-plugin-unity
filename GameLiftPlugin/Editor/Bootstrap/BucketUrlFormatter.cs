// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    internal sealed class BucketUrlFormatter
    {
        public const int MaxBucketNameLength = 63;

        /// <exception cref="ArgumentNullException">For all arguments.</exception>
        /// <exception cref="ArgumentException">For <paramref name="bucketName"/>, if it is longer
        /// than <see cref="MaxBucketNameLength"/>.</exception>
        public string Format(string bucketName, string region)
        {
            if (bucketName is null)
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (region is null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (bucketName.Length > MaxBucketNameLength)
            {
                throw new ArgumentException(DevStrings.BucketNameTooLong, nameof(region));
            }

            return string.Format(Urls.AwsS3BucketTemplate, bucketName, region);
        }
    }
}
