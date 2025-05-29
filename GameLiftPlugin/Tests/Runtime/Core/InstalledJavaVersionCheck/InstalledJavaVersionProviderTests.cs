// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using AmazonGameLiftPlugin.Core.JavaCheck;
using AmazonGameLiftPlugin.Core.JavaCheck.Models;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.InstalledJavaVersionCheck
{
    [TestFixture]
    public class InstalledJavaVersionProviderTests
    {
        private CheckInstalledJavaVersionResponse GetCheckInstalledJavaVersionResponse(string output, int minVersion) {
            var processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(x => x.GetProcessOutput(
                It.IsAny<ProcessStartInfo>())
            ).Returns(output);

            IInstalledJavaVersionProvider installedJavaVersionProvider =
                InstalledJavaVersionProviderFactory.Create(processWrapperMock.Object);

            CheckInstalledJavaVersionResponse response =
                installedJavaVersionProvider.CheckInstalledJavaVersion(new CheckInstalledJavaVersionRequest
                {
                    ExpectedMinimumJavaMajorVersion = minVersion
                });

            processWrapperMock.Verify();
            return response;
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionIsInstalled()
        {
            var output = "java version \"1.8.0_291\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionIsMultiline()
        {
            var output = @"
                Picked up JAVA_TOOL_OPTIONS: -Dlog4j2.formatMsgNoLookups=true
                openjdk version ""1.8.0_322""
                OpenJDK Runtime Environment Corretto-8.322.06.1 (build 1.8.0_322-b06)
                OpenJDK 64-Bit Server VM Corretto-8.322.06.1 (build 25.322-b06, mixed mode)
            ";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionUsesAlternateFormat()
        {
            var output = "java version \"9.0.1\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionUsesShortFormat()
        {
            var output = "openjdk version \"19\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenExpectedJavaVersionUsesShortFormatAndIsV1()
        {
            var output = "openjdk version \"1\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsFalse(response.IsInstalled);

            var response2 = GetCheckInstalledJavaVersionResponse(output, 1);
            Assert.IsTrue(response2.Success, "Request was not successful");
            Assert.IsTrue(response2.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WithSecurityPatch()
        {
            var output = "java version \"1.09.1.1\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsTrue(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenJavaVersionIsTooLow()
        {
            var output = "java version \"1.8.0_291\"";
            var response = GetCheckInstalledJavaVersionResponse(output, 11);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsFalse(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenJavaIsNotInstalled()
        {
            var output = "java is not installed";
            var response = GetCheckInstalledJavaVersionResponse(output, 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsFalse(response.IsInstalled);
        }

        [Test]
        public void CheckInstalledJavaVersion_WhenJavaIsEmpty()
        {
            var response = GetCheckInstalledJavaVersionResponse("", 8);
            Assert.IsTrue(response.Success, "Request was not successful");
            Assert.IsFalse(response.IsInstalled);
        }
    }
}
