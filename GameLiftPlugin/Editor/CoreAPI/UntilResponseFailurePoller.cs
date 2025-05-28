// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    internal sealed class UntilResponseFailurePoller : IResponsePoller
    {
        private readonly Delay _delay;

        public UntilResponseFailurePoller(Delay delay) => _delay = delay;

        public async Task<T> Poll<T>(int periodMs, Func<T> action, Predicate<T> stopCondition = null) where T : Response
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            T response;

            while (true)
            {
                response = action();

                if (!response.Success)
                {
                    break;
                }

                if (stopCondition != null && stopCondition(response))
                {
                    break;
                }

                await Task.Delay(periodMs);
            }

            return response;
        }
    }
}
