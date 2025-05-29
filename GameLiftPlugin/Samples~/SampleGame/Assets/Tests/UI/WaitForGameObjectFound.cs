// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;

namespace SampleTests.UI
{
    public sealed class WaitForGameObjectFound : WaitWithTimeOut
    {
        private readonly string _objectName;

        public override bool keepWaiting
        {
            get
            {
                var gameObject = GameObject.Find(_objectName);
                return gameObject == null && base.keepWaiting;
            }
        }

        public WaitForGameObjectFound(string objectName, float timeoutSec = 5)
            : base(timeoutSec, () => Time.realtimeSinceStartup)
        {
            _objectName = objectName;
        }
    }
}
