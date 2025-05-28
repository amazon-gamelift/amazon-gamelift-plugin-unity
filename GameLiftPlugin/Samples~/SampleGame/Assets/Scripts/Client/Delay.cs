// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Threading;
using System.Threading.Tasks;

public class Delay
{
    public virtual Task Wait(int delayMs, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delayMs, cancellationToken);
    }
}

#endif
