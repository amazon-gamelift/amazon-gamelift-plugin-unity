// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class DeploymentRequestFactoryTests
    {
        private const string TestGameName = "test";
        private const string TestBuildPath = "C:/testbuild";
        private const string TestScenarioPath = "C:/test";

        #region CreateRequest

        [Test]
        public void CreateRequest_WhenSenarioFolderPathIsNull_ThrowsArgumentNullException()
        {
            var coreApiMock = new Mock<CoreApi>();
            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            Assert.Throws<ArgumentNullException>(() => underTest.CreateRequest(null, TestGameName, true));
        }

        [Test]
        public void CreateRequest_WhenSenarioFolderPathIsInvalid_ThrowsArgumentException()
        {
            string testPath = Path.GetInvalidPathChars().First().ToString();

            var coreApiMock = new Mock<CoreApi>();
            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            Assert.Throws<ArgumentException>(() => underTest.CreateRequest(testPath, TestGameName, true));
        }

        [Test]
        public void CreateRequest_WhenGameNameIsNull_ThrowsArgumentNullException()
        {
            var coreApiMock = new Mock<CoreApi>();
            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            Assert.Throws<ArgumentNullException>(() => underTest.CreateRequest(TestScenarioPath, null, true));
        }

        [Test]
        public void CreateRequest_WhenProfileNotSet_SuccessIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: false);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            (DeploymentRequest _, bool success, Response failedResponse) = underTest
                .CreateRequest(TestScenarioPath, TestGameName, true);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNotNull(failedResponse);
        }

        [Test]
        public void CreateRequest_WhenRegionNotSet_SuccessIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: false, valid: false);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            (DeploymentRequest _, bool success, Response failedResponse) = underTest
                .CreateRequest(TestScenarioPath, TestGameName, true);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNotNull(failedResponse);
        }

        [Test]
        public void CreateRequest_WhenBucketNotSet_SuccessIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true);
            coreApiMock.SetUpCoreApiWithBucket(success: false);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            (DeploymentRequest _, bool success, Response failedResponse) = underTest
                .CreateRequest(TestScenarioPath, TestGameName, true);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNotNull(failedResponse);
        }

        [Test]
        public void CreateRequest_WhenAllSet_SuccessIsTrue()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true);
            coreApiMock.SetUpCoreApiWithBucket(success: true);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            (DeploymentRequest request, bool success, Response failedResponse) = underTest
                .CreateRequest(TestScenarioPath, TestGameName, true);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(request);
            Assert.IsNull(failedResponse);
        }

        [Test]
        public void CreateRequest_WhenAllSet_RequestIsValid()
        {
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpCoreApiWithProfile(success: true);
            coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true);
            coreApiMock.SetUpCoreApiWithBucket(success: true);
            coreApiMock.SetUpGetStackNameAsGameName(TestGameName);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            (DeploymentRequest request, bool _, Response _) = underTest
                .CreateRequest(TestScenarioPath, TestGameName, true);

            // Assert
            Assert.IsNotNull(request);
            Assert.IsNotNull(request.BucketName);
            Assert.IsNotNull(request.CfnTemplatePath);
            Assert.IsNotNull(request.GameName);
            Assert.IsNotNull(request.LambdaFolderPath);
            Assert.IsNotNull(request.ParametersPath);
            Assert.IsNotNull(request.Profile);
            Assert.IsNotNull(request.Region);
            Assert.IsNotNull(request.StackName);
        }

        #endregion

        #region

        [Test]
        public void WithServerBuild_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            var coreApiMock = new Mock<CoreApi>();
            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            Assert.Throws<ArgumentNullException>(() => underTest.WithServerBuild(null, TestBuildPath));
        }

        [Test]
        public void WithServerBuild_WhenBuildFolderPathIsNull_ThrowsArgumentNullException()
        {
            var coreApiMock = new Mock<CoreApi>();
            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            Assert.Throws<ArgumentNullException>(() => underTest.WithServerBuild(new DeploymentRequest(), null));
        }

        [Test]
        public void WithServerBuild_WhenAllSet_ModifiesRequest()
        {
            const string testKey = "testKey";
            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpGetBuildS3Key(testKey);

            var underTest = new DeploymentRequestFactory(coreApiMock.Object, false);

            DeploymentRequest result = underTest.WithServerBuild(new DeploymentRequest(), TestBuildPath);

            // Assert
            Assert.AreEqual(TestBuildPath, result.BuildFolderPath);
            Assert.AreEqual(testKey, result.BuildS3Key);
        }

        #endregion
    }
}
