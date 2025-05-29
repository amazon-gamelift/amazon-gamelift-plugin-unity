// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    internal sealed class DialogAction
    {
        public string LabelKey { get; }

        public Action Action { get; }

        public DialogAction(string labelKey, Action action)
        {
            LabelKey = labelKey ?? throw new ArgumentNullException(nameof(labelKey));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }
    }
}
