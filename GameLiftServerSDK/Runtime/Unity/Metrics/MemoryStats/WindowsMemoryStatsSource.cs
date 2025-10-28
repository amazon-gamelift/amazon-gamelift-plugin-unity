/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

using Aws.GameLift.Server;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aws.GameLift.Unity.Metrics
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    /// <summary>
    /// Matching WIN API MEMORYSTATUSEX struct.
    /// https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    /// <summary>
    /// Matching WIN API PROCESS_MEMORY_COUNTERS struct.
    /// https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_MEMORY_COUNTERS
    {
        public uint cb;
        public uint PageFaultCount;
        public nuint PeakWorkingSetSize;
        public nuint WorkingSetSize;
        public nuint QuotaPeakPagedPoolUsage;
        public nuint QuotaPagedPoolUsage;
        public nuint QuotaPeakNonPagedPoolUsage;
        public nuint QuotaNonPagedPoolUsage;
        public nuint PagefileUsage;
        public nuint PeakPagefileUsage;
    }

    /// <summary>
    /// Windows-specific memory info source using Win APIs.
    /// </summary>
    internal sealed class WindowsMemoryStatsSource : IMemoryStatsSource
    {
        private static readonly uint MemoryStatusLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        private static readonly uint ProcessMemoryCountersLength = (uint)Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>();

        private readonly GameLiftLogger _logger;

        public WindowsMemoryStatsSource()
        {
            _logger = GameLiftLogger.Instance;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatus);

        [DllImport("psapi.dll")]
        private static extern bool GetProcessMemoryInfo(nint hProcess, ref PROCESS_MEMORY_COUNTERS counter, uint size);


        public MemoryValues? ReadMemoryInfo()
        {
            MEMORYSTATUSEX GlobalMemory = new MEMORYSTATUSEX { dwLength = MemoryStatusLength };
            if (!GlobalMemoryStatusEx(ref GlobalMemory))
            {
                _logger?.LogError("Unable to read system memory stats. (GlobalMemoryStatusEx failed.)");
                return null;
            }

            PROCESS_MEMORY_COUNTERS ProcessMemory = new PROCESS_MEMORY_COUNTERS { cb = ProcessMemoryCountersLength };
            if (!GetProcessMemoryInfo(Process.GetCurrentProcess().Handle, ref ProcessMemory, ProcessMemoryCountersLength))
            {
                _logger?.LogError("Unable to read processs memory stats. (GetProcessMemoryInfo failed.)");
                return null;
            }

            return new MemoryValues
            {
                PhysicalTotal = (long)GlobalMemory.ullTotalPhys,
                PhysicalAvailable = (long)GlobalMemory.ullAvailPhys,
                PhysicalUsed = (long)ProcessMemory.WorkingSetSize,
                
                VirtualTotal = (long)GlobalMemory.ullTotalPageFile,
                VirtualAvailable = (long)GlobalMemory.ullAvailPageFile,
                VirtualUsed = (long)ProcessMemory.PagefileUsage,

                CommitLimit = (long)GlobalMemory.ullTotalPageFile,
                CommittedAS = (long)GlobalMemory.ullTotalPageFile - (long)GlobalMemory.ullAvailPageFile,
                CommitAvailable = (long)GlobalMemory.ullAvailPageFile
            };
        }
    }
#endif // UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
}
