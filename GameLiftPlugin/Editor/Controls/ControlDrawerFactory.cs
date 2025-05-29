// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Provides a <see cref="ControlDrawer"/> object.
    /// </summary>
    internal static class ControlDrawerFactory
    {
        private static ControlDrawer s_controlDrawer;

        public static ControlDrawer Create()
        {
            if (s_controlDrawer == null)
            {
                s_controlDrawer = new ControlDrawer();
            }

            return s_controlDrawer;
        }
    }
}
