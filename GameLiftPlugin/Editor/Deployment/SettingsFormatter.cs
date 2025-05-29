// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;

namespace AmazonGameLift.Editor
{
    internal static class SettingsFormatter
    {
        public static int? ParseInt(string value)
        {
            bool success = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result);
            return success ? result : default(int?);
        }

        public static string FormatInt(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
