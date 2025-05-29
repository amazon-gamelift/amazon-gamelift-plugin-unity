// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.GameLiftLocalTesting
{
    [TestFixture]
    public class GameLiftProcessTests
    {
        [Test]
        public void Start_WhenParametersIsNotValid_IsNotSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            StartResponse response = gameliftProcess.Start(new StartRequest
            {
                GameLiftLocalFilePath = null
            });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void Start_WhenProcessIsStarted_IsSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            string expectedArguments = "-jar \"\\NonEmptyPath\" -p \"100\"";

            processWrapperMock.Setup(x => x.Start(It.Is<ProcessStartInfo>(
                    p => p.Arguments == expectedArguments))
                )
                .Returns(1);

            StartResponse response = gameliftProcess.Start(
                new StartRequest
                {
                    Port = 100,
                    GameLiftLocalFilePath = @"\NonEmptyPath"
                });

            processWrapperMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.ProcessId);
        }

        [Test]
        public void Start_WhenProcessThrowsException_IsNotSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            processWrapperMock.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
                .Throws(new Exception());

            StartResponse response = gameliftProcess.Start(
                new StartRequest
                {
                    GameLiftLocalFilePath = "NonEmptyPath"
                });

            processWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
        }

        [Test]
        public void Stop_WhenProcessStoppped_IsSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            processWrapperMock.Setup(x => x.Kill(1));

            StopResponse response = gameliftProcess.Stop(
                new StopRequest
                {
                    ProcessId = 1
                });

            processWrapperMock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void Stop_WhenProcessThrows_IsNotSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            processWrapperMock.Setup(x => x.Kill(1))
                .Throws(new Exception());

            StopResponse response = gameliftProcess.Stop(
                new StopRequest
                {
                    ProcessId = 1
                });

            processWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);

        }

        [Test]
        public void RunLocalServer_WhenProcessStarts_IsSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            string filePath = "NonEmptyPath";
            bool showWindow = false;

            processWrapperMock.Setup(x => x.Start(
                    It.Is<ProcessStartInfo>(p => p.FileName == filePath
                    && p.UseShellExecute == showWindow)))
                .Returns(1);

            RunLocalServerResponse response = gameliftProcess.RunLocalServer(new RunLocalServerRequest
            {
                FilePath = filePath,
                ShowWindow = showWindow
            });

            processWrapperMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.ProcessId);
        }

        [Test]
        public void RunLocalServer_WhenParametersIsNotValid_IsNotSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            RunLocalServerResponse response = gameliftProcess.RunLocalServer(null);

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void RunLocalServer_WhenProcessThrowsException_IsNotSuccessful()
        {
            var processWrapperMock = new Mock<IProcessWrapper>();

            var gameliftProcess = new GameLiftProcess(
                    processWrapperMock.Object
                );

            processWrapperMock.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
                .Throws(new Exception());

            RunLocalServerResponse response = gameliftProcess.RunLocalServer(
                new RunLocalServerRequest
                {
                    FilePath = "NonEmptyPath"
                });

            processWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
        }
    }
}
