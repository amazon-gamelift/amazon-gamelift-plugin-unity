// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;

namespace AmazonGameLift.Editor
{
    internal class BootstrapBucketFormatter : IBucketNameFormatter
    {
        /// <inheritdoc/>
        public string FormatBucketName(string accountId, string region)
        {
            string key1 = FormatBucketKey(accountId);
            string key2 = FormatBucketKey(region);
            return $"gamelift-bootstrap-{key1}-{key2}";
        }

        /// <inheritdoc/>
        public string FormatBucketKey(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string lowercase = value.ToLowerInvariant();

            return Regex.Replace(lowercase, "[^a-z0-9-]", string.Empty);
        }

        /// <inheritdoc/>
        public bool ValidateBucketKey(string value)
        {
            return Regex.IsMatch(value, "^[a-z0-9][a-z0-9-]*[a-z0-9]$");
        }
    }
}
