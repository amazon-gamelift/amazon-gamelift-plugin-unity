// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.JavaCheck;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;

namespace AmazonGameLiftPlugin.Core.Tests.InstalledJavaVersionCheck
{
    public class InstalledJavaVersionProviderFactory
    {
        public static IInstalledJavaVersionProvider Create(IProcessWrapper processWrapper)
        {
            return new InstalledJavaVersionProvider(processWrapper);
        }
    }
}
