// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class AwsCredentialsTests
    {
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        [Test]
        public void Refresh_WhenGetProfilesError_CanSelectIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            var response = new GetProfilesResponse() { Profiles = new List<string>() };
            response = Response.Fail(response);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(response)
                .Verifiable();

            AwsCredentials awsCredentials = GetAwsCredentialsWithStubComponents(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(false, awsCredentials.CanSelect);
        }

        [Test]
        public void Refresh_WhenNoProfiles_CanSelectIsFalse()
        {
            var coreApiMock = new Mock<CoreApi>();
            var response = new GetProfilesResponse() { Profiles = new List<string>() };
            response = Response.Ok(response);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(response)
                .Verifiable();

            AwsCredentials awsCredentials = GetAwsCredentialsWithStubComponents(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(false, awsCredentials.CanSelect);
        }

        [Test]
        public void Refresh_WhenAnyProfiles_CanSelectIsTrue()
        {
            var coreApiMock = new Mock<CoreApi>();
            var profilesResponse = new GetProfilesResponse()
            {
                Profiles = new List<string> { "NonEmpty" }
            };
            profilesResponse = Response.Ok(profilesResponse);
            coreApiMock.Setup(target => target.ListCredentialsProfiles())
                .Returns(profilesResponse)
                .Verifiable();

            AwsCredentials awsCredentials = GetAwsCredentialsWithStubComponents(coreApiMock.Object);

            // Act
            awsCredentials.Refresh();

            // Assert
            coreApiMock.Verify();
            Assert.AreEqual(true, awsCredentials.CanSelect);
        }

        private AwsCredentials GetAwsCredentialsWithStubComponents(CoreApi coreApi)
        {
            var regionMock = new Mock<RegionBootstrap>(coreApi);
            regionMock.Setup(target => target.Refresh())
                .Verifiable();

            var mockLogger = new MockLogger();
            var creationMock = new Mock<AwsCredentialsCreation>(_textProvider, regionMock.Object, coreApi, mockLogger);
            creationMock.Setup(target => target.Refresh())
                .Verifiable();

            var updateMock = new Mock<AwsCredentialsUpdate>(_textProvider, regionMock.Object, coreApi, mockLogger);
            updateMock.Setup(target => target.Refresh())
                .Verifiable();

            return new AwsCredentials(creationMock.Object, updateMock.Object, coreApi);
        }
    }
}
