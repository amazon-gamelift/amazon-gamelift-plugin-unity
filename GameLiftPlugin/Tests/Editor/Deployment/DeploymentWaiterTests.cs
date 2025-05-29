// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using Moq.Language;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class DeploymentWaiterTests
    {
        private const string TestProfile = "test";
        private const string TestScenarioName = "Test Name";
        private const string TestRegion = "test-region";
        private const string TestStackName = "test-name";
        private static readonly bool[] s_boolValues = new bool[] { true, false };

        #region WaitUntilDone Success

        [UnityTest]
        public IEnumerator WaitUntilDone_WhenDescribeStackFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();

                DescribeStackResponse response1 = Response.Fail(new DescribeStackResponse());

                (DeploymentWaiter _, Task<DeploymentResponse> waitTask) =
                    SetUpWaitUntilDoneWithDescribeStackResponses(new[] { response1 }, coreApiMock);

                // Act
                DeploymentResponse response = await waitTask;

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator WaitUntilDone_WhenDescribeStackReviewAndFailure_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                (DeploymentWaiter _, DeploymentResponse response) = await TestWaitUntilDoneWhenDescribeStackReviewAndFailure();
                Assert.IsFalse(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator WaitUntilDone_WhenDescribeStackInvalidStatus_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeploymentWaiter _, DeploymentResponse response) = await TestWaitUntilDoneWhenDescribeStackReviewAndInvalidStatus();
                Assert.IsFalse(response.Success);
                Assert.AreEqual(AmazonGameLift.Editor.ErrorCode.StackStatusInvalid, response.ErrorCode);
            }
        }

        [UnityTest]
        public IEnumerator WaitUntilDone_WhenDescribeStackCreateComplete_SuccessIsTrue()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeploymentWaiter _, DeploymentResponse response) = await TestWaitUntilDoneWhenDescribeStackStatusCreateComplete();
                Assert.IsTrue(response.Success);
            }
        }

        [UnityTest]
        public IEnumerator WaitUntilDone_WhenCancelWaiting_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                DescribeStackResponse[] responses = new[]
                {
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.ReviewInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateComplete }),
                };

                (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) = SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);
                underTest.CancelWaiting();

                // Act
                Response response = await waitTask;

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(response.Success);
                Assert.AreEqual(AmazonGameLift.Editor.ErrorCode.OperationCancelled, response.ErrorCode);
            }
        }

        #endregion

        #region CancelWaiting

        [Test]
        public void CancelWaiting_WhenNotWaiting_SuccessIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            DeploymentWaiter underTest = GetUnitUnderTest(coreApiMock);
            Response response = underTest.CancelWaiting();
            Assert.IsFalse(response.Success);
        }

        [UnityTest]
        public IEnumerator CancelWaiting_WhenWaitUntilDone_SuccessIsTrue()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                DescribeStackResponse[] responses = new[]
                {
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.ReviewInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateComplete }),
                };

                (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) = SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);

                // Act
                Response response = underTest.CancelWaiting();

                // Assert
                coreApiMock.Verify();
                Assert.IsTrue(response.Success);

                await waitTask;
            }
        }

        [UnityTest]
        public IEnumerator CancelWaiting_WhenWaitUntilDoneAndCancelWaiting_SuccessIsFalse()
        {
            yield return Run().AsCoroutine();
            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();
                DescribeStackResponse[] responses = new[]
                {
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.ReviewInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateInProgress }),
                    Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateComplete }),
                };

                (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) = SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);
                underTest.CancelWaiting();

                // Act
                Response response = underTest.CancelWaiting();

                // Assert
                coreApiMock.Verify();
                Assert.IsFalse(response.Success);
                await waitTask;
            }
        }

        #endregion

        #region CanCancel

        [Test]
        public void CanCancel_WhenNewInstance_IsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            DeploymentWaiter underTest = GetUnitUnderTest(coreApiMock);

            Assert.IsFalse(underTest.CanCancel);
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenDescribeStackReviewAndFailure_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeploymentWaiter underTest, DeploymentResponse _) = await TestWaitUntilDoneWhenDescribeStackReviewAndFailure();
                Assert.IsFalse(underTest.CanCancel);
            }
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenWaitUntilDoneAndDescribeStackReviewAndInvalidStatus_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeploymentWaiter underTest, DeploymentResponse _) = await TestWaitUntilDoneWhenDescribeStackReviewAndInvalidStatus();
                Assert.IsFalse(underTest.CanCancel);
            }
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenWaitUntilDoneAndDescribeStackStatusCreateComplete_IsFalse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                (DeploymentWaiter underTest, DeploymentResponse _) = await TestWaitUntilDoneWhenDescribeStackStatusCreateComplete();
                Assert.IsFalse(underTest.CanCancel);
            }
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenWaitUntilDoneAndNewStackAndCancelDeploymentAndCoreNotExpected_IsExpected([ValueSource(nameof(s_boolValues))] bool expected)
        {
            var coreApiMock = new Mock<CoreApi>();
            (DeploymentWaiter underTest, Task<DeploymentResponse> _) = SetUpCancelDeploymentWhenNewStack(coreApiSuccess: !expected, coreApiMock);

            while (!underTest.CanCancel)
            {
                yield return null;
            }

            // Act
            underTest.CancelDeployment();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.CanCancel);
        }

        [UnityTest]
        public IEnumerator CanCancel_WhenWaitUntilDoneAndStackExistsAndCancelDeploymentAndCoreNotExpected_IsExpected([ValueSource(nameof(s_boolValues))] bool expected)
        {
            var coreApiMock = new Mock<CoreApi>();
            (DeploymentWaiter underTest, Task<DeploymentResponse> _) = SetUpCancelDeploymentWhenStackExists(coreApiSuccess: !expected, coreApiMock);

            while (!underTest.CanCancel)
            {
                yield return null;
            }

            // Act
            underTest.CancelDeployment();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(expected, underTest.CanCancel);
        }

        #endregion

        #region CancelDeployment

        [Test]
        public void CancelDeployment_WhenCanCancelIsFalse_SuccessIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();

            coreApiMock.Verify(
                target => target.CancelDeployment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);

            coreApiMock.Verify(
                target => target.DeleteStack(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);

            DeploymentWaiter underTest = GetUnitUnderTest(coreApiMock);

            Response cancelResponse = underTest.CancelDeployment();

            // Assert
            coreApiMock.Verify();
            Assert.IsFalse(cancelResponse.Success);
            Assert.AreEqual(AmazonGameLift.Editor.ErrorCode.OperationInvalid, cancelResponse.ErrorCode);
        }

        [UnityTest]
        public IEnumerator CancelDeployment_WhenWaitUntilDoneAndNewStackAndCanCancelIsTrueAndCoreExpected_SuccessIsExpected([ValueSource(nameof(s_boolValues))] bool expected)
        {
            var coreApiMock = new Mock<CoreApi>();
            (DeploymentWaiter underTest, Task<DeploymentResponse> _) = SetUpCancelDeploymentWhenNewStack(coreApiSuccess: expected, coreApiMock);

            while (!underTest.CanCancel)
            {
                yield return null;
            }

            // Act
            Response cancelResponse = underTest.CancelDeployment();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(expected, cancelResponse.Success);
        }

        [UnityTest]
        public IEnumerator CancelDeployment_WhenWaitUntilDoneAndStackExistsAndCanCancelIsTrueAndCoreExpected_SuccessIsExpected([ValueSource(nameof(s_boolValues))] bool expected)
        {
            var coreApiMock = new Mock<CoreApi>();
            (DeploymentWaiter underTest, Task<DeploymentResponse> _) = SetUpCancelDeploymentWhenStackExists(coreApiSuccess: expected, coreApiMock);

            while (!underTest.CanCancel)
            {
                yield return null;
            }

            // Act
            Response cancelResponse = underTest.CancelDeployment();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(expected, cancelResponse.Success);
        }

        #endregion

        #region InfoUpdated

        [UnityTest]
        public IEnumerator InfoUpdated_WhenWaitUntilDoneAndDescribeStackStatusPolled_IsRaised()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                const int expectedUpdateCount = 3;
                string[] statuses = new[]
                {
                    StackStatus.UpdateInProgress,
                    StackStatus.UpdateInProgress,
                    StackStatus.UpdateComplete
                };

                var coreApiMock = new Mock<CoreApi>();

                DescribeStackResponse[] responses = CreateDescribeStackSequenceResponses(statuses);
                SetUpCoreApiWithDescribeStackSequence(coreApiMock, TestProfile, TestRegion, TestStackName, responses);

                var deploymentId = new DeploymentId(TestProfile, TestRegion, TestStackName, TestScenarioName);
                DeploymentWaiter underTest = GetUnitUnderTest(coreApiMock);

                int updateCount = 0;
                var updates = new List<DeploymentInfo>();
                underTest.InfoUpdated += info =>
                {
                    updateCount++;
                    updates.Add(info);
                };

                DeploymentResponse response = await underTest.WaitUntilDone(deploymentId);

                Assert.AreEqual(expectedUpdateCount, updateCount);

                for (int i = 0; i < updates.Count; i++)
                {
                    DeploymentInfo info = updates[i];
                    Assert.AreEqual(statuses[i], info.StackStatus);
                }
            }
        }

        [UnityTest]
        public IEnumerator InfoUpdated_WhenWaitUntilDoneAndDescribeStackStatusPollFails_IsNotRaised()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var coreApiMock = new Mock<CoreApi>();

                DescribeStackResponse response1 = Response.Fail(new DescribeStackResponse());

                (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) =
                    SetUpWaitUntilDoneWithDescribeStackResponses(new[] { response1 }, coreApiMock);

                int updateCount = 0;
                underTest.InfoUpdated += info =>
                {
                    updateCount++;
                };

                // Act
                DeploymentResponse response = await waitTask;

                // Assert
                coreApiMock.Verify();
                Assert.AreEqual(0, updateCount);
            }
        }

        #endregion

        private static (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) SetUpCancelDeploymentWhenNewStack(bool coreApiSuccess, Mock<CoreApi> coreApiMock)
        {
            DescribeStackResponse[] responses = new[]
            {
                Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.ReviewInProgress }),
                Response.Ok(new DescribeStackResponse { StackStatus = StackStatus.CreateInProgress }),
                Response.Fail(new DescribeStackResponse { ErrorCode = "TestError" }),
            };

            SetUpCoreApiWithDeleteStack(coreApiMock, success: coreApiSuccess, TestProfile, TestRegion, TestStackName);
            return SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);
        }

        private static (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) SetUpCancelDeploymentWhenStackExists(bool coreApiSuccess, Mock<CoreApi> coreApiMock)
        {
            string[] statuses = new[]
            {
                StackStatus.UpdateInProgress,
                StackStatus.UpdateRollbackInProgress,
                StackStatus.UpdateRollbackComplete
            };

            SetUpCoreApiWithCancelDeployment(coreApiMock, success: coreApiSuccess, TestProfile, TestRegion, TestStackName);
            DescribeStackResponse[] responses = CreateDescribeStackSequenceResponses(statuses);
            return SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);
        }

        private static (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask)
            SetUpWaitUntilDoneWithDescribeStackResponses(DescribeStackResponse[] responses, Mock<CoreApi> coreApiMock)
        {
            SetUpCoreApiWithDescribeStackSequence(coreApiMock, TestProfile, TestRegion, TestStackName, responses);

            var deploymentId = new DeploymentId(TestProfile, TestRegion, TestStackName, TestScenarioName);
            DeploymentWaiter underTest = GetUnitUnderTest(coreApiMock);

            Task<DeploymentResponse> waitTask = underTest.WaitUntilDone(deploymentId);
            return (underTest, waitTask);
        }

        private async Task<(DeploymentWaiter underTest, DeploymentResponse response)> TestWaitUntilDoneWhenDescribeStackReviewAndFailure()
        {
            var coreApiMock = new Mock<CoreApi>();

            DescribeStackResponse response1 = Response.Ok(new DescribeStackResponse() { StackStatus = StackStatus.ReviewInProgress });
            DescribeStackResponse response2 = Response.Fail(new DescribeStackResponse());

            (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) =
                SetUpWaitUntilDoneWithDescribeStackResponses(new[] { response1, response2 }, coreApiMock);

            // Act
            DeploymentResponse response = await waitTask;

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeploymentWaiter underTest, DeploymentResponse response)> TestWaitUntilDoneWhenDescribeStackReviewAndInvalidStatus()
        {
            var coreApiMock = new Mock<CoreApi>();
            string[] statuses = new[]
            {
                StackStatus.ReviewInProgress,
                StackStatus.DeleteComplete,
            };

            DescribeStackResponse[] responses = CreateDescribeStackSequenceResponses(statuses);
            (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) =
                SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);

            // Act
            DeploymentResponse response = await waitTask;

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private async Task<(DeploymentWaiter underTest, DeploymentResponse response)> TestWaitUntilDoneWhenDescribeStackStatusCreateComplete()
        {
            var coreApiMock = new Mock<CoreApi>();
            string[] statuses = new[]
            {
                StackStatus.ReviewInProgress,
                StackStatus.CreateComplete,
            };

            DescribeStackResponse[] responses = CreateDescribeStackSequenceResponses(statuses);
            (DeploymentWaiter underTest, Task<DeploymentResponse> waitTask) =
                SetUpWaitUntilDoneWithDescribeStackResponses(responses, coreApiMock);

            // Act
            DeploymentResponse response = await waitTask;

            // Assert
            coreApiMock.Verify();
            return (underTest, response);
        }

        private static DescribeStackResponse[] CreateDescribeStackSequenceResponses(IEnumerable<string> results)
        {
            return results.Select(status => Response.Ok(new DescribeStackResponse() { StackStatus = status })).ToArray();
        }

        private static void SetUpCoreApiWithDescribeStackSequence(Mock<CoreApi> coreApiMock,
            string profileName, string region, string stackName,
            DescribeStackResponse[] responses)
        {
            ISetupSequentialResult<DescribeStackResponse> setup = coreApiMock
                .SetupSequence(target => target.DescribeStack(profileName, region, stackName));

            foreach (DescribeStackResponse item in responses)
            {
                setup = setup.Returns(item);
            }
        }

        private static void SetUpCoreApiWithCancelDeployment(Mock<CoreApi> coreApiMock, bool success,
            string profileName = "test-profile", string region = "test-region", string stackName = "test-stack")
        {
            var response = new CancelDeploymentResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.CancelDeployment(profileName, region, stackName, It.IsAny<string>()))
                .Returns(response)
                .Verifiable();
        }

        private static void SetUpCoreApiWithDeleteStack(Mock<CoreApi> coreApiMock, bool success,
            string profileName = "test-profile", string region = "test-region", string stackName = "test-stack")
        {
            var response = new DeleteStackResponse();
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.DeleteStack(profileName, region, stackName))
                .Returns(response)
                .Verifiable();
        }

        private static DeploymentWaiter GetUnitUnderTest(Mock<CoreApi> coreApiMock)
        {
            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new DeploymentWaiter(delayMock.Object, coreApiMock.Object);
        }
    }
}
