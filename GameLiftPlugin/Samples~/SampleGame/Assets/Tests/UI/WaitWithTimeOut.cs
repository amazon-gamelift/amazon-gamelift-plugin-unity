// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using UnityEngine;

namespace SampleTests.UI
{
    public class WaitWithTimeOut : CustomYieldInstruction
    {
        private readonly float _timeoutSec;
        private readonly Func<float> _getCurrentTime;
        private readonly float _startTime;

        public bool TimedOut { get; private set; }

        public override bool keepWaiting
        {
            get
            {
                if (_getCurrentTime() - _startTime >= _timeoutSec)
                {
                    TimedOut = true;
                }

                return !TimedOut;
            }
        }

        public WaitWithTimeOut(float timeoutSec, Func<float> getCurrentTime)
        {
            _timeoutSec = timeoutSec;
            _getCurrentTime = getCurrentTime ?? throw new ArgumentNullException(nameof(getCurrentTime));
            _startTime = getCurrentTime();
        }
    }
}
