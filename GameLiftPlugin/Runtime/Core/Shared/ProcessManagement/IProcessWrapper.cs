// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace AmazonGameLiftPlugin.Core.Shared.ProcessManagement
{
    public interface IProcessWrapper
    {
        string GetProcessOutput(ProcessStartInfo startInfo);

        (int, string) GetProcessIdAndStandardOutput(ProcessStartInfo startInfo);

        int Start(ProcessStartInfo processStartInfo);

        void Kill(int processId);
    }
}
