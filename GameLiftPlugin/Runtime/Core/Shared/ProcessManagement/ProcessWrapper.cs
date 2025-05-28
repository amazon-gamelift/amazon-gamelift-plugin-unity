// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.ComponentModel;
using System.Diagnostics;

namespace AmazonGameLiftPlugin.Core.Shared.ProcessManagement
{
    public class ProcessWrapper : IProcessWrapper
    {
        public int Start(ProcessStartInfo processStartInfo)
        {
            var process = Process.Start(processStartInfo);

            return process.Id;
        }

        public (int, string) GetProcessIdAndStandardOutput(ProcessStartInfo startInfo)
        {
            var process = Process.Start(startInfo);

            return (process.Id, process.StandardOutput.ReadLine());
        }

        public string GetProcessOutput(ProcessStartInfo startInfo)
        {
            try
            {
                return Process.Start(startInfo)?.StandardError?.ReadToEnd();
            }
            catch (Win32Exception ex)
            {
                if (ex.Message.Equals("The system cannot find the file specified"))
                {
                    throw new ExecutableNotFoundException("Executable not found", ex);
                }
                else
                {
                    throw;
                }
            }
        }

        public void Kill(int processId)
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
        }
    }
}
