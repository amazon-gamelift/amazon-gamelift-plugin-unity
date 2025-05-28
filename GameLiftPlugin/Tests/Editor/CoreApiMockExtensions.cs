// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.AccountManagement.Models;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    internal static class CoreApiMockExtensions
    {
        public static void SetUpCoreApiForBootstrapSuccess(this Mock<CoreApi> coreApiMock,
            string testBucketName = "test-bucket", string testRegion = "test-region")
        {
            SetUpCoreApiWithBucket(coreApiMock, true, testBucketName);
            SetUpCoreApiWithRegion(coreApiMock, true, true, testRegion);
        }

        public static void SetUpCoreApiWithRegion(this Mock<CoreApi> coreApiMock, bool success, bool valid, string testRegion = "test-region")
        {
            if (coreApiMock is null)
            {
                throw new ArgumentNullException(nameof(coreApiMock));
            }

            SetUpCoreApiWithSetting(coreApiMock, SettingsKeys.CurrentRegion, success, testRegion);

            coreApiMock.Setup(target => target.IsValidRegion(testRegion))
                .Returns(valid);
        }

        public static void SetUpCoreApiWithBucket(this Mock<CoreApi> coreApiMock, bool success, string testBucketName = "test-bucket")
        {
            if (coreApiMock is null)
            {
                throw new ArgumentNullException(nameof(coreApiMock));
            }

            SetUpCoreApiWithSetting(coreApiMock, SettingsKeys.CurrentBucketName, success, testBucketName);
        }

        public static void SetUpCoreApiWithProfile(this Mock<CoreApi> coreApiMock, bool success, string profile = "test-profile")
        {
            SetUpCoreApiWithSetting(coreApiMock, SettingsKeys.CurrentProfileName, success, profile);
        }

        public static void SetUpCoreApiWithAccountId(this Mock<CoreApi> coreApiMock, bool success, string accountId = "test-id")
        {
            var accountIdResponse = new RetrieveAccountIdByCredentialsResponse()
            {
                AccountId = accountId
            };
            accountIdResponse = success ? Response.Ok(accountIdResponse) : Response.Fail(accountIdResponse);

            coreApiMock.Setup(target => target.RetrieveAccountId(It.IsAny<string>()))
                .Returns(accountIdResponse);
        }

        public static void SetUpGetStackNameAsGameName(this Mock<CoreApi> coreApiMock, string gameName)
        {
            coreApiMock.Setup(target => target.GetStackName(gameName))
                .Returns(gameName);
        }

        public static void SetUpGetBuildS3Key(this Mock<CoreApi> coreApiMock, string result)
        {
            coreApiMock.Setup(target => target.GetBuildS3Key())
                .Returns(result);
        }

        public static void SetUpCoreApiWithDescribeStack(this Mock<CoreApi> coreApiMock, bool success,
            string profileName = "test-profile", string region = "test-region", string stackName = "test-stack",
            string result = StackStatus.CreateComplete, string gameName = "test-game", Dictionary<string, string> outputs = null)
        {
            var response = new DescribeStackResponse()
            {
                StackId = "test-stack-id",
                StackStatus = result,
                GameName = gameName,
                Outputs = outputs ?? new Dictionary<string, string>()
            };
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.DescribeStack(profileName, region, stackName))
                .Returns(response)
                .Verifiable();
        }

        public static List<string> SetUpWithTestProfileListOf2(this Mock<CoreApi> coreApiMock)
        {
            if (coreApiMock is null)
            {
                throw new ArgumentNullException(nameof(coreApiMock));
            }

            var testProfiles = new List<string> { "NonEmpty", "Current" };
            var profilesResponse = new GetProfilesResponse()
            {
                Profiles = testProfiles
            };
            profilesResponse = Response.Ok(profilesResponse);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(profilesResponse)
                .Verifiable();

            return testProfiles;
        }

        internal static void SetUpCoreApiWithSetting(this Mock<CoreApi> coreApiMock, SettingsKeys key, bool success, string successResult)
        {
            var response = new GetSettingResponse() { Value = success ? successResult : null };
            response = success ? Response.Ok(response) : Response.Fail(response);
            coreApiMock.Setup(target => target.GetSetting(key))
                .Returns(response);
        }
    }
}
