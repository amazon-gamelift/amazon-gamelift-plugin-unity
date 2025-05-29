// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Threading.Tasks;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public static class CoroutineTaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            while (!task.IsCompleted)
            {
                yield return null;
            }

            task.GetAwaiter().GetResult();
        }
    }
}

