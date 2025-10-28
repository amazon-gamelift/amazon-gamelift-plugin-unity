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

using System;
using System.Collections.Generic;
using System.Globalization;
using Aws.GameLift.Server;

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Abstraction over proc filesystem reads to enable testing without OS-specific files.
    /// </summary>
    public interface IProcReader
    {
        bool Exists(string path);
        string ReadAllText(string path);
        IEnumerable<string> ReadLines(string path);
    }

    /// <summary>
    /// Production implementation of IProcReader using System.IO.
    /// </summary>
    internal sealed class ProcReader : IProcReader
    {
        private readonly int bufferSize;

        internal ProcReader(int bufferSize = 512)
        {
            this.bufferSize = bufferSize > 0 ? bufferSize : 512;
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public IEnumerable<string> ReadLines(string path)
        {
            // Use streaming to match original logic
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, bufferSize))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fs, System.Text.Encoding.ASCII, false, bufferSize))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }

    /// <summary>
    /// Linux-specific memory info source using /proc/ files.
    /// </summary>
    public class LinuxMemoryStatsSource : IMemoryStatsSource
    {
        // Log-once guards to avoid spamming warnings each tick
        private static bool s_warnedMemInfoMissing;
        private static bool s_warnedMemInfoFieldsMissing;
        private static bool s_warnedMemInfoReadError;
        private static bool s_warnedMeminfoStreamError;
        private static bool s_loggedMemAvailableFallback;
        private static bool s_warnedStatmNotFound;
        private static bool s_warnedStatmEmpty;
        private static bool s_warnedStatmParseIncomplete;
        private static bool s_warnedStatmParseFailure;
        private static bool s_warnedStatmReadError;

        private const string ProcMemInfoPath = "/proc/meminfo";
        private const string ProcSelfStatmPath = "/proc/self/statm";

        // bytes per system page, used for calculating RSS
        private static readonly long SystemPageSizeBytes = Environment.SystemPageSize;

        private readonly IProcReader _procReader;
        private readonly GameLiftLogger _logger;

        public LinuxMemoryStatsSource(IProcReader procReader = null)
        {
            _procReader = procReader ?? new ProcReader();
            _logger = GameLiftLogger.Instance;
        }

        public MemoryValues? ReadMemoryInfo()
        {
            var memInfo = ReadProcMemoryInfo();
            var vmMetrics = ReadProcSelfStatm();

            long virtualTotal = 0;
            long commitAvailable = 0;

            try
            {
                if (memInfo.virtualMemory.memTotal > 0)
                {
                    virtualTotal = memInfo.virtualMemory.memTotal + memInfo.virtualMemory.swapTotal;
                }

                if (memInfo.virtualMemory.commitLimit > 0 && memInfo.virtualMemory.committedAS >= 0)
                {
                    commitAvailable = Math.Max(0, memInfo.virtualMemory.commitLimit - memInfo.virtualMemory.committedAS);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to calculate virtual/commit memory metrics: {ex.Message}");
            }

            return new MemoryValues
            {
                PhysicalTotal = memInfo.physicalMemory.total,
                PhysicalAvailable = memInfo.physicalMemory.available,
                PhysicalUsed = vmMetrics.vmRss,

                VirtualTotal = virtualTotal,
                VirtualAvailable = commitAvailable,
                VirtualUsed = vmMetrics.vmSize,

                CommitLimit = memInfo.virtualMemory.commitLimit,
                CommittedAS = memInfo.virtualMemory.committedAS,
                CommitAvailable = commitAvailable
            };
        }

        /// <summary>
        /// Read /proc/meminfo once and extract required fields.
        /// </summary>
        private ((long total, long available) physicalMemory, (long memTotal, long swapTotal, long committedAS, long commitLimit) virtualMemory) ReadProcMemoryInfo()
        {
            try
            {
                if (!_procReader.Exists(ProcMemInfoPath))
                {
                    if (!s_warnedMemInfoMissing)
                    {
                        _logger?.LogWarning($"Falling back to zero for memory metrics, {ProcMemInfoPath} not available");
                        s_warnedMemInfoMissing = true;
                    }
                    return ((0, 0), (0, 0, 0, 0));
                }

                var fields = new[] { "MemTotal:", "MemAvailable:", "MemFree:", "Cached:", "SwapTotal:", "Committed_AS:", "CommitLimit:" };
                var values = ReadProcMemoryLines(ProcMemInfoPath, fields);
                long memTotal = values.Length > 0 ? values[0] : 0;
                long memAvailable = values.Length > 1 ? values[1] : 0;
                long memFree = values.Length > 2 ? values[2] : 0;
                long cached = values.Length > 3 ? values[3] : 0;
                long swapTotal = values.Length > 4 ? values[4] : 0;
                long committedAS = values.Length > 5 ? values[5] : 0;
                long commitLimit = values.Length > 6 ? values[6] : 0;

                // Fallback: if MemAvailable is missing (older kernels), approximate as MemFree + Cached
                if (memAvailable == 0 && (memFree > 0 || cached > 0))
                {
                    memAvailable = memFree + cached;
                    if (!s_loggedMemAvailableFallback)
                    {
                        _logger?.LogDebug("/proc/meminfo MemAvailable missing; using MemFree+Cached as fallback.");
                        s_loggedMemAvailableFallback = true;
                    }
                }
                if (memTotal == 0 || commitLimit == 0)
                {
                    if (!s_warnedMemInfoFieldsMissing)
                    {
                        _logger?.LogWarning("One or more /proc/meminfo fields missing or zero (MemTotal or CommitLimit). Metrics may be incomplete.");
                        s_warnedMemInfoFieldsMissing = true;
                    }
                }
                return (
                    physicalMemory: (memTotal * 1024, memAvailable * 1024),
                    virtualMemory: (memTotal * 1024, swapTotal * 1024, committedAS * 1024, commitLimit * 1024)
                );
            }
            catch (Exception ex)
            {
                if (!s_warnedMemInfoReadError)
                {
                    _logger?.LogWarning($"Failed to read {ProcMemInfoPath}: {ex.Message}");
                    s_warnedMemInfoReadError = true;
                }
                return ((0, 0), (0, 0, 0, 0));
            }
        }

        /// <summary>
        /// Read /proc/self/statm to obtain process virtual size and RSS (bytes).
        /// </summary>
        private (long vmSize, long vmRss) ReadProcSelfStatm()
        {
            try
            {
                if (!_procReader.Exists(ProcSelfStatmPath))
                {
                    if (!s_warnedStatmNotFound)
                    {
                        _logger?.LogWarning("statm not found; virtual memory metrics will be zero");
                        s_warnedStatmNotFound = true;
                    }
                    return (0, 0);
                }
                var content = _procReader.ReadAllText(ProcSelfStatmPath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    if (!s_warnedStatmEmpty)
                    {
                        _logger?.LogWarning("statm empty; virtual memory metrics will be zero");
                        s_warnedStatmEmpty = true;
                    }
                    return (0, 0);
                }
                var parts = content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    if (!s_warnedStatmParseIncomplete)
                    {
                        _logger?.LogWarning("statm parse incomplete; virtual memory metrics will be zero");
                        s_warnedStatmParseIncomplete = true;
                    }
                    return (0, 0);
                }
                long sizePages, residentPages;
                if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out sizePages) ||
                    !long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out residentPages))
                {
                    if (!s_warnedStatmParseFailure)
                    {
                        _logger?.LogWarning("statm parse failure; virtual memory metrics will be zero");
                        s_warnedStatmParseFailure = true;
                    }
                    return (0, 0);
                }
                long pageSize = SystemPageSizeBytes;
                return (sizePages * pageSize, residentPages * pageSize);
            }
            catch (Exception ex)
            {
                if (!s_warnedStatmReadError)
                {
                    _logger?.LogWarning($"statm read error: {ex.Message}");
                    s_warnedStatmReadError = true;
                }
                return (0, 0);
            }
        }

        /// <summary>
        /// Stream /proc style file extracting specific key fields (values in KB).
        /// </summary>
        private long[] ReadProcMemoryLines(string filePath, string[] fields)
        {
            var values = new long[fields.Length];
            var foundFlags = new bool[fields.Length];
            int foundCount = 0;
            try
            {
                foreach (var line in _procReader.ReadLines(filePath))
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (!foundFlags[i] && line.StartsWith(fields[i], StringComparison.Ordinal))
                        {
                            values[i] = ParseMemInfoValueFast(line, fields[i].Length);
                            foundFlags[i] = true;
                            foundCount++;
                            break;
                        }
                    }
                    if (foundCount == fields.Length) break;
                }
            }
            catch (Exception ex)
            {
                if (!s_warnedMeminfoStreamError)
                {
                    _logger?.LogWarning($"Error streaming {filePath}: {ex.Message}");
                    s_warnedMeminfoStreamError = true;
                }
            }
            return values;
        }

        /// <summary>
        /// Fast parse memory info line to extract numeric value in kB.
        /// Assumes line starts with field name and ends with "kB".
        /// </summary>
        private long ParseMemInfoValueFast(string line, int prefixLength)
        {
            if (string.IsNullOrEmpty(line) || line.Length <= prefixLength) return 0;
            int endIndex = line.Length;
            // Strip trailing "kB" (case sensitive per /proc/meminfo) and whitespace
            if (endIndex >= 2 && line[endIndex - 2] == 'k' && line[endIndex - 1] == 'B')
            {
                endIndex -= 2;
                while (endIndex > prefixLength && char.IsWhiteSpace(line[endIndex - 1])) endIndex--;
            }
            int startIndex = endIndex;
            while (startIndex > prefixLength && char.IsDigit(line[startIndex - 1])) startIndex--;
            if (endIndex > startIndex && long.TryParse(line.Substring(startIndex, endIndex - startIndex), NumberStyles.None, CultureInfo.InvariantCulture, out long value))
            {
                return value;
            }
            _logger?.LogWarning($"Failed to parse memory value from line: {line}");
            return 0;
        }
    }
}
