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

#if UNITY_EDITOR || UNITY_SERVER
using log4net.Config;
using UnityEngine;

namespace Aws.GameLift.Unity
{
    public class DefaultLoggingConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Configure()
        {
            if (!log4net.LogManager.GetRepository().Configured)
            {
                // Configure log4net to support the default console output
                BasicConfigurator.Configure();
            }
        }
    }
}
#endif
