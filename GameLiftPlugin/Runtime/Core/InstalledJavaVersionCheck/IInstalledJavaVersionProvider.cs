// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.JavaCheck.Models;

namespace AmazonGameLiftPlugin.Core.JavaCheck
{
    public interface IInstalledJavaVersionProvider
    {
        /// <summary>
        /// Checks whether expected major version of java is installed or not.
        /// Expected Version number is standard java version number like
        /// described on the <see href="https://www.oracle.com/java/technologies/javase/versioning-naming.html">page</see>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        CheckInstalledJavaVersionResponse CheckInstalledJavaVersion(CheckInstalledJavaVersionRequest request);
    }
}
