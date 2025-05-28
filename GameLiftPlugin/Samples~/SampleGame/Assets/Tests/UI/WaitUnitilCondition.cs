// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using UnityEngine;

namespace SampleTests.UI
{
    public sealed class WaitUnitilCondition : WaitWithTimeOut
    {
        private readonly Func<bool> _evaluateCondition;

        public override bool keepWaiting
        {
            get
            {
                bool currentValue = _evaluateCondition();
                return currentValue && base.keepWaiting;
            }
        }

        public WaitUnitilCondition(Func<bool> evaluateCondition, float timeoutSec = 10)
            : base(timeoutSec, () => Time.realtimeSinceStartup)
        {
            _evaluateCondition = evaluateCondition ?? throw new ArgumentNullException(nameof(evaluateCondition));
        }
    }
}
