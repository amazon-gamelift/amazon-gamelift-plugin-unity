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

#if UNITY_SERVER || UNITY_EDITOR
using System.Collections.Generic;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model.Metrics;

namespace GameLiftServerSDK.Editor.UnitTests.Helpers
{
    internal class TestStatsDClient : IStatsDClient
    {
        public readonly List<(string metric,double value,string type,IList<Tag> tags)> GaugeCalls = new();
        public readonly List<(string metric,int value,string type,IList<Tag> tags)> CounterCalls = new();
        public readonly List<(string metric,double value,string type,IList<Tag> tags)> TimingCalls = new();
        public readonly List<Tag> GlobalTags = new();
        public void Increment(string metric, int value, IList<Tag> tags, SampleRate sampleRate) => CounterCalls.Add((metric,value,"inc",tags));
        public void Decrement(string metric, int value, IList<Tag> tags, SampleRate sampleRate) => CounterCalls.Add((metric,-value,"dec",tags));
        public void Gauge(string metric, double value, IList<Tag> tags, SampleRate sampleRate) => GaugeCalls.Add((metric,value,"g",tags));
        public void Timing(string metric, double value, IList<Tag> tags, SampleRate sampleRate) => TimingCalls.Add((metric,value,"ms",tags));
        public void AddGlobalTag(Tag tag) {
            if (tag == null) return;
            int idx = GlobalTags.FindIndex(t => t.Key == tag.Key);
            if (idx >= 0) {
                GlobalTags[idx] = tag; // replace existing key
            } else {
                GlobalTags.Add(tag);
            }
        }
        public void AddGlobalTag(string tag) { AddGlobalTag(new Tag(tag)); }
        public void RemoveGlobalTag(Tag tag) { if(tag!=null) GlobalTags.RemoveAll(t=>t.Key==tag.Key); }
        public void RemoveGlobalTag(string tag) { GlobalTags.RemoveAll(t=>t.Key==tag); }
        public void Dispose() {}
        public void Flush() {}
    }
}
#endif
