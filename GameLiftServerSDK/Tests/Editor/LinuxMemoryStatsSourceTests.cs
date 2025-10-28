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

#if (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && UNITY_SERVER
using NUnit.Framework;
using Aws.GameLift.Unity.Metrics;
using GameLiftServerSDK.Editor.UnitTests.Helpers;

namespace GameLiftServerSDK.Editor.UnitTests
{
    /// <summary>
    /// Tests for LinuxMemoryStatsSource class using mock ProcReader
    /// </summary>
    public class LinuxMemoryStatsSourceTests
    {
        private static readonly long SysPageSize = System.Environment.SystemPageSize;

        private static FakeProcReader BuildBasicProc(long memTotalKb = 1024 * 1024, long memAvailKb = 512 * 1024, long swapTotalKb = 0, long committedASKb = 128 * 1024, long commitLimitKb = 1024 * 1024)
        {
            var meminfo = $"MemTotal: {memTotalKb} kB\nMemAvailable: {memAvailKb} kB\nSwapTotal: {swapTotalKb} kB\nCommitted_AS: {committedASKb} kB\nCommitLimit: {commitLimitKb} kB\n";
            long vmSizePages = (memTotalKb * 1024) / SysPageSize;
            long vmRssPages = (memAvailKb * 1024) / SysPageSize;
            var statm = vmSizePages + " " + vmRssPages + " 0 0 0 0 0";
            return new FakeProcReader()
                .AddFile("/proc/meminfo", meminfo)
                .AddFile("/proc/self/statm", statm);
        }

        [Test]
        public void ReadMemoryInfo_ReturnsExpectedValues()
        {
            var proc = BuildBasicProc(memTotalKb: 2000000, memAvailKb: 1000000, swapTotalKb: 500000, committedASKb: 250000, commitLimitKb: 3000000);
            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            const double KB = 1024.0;

            // Physical memory
            Assert.AreEqual(2000000 * KB, memoryInfo.PhysicalTotal, 0.1, "Physical total mismatch");
            Assert.AreEqual(1000000 * KB, memoryInfo.PhysicalAvailable, 0.1, "Physical available mismatch");

            // Virtual memory - should be computed
            Assert.AreEqual((2000000 + 500000) * KB, memoryInfo.VirtualTotal, 0.1, "Virtual total should be memTotal + swapTotal");
            Assert.AreEqual((3000000 - 250000) * KB, memoryInfo.VirtualAvailable, 0.1, "Virtual available should be commitLimit - committedAS");

            // Commit metrics
            Assert.AreEqual(3000000 * KB, memoryInfo.CommitLimit, 0.1, "Commit limit mismatch");
            Assert.AreEqual(250000 * KB, memoryInfo.CommittedAS, 0.1, "Committed AS mismatch");
            Assert.AreEqual((3000000 - 250000) * KB, memoryInfo.CommitAvailable, 0.1, "Commit available should match virtual available");
        }

        [Test]
        public void ReadMemoryInfo_WithMissingMemAvailable_UsesFallback()
        {
            // Create proc reader without MemAvailable, only MemFree and Cached
            var meminfo = "MemTotal: 1000000 kB\nMemFree: 400000 kB\nCached: 100000 kB\nSwapTotal: 0 kB\nCommitted_AS: 128000 kB\nCommitLimit: 1000000 kB\n";
            long vmSizePages = (1000000 * 1024) / SysPageSize;
            long vmRssPages = (500000 * 1024) / SysPageSize;
            var statm = vmSizePages + " " + vmRssPages + " 0 0 0 0 0";

            var proc = new FakeProcReader()
                .AddFile("/proc/meminfo", meminfo)
                .AddFile("/proc/self/statm", statm);

            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            const double KB = 1024.0;

            // Should use MemFree + Cached as fallback for MemAvailable
            Assert.AreEqual((400000 + 100000) * KB, memoryInfo.PhysicalAvailable, 0.1, "Should use MemFree + Cached fallback");
        }

        [Test]
        public void ReadMemoryInfo_WithMissingProcFiles_ReturnsZeros()
        {
            var emptyProc = new FakeProcReader(); // No files added
            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(emptyProc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            Assert.AreEqual(0, memoryInfo.PhysicalTotal, "Should return 0 when proc files missing");
            Assert.AreEqual(0, memoryInfo.PhysicalAvailable, "Should return 0 when proc files missing");
            Assert.AreEqual(0, memoryInfo.VirtualTotal, "Should return 0 when proc files missing");
        }

        [Test]
        public void ReadMemoryInfo_WithNegativeCommitValues_ClampsToZero()
        {
            // Create scenario where committedAS > commitLimit
            var proc = BuildBasicProc(memTotalKb: 1000000, memAvailKb: 500000, committedASKb: 1200000, commitLimitKb: 1000000);
            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            Assert.AreEqual(0, memoryInfo.VirtualAvailable, "Should clamp negative commit available to 0");
            Assert.AreEqual(0, memoryInfo.CommitAvailable, "Should clamp negative commit available to 0");
        }

        [Test]
        public void ReadMemoryInfo_WithProcessMemoryMetrics_ReturnsCorrectRSSAndVmSize()
        {
            // Test that RSS and VmSize are calculated correctly from statm pages
            var proc = BuildBasicProc(memTotalKb: 1000000, memAvailKb: 500000);
            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            // Verify that process memory values are calculated from statm pages
            long expectedVmSizePages = (1000000 * 1024) / SysPageSize;
            long expectedVmRssPages = (500000 * 1024) / SysPageSize;
            long expectedVmSize = expectedVmSizePages * SysPageSize;
            long expectedVmRss = expectedVmRssPages * SysPageSize;

            Assert.AreEqual(expectedVmSize, memoryInfo.VirtualUsed, SysPageSize, "VirtualUsed should match calculated VmSize");
            Assert.AreEqual(expectedVmRss, memoryInfo.PhysicalUsed, SysPageSize, "PhysicalUsed should match calculated RSS");
        }

        [Test]
        public void ReadMemoryInfo_WithInvalidStatmFormat_ReturnsZeroForProcessMetrics()
        {
            // Create proc reader with valid meminfo but invalid statm
            var meminfo = "MemTotal: 1000000 kB\nMemAvailable: 500000 kB\nSwapTotal: 0 kB\nCommitted_AS: 128000 kB\nCommitLimit: 1000000 kB\n";
            var invalidStatm = "invalid data"; // Not in expected format

            var proc = new FakeProcReader()
                .AddFile("/proc/meminfo", meminfo)
                .AddFile("/proc/self/statm", invalidStatm);

            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            // System memory should still work
            Assert.AreEqual(1000000 * 1024, memoryInfo.PhysicalTotal, 0.1, "System memory should still be read");

            // Process memory should be zero due to invalid statm
            Assert.AreEqual(0, memoryInfo.VirtualUsed, "VirtualUsed should be 0 when statm is invalid");
            Assert.AreEqual(0, memoryInfo.PhysicalUsed, "PhysicalUsed should be 0 when statm is invalid");
        }

        [Test]
        public void ReadMemoryInfo_WithZeroCommitLimit_ReturnsZeroCommitAvailable()
        {
            // Test edge case where CommitLimit is 0
            var proc = BuildBasicProc(memTotalKb: 1000000, memAvailKb: 500000, committedASKb: 100000, commitLimitKb: 0);
            var linuxMemoryInfoSource = new LinuxMemoryStatsSource(proc);

            var maybeMemoryInfo = linuxMemoryInfoSource.ReadMemoryInfo();
            Assert.True(maybeMemoryInfo.HasValue);
            var memoryInfo = maybeMemoryInfo.Value;

            Assert.AreEqual(0, memoryInfo.CommitLimit, 0.1, "CommitLimit should be 0");
            Assert.AreEqual(0, memoryInfo.VirtualAvailable, 0.1, "VirtualAvailable should be 0 when CommitLimit is 0");
            Assert.AreEqual(0, memoryInfo.CommitAvailable, 0.1, "CommitAvailable should be 0 when CommitLimit is 0");
        }
    }
}
#endif
