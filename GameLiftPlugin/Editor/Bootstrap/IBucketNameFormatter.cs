// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal interface IBucketNameFormatter
    {
        /// <exception cref="ArgumentNullException">For all arguments.</exception>
        string FormatBucketName(string accountId, string region);

        /// <summary>
        /// Removes all symbols except lowercase latin letters, digits and middle dashes. Can result in an empty string.
        /// </summary>
        string FormatBucketKey(string value);

        /// <exception cref="ArgumentNullException"></exception>
        bool ValidateBucketKey(string value);
    }
}
