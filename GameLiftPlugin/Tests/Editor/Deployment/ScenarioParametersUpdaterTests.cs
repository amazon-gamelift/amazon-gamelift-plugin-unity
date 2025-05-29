// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class ScenarioParametersUpdaterTests
    {
        [Test]
        public void Update_WhenFileReadAllTextError_FailsWithError()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testErrorCode = "testErrorCode";
            var coreApiMock = new Mock<CoreApi>();

            FileReadAllTextResponse fileReadResponse = Response.Fail(new FileReadAllTextResponse() { ErrorCode = testErrorCode });
            coreApiMock.Setup(target => target.FileReadAllText(testFilePath))
                .Returns(fileReadResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(testErrorCode, response.ErrorCode);
        }

        [Test]
        public void Update_WhenReadParametersError_FailsWithError()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testParametersInput = "test input";
            const string testErrorCode = "testErrorCode";
            var coreApiMock = new Mock<CoreApi>();

            SetUpCoreApiForFileReadAllTextSuccess(coreApiMock, testFilePath, testParametersInput);

            var editorMock = new Mock<ScenarioParametersEditor>();

            var readResponse = Response.Fail(new Response() { ErrorCode = testErrorCode });
            editorMock.Setup(target => target.ReadParameters(testParametersInput))
                .Returns(readResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock, editorMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            editorMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(testErrorCode, response.ErrorCode);
        }

        [Test]
        public void Update_WhenSetParameterError_FailsWithError()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testParametersInput = "test input";
            const string testErrorCode = "testErrorCode";
            var coreApiMock = new Mock<CoreApi>();

            SetUpCoreApiForFileReadAllTextSuccess(coreApiMock, testFilePath, testParametersInput);

            var editorMock = new Mock<ScenarioParametersEditor>();

            var readResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.ReadParameters(testParametersInput))
                .Returns(readResponse)
                .Verifiable();

            var setResponse = Response.Fail(new Response() { ErrorCode = testErrorCode });
            editorMock.Setup(target => target.SetParameter(ScenarioParameterKeys.GameName, testGameName))
                .Returns(setResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock, editorMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            editorMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(testErrorCode, response.ErrorCode);
        }

        [Test]
        public void Update_WhenSaveParametersError_FailsWithError()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testParametersInput = "test input";
            const string testErrorCode = "testErrorCode";
            var coreApiMock = new Mock<CoreApi>();

            SetUpCoreApiForFileReadAllTextSuccess(coreApiMock, testFilePath, testParametersInput);

            var editorMock = new Mock<ScenarioParametersEditor>();

            var readResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.ReadParameters(testParametersInput))
                .Returns(readResponse)
                .Verifiable();

            var setResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.SetParameter(ScenarioParameterKeys.GameName, testGameName))
                .Returns(setResponse)
                .Verifiable();

            SaveParametersResponse saveResponse = Response.Fail(new SaveParametersResponse() { ErrorCode = testErrorCode });
            editorMock.Setup(target => target.SaveParameters())
                .Returns(saveResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock, editorMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            editorMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(testErrorCode, response.ErrorCode);
        }

        [Test]
        public void Update_WhenFileWriteAllTextError_FailsWithError()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testParametersInput = "test input";
            const string testParametersOutput = "test output";
            const string testErrorCode = "testErrorCode";

            var coreApiMock = new Mock<CoreApi>();

            SetUpCoreApiForFileReadAllTextSuccess(coreApiMock, testFilePath, testParametersInput);

            var writeResponse = Response.Fail(new Response() { ErrorCode = testErrorCode });
            coreApiMock.Setup(target => target.FileWriteAllText(testFilePath, testParametersOutput))
                .Returns(writeResponse)
                .Verifiable();

            var editorMock = new Mock<ScenarioParametersEditor>();

            var readResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.ReadParameters(testParametersInput))
                .Returns(readResponse)
                .Verifiable();

            var setResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.SetParameter(ScenarioParameterKeys.GameName, testGameName))
                .Returns(setResponse)
                .Verifiable();

            SaveParametersResponse saveResponse = Response.Ok(new SaveParametersResponse(testParametersOutput));
            editorMock.Setup(target => target.SaveParameters())
                .Returns(saveResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock, editorMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            editorMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(testErrorCode, response.ErrorCode);
        }

        [Test]
        public void Update_WhenNoErrors_ReadsAndSetsParameterAndSaves()
        {
            const string testFilePath = "TestFile";
            const string testGameName = "Test Game";
            const string testParametersInput = "test input";
            const string testParametersOutput = "test output";
            var coreApiMock = new Mock<CoreApi>();

            SetUpCoreApiForFileReadAllTextSuccess(coreApiMock, testFilePath, testParametersInput);
            SetUpCoreApiForFileWriteAllTextSuccess(coreApiMock, testFilePath, testParametersOutput);

            var editorMock = new Mock<ScenarioParametersEditor>();

            var readResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.ReadParameters(testParametersInput))
                .Returns(readResponse)
                .Verifiable();

            var setResponse = Response.Ok(new Response());
            editorMock.Setup(target => target.SetParameter(ScenarioParameterKeys.GameName, testGameName))
                .Returns(setResponse)
                .Verifiable();

            SaveParametersResponse saveResponse = Response.Ok(new SaveParametersResponse(testParametersOutput));
            editorMock.Setup(target => target.SaveParameters())
                .Returns(saveResponse)
                .Verifiable();

            ScenarioParametersUpdater underTest = GetUnitUnderTest(coreApiMock, editorMock);

            // Act
            Response response = underTest.Update(testFilePath, PrepareParameters(testGameName));

            // Assert
            coreApiMock.Verify();
            editorMock.Verify();
            Assert.IsTrue(response.Success);
        }

        private void SetUpCoreApiForFileReadAllTextSuccess(Mock<CoreApi> coreApiMock, string testFilePath, string testParametersInput)
        {
            FileReadAllTextResponse response = Response.Ok(new FileReadAllTextResponse(testParametersInput));
            coreApiMock.Setup(target => target.FileReadAllText(testFilePath))
                .Returns(response)
                .Verifiable();
        }

        private void SetUpCoreApiForFileWriteAllTextSuccess(Mock<CoreApi> coreApiMock, string testFilePath, string testParametersOutput)
        {
            var response = Response.Ok(new Response());
            coreApiMock.Setup(target => target.FileWriteAllText(testFilePath, testParametersOutput))
                .Returns(response)
                .Verifiable();
        }

        private IReadOnlyDictionary<string, string> PrepareParameters(string gameName)
        {
            return new Dictionary<string, string>
            {
                { ScenarioParameterKeys.GameName, gameName }
            };
        }

        private ScenarioParametersUpdater GetUnitUnderTest(Mock<CoreApi> coreApi = null,
            Mock<ScenarioParametersEditor> scenarioParametersEditor = null)
        {
            coreApi = coreApi ?? new Mock<CoreApi>();
            scenarioParametersEditor = scenarioParametersEditor ?? new Mock<ScenarioParametersEditor>();
            return new ScenarioParametersUpdater(coreApi.Object, () => scenarioParametersEditor.Object);
        }
    }
}
