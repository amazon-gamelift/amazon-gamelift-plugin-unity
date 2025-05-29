// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public interface IResponsePoller
    {
        /// <exception cref="ArgumentNullException">For <paramref name="action"/>.</exception>
        Task<T> Poll<T>(int periodMs, Func<T> action, Predicate<T> stopCondition = null) where T : Response;
    }
}
