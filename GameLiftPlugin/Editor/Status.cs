// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;

namespace AmazonGameLift.Editor
{
    [Serializable]
    internal sealed class Status : IReadStatus
    {
        public string Message { get; private set; }

        public MessageType Type { get; private set; }

        public bool IsDisplayed { get; set; }

        public event Action Changed = default;

        public void SetMessage(string value, MessageType type)
        {
            if (Message != value || Type != type)
            {
                Message = value;
                Type = type;
                Changed?.Invoke();
            }
        }
    }
}
