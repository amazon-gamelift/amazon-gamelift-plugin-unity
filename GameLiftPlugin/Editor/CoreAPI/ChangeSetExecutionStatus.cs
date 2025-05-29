// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public static class ChangeSetExecutionStatus
    {
        public const string Unavailable = "UNAVAILABLE";
        public const string Available = "AVAILABLE";
        public const string ExecuteInProgress = "EXECUTE_IN_PROGRESS";
        public const string ExecuteComplete = "EXECUTE_COMPLETE";
        public const string ExecuteFailed = "EXECUTE_FAILED";
        public const string Obsolete = "OBSOLETE";
    }
}
