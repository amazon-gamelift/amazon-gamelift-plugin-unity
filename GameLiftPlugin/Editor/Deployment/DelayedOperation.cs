// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AmazonGameLift.Editor
{
    internal class DelayedOperation
    {
        private readonly Action _action;
        private readonly Delay _delay;
        private readonly int _delayMs;
        private CancellationTokenSource _cancellation;

        public DelayedOperation(Action action, Delay delay, int delayMs)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _delay = delay ?? throw new ArgumentNullException(nameof(delay));
            _delayMs = delayMs;
        }

        public async Task Request()
        {
            Cancel();
            _cancellation = new CancellationTokenSource();

            try
            {
                await _delay.Wait(_delayMs, _cancellation.Token);
                _action();
            }
            catch (TaskCanceledException)
            {
            }
        }

        public void Cancel()
        {
            _cancellation?.Cancel();
        }
    }
}
