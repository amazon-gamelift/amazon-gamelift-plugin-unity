// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class DeployerTests
    {
        private const string TestS3BucketKey = "nonempty";
        private static readonly bool[] s_boolValues = new bool[] { true, false };

        [Test]
        public void StartDeployment_WhenSenarioFolderPathIsNull_ThrowsArgumentNullException()
        {
            const string testGameName = "test";

            var coreApiMock = new Mock<CoreApi>();
            TestedDeployer underTest = GetTestedDeployer(coreApiMock);

            AssertAsync.ThrowsAsync<ArgumentNullException>(async () => await underTest.StartDeployment(null, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask));
        }

        [Test]
        public void StartDeployment_WhenGameNameIsNull_ThrowsArgumentNullException()
        {
            const string testScenarioPath = "C:/test";

            var coreApiMock = new Mock<CoreApi>();
            TestedDeployer underTest = GetTestedDeployer(coreApiMock);

            AssertAsync.ThrowsAsync<ArgumentNullException>(async () => await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, null, true, ConfirmChangeSetTask));
        }

        #region StartDeployment Success

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndValidateCfnTemplateFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndValidateCfnTemplateFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndCreateChangeSetFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndCreateChangeSetFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndDescribeChangeSetFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndDescribeChangeSetFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndDescribeChangeSetNotAvailable_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndDescribeChangeSetNotAvailable();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndStackExistsFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndStackExistsFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndNotConfirmed_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndNotConfirmed();
                Assert.IsFalse(response.Success);
                Assert.AreEqual(response.ErrorCode, AmazonGameLift.Editor.ErrorCode.OperationCancelled);
            }
        }

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndDeploySuccessIsExpected_SuccessIsIsExpected(
            [ValueSource(nameof(s_boolValues))] bool expected)
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeployerBase _, DeploymentRequest _, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndDeploySuccess(expected);
                Assert.AreEqual(expected, response.Success);
            }
        }

        #endregion

        [UnityTest]
        public IEnumerator StartDeployment_WhenCreateRequestSuccessAndDeploySuccess_DeploymentIdIsExpected()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeployerBase deployer, DeploymentRequest request, DeploymentResponse response) = await TestStartDeploymentWhenCreateRequestSuccessAndDeploySuccess(true);
                Assert.AreEqual(request.Profile, response.DeploymentId.Profile);
                Assert.AreEqual(request.Region, response.DeploymentId.Region);
                Assert.AreEqual(deployer.DisplayName, response.DeploymentId.ScenarioName);
                Assert.AreEqual(request.StackName, response.DeploymentId.StackName);
            }
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestFailure()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";

            var coreApiMock = new Mock<CoreApi>();

            var requestFactoryMock = new Mock<DeploymentRequestFactory>(coreApiMock.Object);

            requestFactoryMock.Setup(target => target.CreateRequest(testScenarioPath, testGameName, true))
                .Returns((null, false, Response.Fail(new Response())))
                .Verifiable();

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndValidateCfnTemplateFailure()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";

            var coreApiMock = new Mock<CoreApi>();

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: false, request.Profile, request.Region, request.CfnTemplatePath);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndCreateChangeSetFailure()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpGetStackNameAsGameName(testGameName);

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: false, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndDescribeChangeSetFailure()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";
            const string testChangeSet = "test-set";

            var coreApiMock = new Mock<CoreApi>();

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key, testChangeSet);
            SetUpCoreApiWithDescribeChangeSet(coreApiMock, success: false, request.Profile, request.Region,
                request.StackName, testChangeSet);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndDescribeChangeSetNotAvailable()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";
            const string testChangeSet = "test-set";

            var coreApiMock = new Mock<CoreApi>();

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key, testChangeSet);
            SetUpCoreApiWithDescribeChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.StackName, testChangeSet, ChangeSetExecutionStatus.Obsolete);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndStackExistsFailure()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";
            const string testChangeSet = "test-set";

            var coreApiMock = new Mock<CoreApi>();

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key, testChangeSet);
            SetUpCoreApiWithDescribeChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.StackName, testChangeSet);
            coreApiMock.SetUpCoreApiWithDescribeStack(success: false, request.Profile, request.Region,
                request.StackName);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndNotConfirmed()
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";
            const string testChangeSet = "test-set";

            var coreApiMock = new Mock<CoreApi>();

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key, testChangeSet);
            SetUpCoreApiWithDescribeChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.StackName, testChangeSet);
            coreApiMock.SetUpCoreApiWithDescribeStack(success: true, request.Profile, request.Region,
                request.StackName);

            ConfirmChangesDelegate dontConfirmChanges = _ => Task.FromResult(false);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, dontConfirmChanges);

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeployerBase underTest, DeploymentRequest request, DeploymentResponse response)> TestStartDeploymentWhenCreateRequestSuccessAndDeploySuccess(bool deployReturnsSuccess)
        {
            const string testGameName = "test";
            const string testScenarioPath = "C:/test";
            const string testChangeSet = "test-set";

            var coreApiMock = new Mock<CoreApi>();
            coreApiMock.SetUpGetStackNameAsGameName(testGameName);

            Mock<DeploymentRequestFactory> requestFactoryMock = SetUpRequestMockForSuccess(testGameName,
                testScenarioPath, coreApiMock, out DeploymentRequest request);

            SetUpCoreApiWithValidateCfnTemplate(coreApiMock, success: true, request.Profile, request.Region, request.CfnTemplatePath);
            SetUpCoreApiWithCreateChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.BucketName, request.StackName, request.CfnTemplatePath, request.ParametersPath,
                request.GameName, request.LambdaFolderPath, request.BuildS3Key, testChangeSet);
            SetUpCoreApiWithDescribeChangeSet(coreApiMock, success: true, request.Profile, request.Region,
                request.StackName, testChangeSet);
            coreApiMock.SetUpCoreApiWithDescribeStack(success: true, request.Profile, request.Region,
                request.StackName, result: StackStatus.ReviewInProgress);

            TestedDeployer underTest = GetTestedDeployer(coreApiMock, deployReturnsSuccess);
            underTest.RequestFactory = requestFactoryMock.Object;

            // Act
            DeploymentResponse response = await underTest.StartDeployment(testScenarioPath, TestS3BucketKey, testGameName, true, ConfirmChangeSetTask);

            // Assert
            coreApiMock.Verify();
            return (underTest, request, response);
        }

        private static Mock<DeploymentRequestFactory> SetUpRequestMockForSuccess(string testGameName, string testScenarioPath,
            Mock<CoreApi> coreApiMock, out DeploymentRequest request)
        {
            const string testBuildPath = "C:/testbuild";
            var requestFactoryMock = new Mock<DeploymentRequestFactory>(coreApiMock.Object);
            var requestLocal = new DeploymentRequest()
            {
                BucketName = "test-bucket",
                CfnTemplatePath = "C:/test/template.yml",
                GameName = testGameName,
                LambdaFolderPath = "C:/test/l",
                ParametersPath = "C:/test/p.json",
                Profile = "test-profile",
                Region = "test-region",
                StackName = testGameName,
            };
            request = requestLocal;
            requestFactoryMock.Setup(target => target.CreateRequest(testScenarioPath, testGameName, true))
                .Returns((request, true, null))
                .Verifiable();
            requestFactoryMock.Setup(target => target.WithServerBuild(requestLocal, testBuildPath))
                .CallBase();
            return requestFactoryMock;
        }

        private static void SetUpCoreApiWithValidateCfnTemplate(Mock<CoreApi> coreApiMock, bool success,
            string profileName, string region, string templateFilePath)
        {
            var response = new ValidateCfnTemplateResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.ValidateCfnTemplate(profileName, region, templateFilePath))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpCoreApiWithCreateChangeSet(Mock<CoreApi> coreApiMock, bool success,
            string profileName, string region, string bucketName, string stackName,
            string templateFilePath, string parametersFilePath, string gameName, string lambdaFolderPath,
            string buildS3Key, string result = "test-cs")
        {
            var response = new CreateChangeSetResponse() { CreatedChangeSetName = result };
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.CreateChangeSet(profileName, region, bucketName,
                stackName, templateFilePath, parametersFilePath, gameName, lambdaFolderPath, buildS3Key))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpCoreApiWithDescribeChangeSet(Mock<CoreApi> coreApiMock, bool success,
            string profileName, string region, string stackName, string changeSetName,
            string resultExecutionStatus = ChangeSetExecutionStatus.Available)
        {
            var response = new DescribeChangeSetResponse()
            {
                ExecutionStatus = resultExecutionStatus,
                Changes = new Change[0]
            };
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.DescribeChangeSet(profileName, region, stackName, changeSetName))
                .Returns(response)
                .Verifiable();
        }

        private static Task<bool> ConfirmChangeSetTask(ConfirmChangesRequest _)
        {
            return Task.FromResult(true);
        }

        private static TestedDeployer GetTestedDeployer(Mock<CoreApi> coreApiMock, bool deployReturnsSuccess = true, bool hasGameServer = false)
        {
            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new TestedDeployer(delayMock.Object, coreApiMock.Object, deployReturnsSuccess, hasGameServer);
        }
    }
}
