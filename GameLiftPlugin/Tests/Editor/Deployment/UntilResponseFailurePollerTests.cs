// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class UntilResponseFailurePollerTests
    {
        [UnityTest]
        public IEnumerator Poll_WhenStopConditionIsTrueAndResponseOk_ReturnsExpectedResponse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var returnedResponse = new Response();
                UntilResponseFailurePoller underTest = GetUnitUnderTest();

                Response response = await underTest.Poll(0, () => Response.Ok(returnedResponse), _ => true);
                Assert.AreEqual(returnedResponse, response);
            }
        }

        [UnityTest]
        public IEnumerator Poll_WhenStopConditionIsFalseAndResponseFail_ReturnsExpectedResponse()
        {
            yield return Run().AsCoroutine();

            async Task Run()
            {
                var returnedResponse = new Response();
                UntilResponseFailurePoller underTest = GetUnitUnderTest();

                Response response = await underTest.Poll(0, () => Response.Fail(returnedResponse), _ => false);
                Assert.AreEqual(returnedResponse, response);
                Assert.IsFalse(response.Success);
            }
        }

        private static UntilResponseFailurePoller GetUnitUnderTest()
        {
            var delayMock = new Mock<Delay>();
            delayMock.Setup(target => target.Wait(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new UntilResponseFailurePoller(delayMock.Object);
        }
    }
}
