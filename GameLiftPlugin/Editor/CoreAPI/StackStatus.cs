// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// All statuses at https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/using-cfn-describing-stacks.html
    /// </summary>
    public static class StackStatus
    {
        public const string CreateComplete = "CREATE_COMPLETE";
        public const string CreateFailed = "CREATE_FAILED";
        public const string CreateInProgress = "CREATE_IN_PROGRESS";
        public const string DeleteComplete = "DELETE_COMPLETE";
        public const string DeleteFailed = "DELETE_FAILED";
        public const string DeleteInProgress = "DELETE_IN_PROGRESS";
        public const string ReviewInProgress = "REVIEW_IN_PROGRESS";
        public const string RollbackComplete = "ROLLBACK_COMPLETE";
        public const string RollbackFailed = "ROLLBACK_FAILED";
        public const string RollbackInProgress = "ROLLBACK_IN_PROGRESS";
        public const string UpdateComplete = "UPDATE_COMPLETE";
        public const string UpdateCompleteCleanUpInProgress = "UPDATE_COMPLETE_CLEANUP_IN_PROGRESS";
        public const string UpdateFailed = "UPDATE_FAILED";
        public const string UpdateInProgress = "UPDATE_IN_PROGRESS";
        public const string UpdateRollbackInProgress = "UPDATE_ROLLBACK_IN_PROGRESS";
        public const string UpdateRollbackComplete = "UPDATE_ROLLBACK_COMPLETE";

        public static bool IsStackStatusOperationDone(this string stackStatus)
        {
            if (stackStatus is null)
            {
                throw new ArgumentNullException(nameof(stackStatus));
            }

            return stackStatus.Contains("_COMPLETE") || stackStatus.Contains("_FAILED");
        }

        public static bool IsStackStatusFailed(this string stackStatus)
        {
            if (stackStatus is null)
            {
                throw new ArgumentNullException(nameof(stackStatus));
            }

            return stackStatus.Contains("_FAILED");
        }

        public static bool IsStackStatusInProgress(this string stackStatus)
        {
            if (stackStatus is null)
            {
                throw new ArgumentNullException(nameof(stackStatus));
            }

            return stackStatus.Contains("_IN_PROGRESS");
        }

        public static bool IsStackStatusRollback(this string stackStatus)
        {
            if (stackStatus is null)
            {
                throw new ArgumentNullException(nameof(stackStatus));
            }

            return stackStatus.Contains("ROLLBACK");
        }

        public static bool IsStackStatusModifiable(this string stackStatus)
        {
            if (stackStatus is null)
            {
                throw new ArgumentNullException(nameof(stackStatus));
            }

            return stackStatus.Contains("_COMPLETE")
                && stackStatus != DeleteComplete
                && stackStatus != RollbackComplete;
        }
    }
}
