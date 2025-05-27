// Copyright Amazon.com; Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Linq;

namespace AmazonGameLift.Editor
{
    public static class ContainersRegions
    {
        public static readonly string[] SupportedRegions =
        {
            "ap-northeast-1",
            "ap-northeast-2",
            "ap-southeast-2",
            "eu-central-1",
            "eu-west-1",
            "us-east-1",
            "us-west-2"
        };

        public static bool isContainersRegion(string region)
        {
            return SupportedRegions.Contains(region);
        }
    }
}