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

namespace Aws.GameLift.Unity.Metrics
{
    /// <summary>
    /// Memory information data structure with values ready for gauge publication.
    /// All values are in bytes.
    /// </summary>
    public struct MemoryValues
    {
        public long PhysicalTotal;
        public long PhysicalAvailable;
        public long PhysicalUsed;

        public long VirtualTotal;
        public long VirtualAvailable;
        public long VirtualUsed;

        public long CommitLimit;
        public long CommittedAS;
        public long CommitAvailable;
    }

    /// <summary>
    /// Platform-specific memory info source interface for DI.
    /// </summary>
    public interface IMemoryStatsSource
    {
        MemoryValues? ReadMemoryInfo();
    }
}
