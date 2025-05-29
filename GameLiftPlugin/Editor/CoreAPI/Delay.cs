// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;

namespace AmazonGameLift.Editor
{
    public class Delay
    {
        public virtual Task Wait(int delayMs, CancellationToken cancellationToken = default)
        {
            return Task.Delay(delayMs, cancellationToken);
        }
    }
}
