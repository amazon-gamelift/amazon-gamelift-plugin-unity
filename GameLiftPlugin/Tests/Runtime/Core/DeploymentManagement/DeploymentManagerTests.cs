// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.S3;
using Amazon.S3.Model;
using AmazonGameLiftPlugin.Core.DeploymentManagement;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.FileZip;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.DeploymentManagement
{
    [TestFixture]
    public class DeploymentManagerTests
    {
        [Test]
        [Ignore("Only for development")]
        public void IntegrationTest()
        {
            string accessKey = "";
            string secretKey = "";
            var amazonS3Client = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    RegionEndpoint.EUWest1
                );

            var deploymentManager = new DeploymentManager(
                        new AmazonCloudFormationWrapper(
                                accessKey,
                                secretKey,
                                "eu-west-1"
                            ),
                        new AmazonS3Wrapper(amazonS3Client),
                        new FileWrapper(),
                        new FileZip());
        }

        [Test]
        public void CreateChangeSet_WhenStackDoesNotExists_CreatesStackWithChangeSet()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Throws(new AmazonCloudFormationException("Stack does not exist"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            amazonCloudFomrationClientMock.Verify(
                    x => x.CreateChangeSet(It.Is<CreateChangeSetRequest>(
                        p => p.StackName == stackName && p.ChangeSetType == ChangeSetType.CREATE)),
                    Times.Once()
                );

            Assert.IsTrue(response.Success);
        }

        [Test]
        public void CreateChangeSet_WhenStackExists_CreatesChangeSetForUpdate()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>
                    {
                        new Stack()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            amazonCloudFomrationClientMock.Verify(
                    x => x.CreateChangeSet(It.Is<CreateChangeSetRequest>(
                        p => p.StackName == stackName && p.ChangeSetType == ChangeSetType.UPDATE)),
                    Times.Once()
                );

            Assert.IsTrue(response.Success);
        }

        [Test]
        public void CreateChangeSet_WhenBootstrapDoesNotExist_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(false);

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.BucketDoesNotExist, response.ErrorCode);
        }

        [Test]
        public void CreateChangeSet_WhenUploadingLambdaIsNotSuccessful_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            fileZipMock.Setup(x => x.Zip(It.IsAny<string>(), It.IsAny<string>())).Throws(new System.Exception());

            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName,
                    LambdaSourcePath = "NonEmptyPath",
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.UnknownError, response.ErrorCode);
        }

        [Test]
        public void CreateChangeSet_WhenTemplateUploadIsNotSuccessful_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            s3WrapperMock.Setup(x => x.PutObject(It.IsAny<PutObjectRequest>()))
                .Throws(new AmazonS3Exception("BucketDoesNotExists"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.AwsError, response.ErrorCode);
        }

        [Test]
        public void CreateChangeSet_WhenSKDThrowsAlreadyExistsException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            amazonCloudFomrationClientMock.Setup(x => x.CreateChangeSet(It.IsAny<CreateChangeSetRequest>())).Throws(new AlreadyExistsException(""));

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>
                    {
                        new Stack()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.ResourceWithTheNameRequestetAlreadyExists, response.ErrorCode);
        }

        [Test]
        public void CreateChangeSet_WhenSKDThrowsInsufficientCapabilitiesException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            amazonCloudFomrationClientMock.Setup(x => x.CreateChangeSet(It.IsAny<CreateChangeSetRequest>())).Throws(new InsufficientCapabilitiesException(""));

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>
                    {
                        new Stack()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InsufficientCapabilities, response.ErrorCode);
        }

        [Test]
        public void CreateChangeSet_WhenSKDThrowsLimitExceededException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";
            string bootstrapBucketName = "bucketname1";

            fileWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(string.Empty);
            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.DoesBucketExist(bootstrapBucketName))
                .Returns(true);

            amazonCloudFomrationClientMock.Setup(x => x.CreateChangeSet(It.IsAny<CreateChangeSetRequest>())).Throws(new LimitExceededException(""));

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>
                    {
                        new Stack()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CreateChangeSetResponse response =
                deploymentManager.CreateChangeSet(new Core.DeploymentManagement.Models.CreateChangeSetRequest
                {
                    StackName = stackName,
                    ParametersFilePath = "NonEmptyPath",
                    TemplateFilePath = "NonEmptyPath",
                    BootstrapBucketName = bootstrapBucketName
                });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.LimitExceeded, response.ErrorCode);
        }

        [Test]
        public void DescribeStack_WhenStackExists_ReturnsOutputsWithStatus()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";

            string outputKey1 = "Key1";
            string outputKey2 = "Key2";
            var stack = new Stack
            {
                StackName = stackName,
                Outputs = new List<Output>
                {
                    new Output { OutputKey=outputKey1,OutputValue= "Value1" },
                    new Output { OutputKey=outputKey2,OutputValue= "Value2" }
                }
            };
            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>()
                    {
                        stack
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DescribeStackResponse response =
                deploymentManager.DescribeStack(new Core.DeploymentManagement.Models.DescribeStackRequest
                {
                    StackName = stackName,
                    OutputKeys = new List<string>
                {
                    outputKey1,
                    outputKey2
                }
                });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsTrue(response.Success);
            Assert.AreEqual(2, response.Outputs.Count);
            Assert.IsTrue(response.Outputs.ContainsKey(outputKey1));
            Assert.IsTrue(response.Outputs.ContainsKey(outputKey2));
        }

        [Test]
        public void DescribeStack_WhenStackDoesNotExists_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>()
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DescribeStackResponse response =
                deploymentManager.DescribeStack(new Core.DeploymentManagement.Models.DescribeStackRequest
                {
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(response.Success);
        }

        [Test]
        public void DescribeChangeSet_WhenChangeSetExists_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.DescribeChangeSet(
                It.Is<DescribeChangeSetRequest>(p => p.ChangeSetName == changeSetName && p.StackName == stackName)))
                .Returns(new DescribeChangeSetResponse
                {
                    Changes = new List<Change>
                    {
                        new Change(),
                        new Change()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DescribeChangeSetResponse response = deploymentManager.DescribeChangeSet(new Core.DeploymentManagement.Models.DescribeChangeSetRequest
            {
                ChangeSetName = changeSetName,
                StackName = stackName
            });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.NotNull(response.Changes);
            Assert.AreEqual(2, response.Changes.Count());
        }

        [Test]
        public void DescribeChangeSet_WhenChangeSetDoesNotExists_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.DescribeChangeSet(
                It.Is<DescribeChangeSetRequest>(p => p.ChangeSetName == changeSetName && p.StackName == stackName)))
                .Throws(new AmazonCloudFormationException("ChangeSet does not exists"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DescribeChangeSetResponse response = deploymentManager.DescribeChangeSet(new Core.DeploymentManagement.Models.DescribeChangeSetRequest
            {
                ChangeSetName = changeSetName,
                StackName = stackName
            });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.AwsError, response.ErrorCode);
            Assert.AreEqual("ChangeSet does not exists", response.ErrorMessage);
        }

        [Test]
        public void ExecuteChangeSet_WhenChangeSetExists_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Returns(new ExecuteChangeSetResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void ExecuteChangeSet_WhenChangeSetDoesNotExists_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new AmazonCloudFormationException("ChangeSet does not exists"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.AwsError, response.ErrorCode);
            Assert.AreEqual("ChangeSet does not exists", response.ErrorMessage);
        }

        [Test]
        public void ExecuteChangeSet_WhenSDKThrowsTokenAlreadyExistsException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new TokenAlreadyExistsException(""));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.TokenAlreadyExists, response.ErrorCode);
        }

        [Test]
        public void ExecuteChangeSet_WhenSDKThrowsInvalidChangeSetStatusException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new InvalidChangeSetStatusException(""));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidChangeSetStatus, response.ErrorCode);
        }

        [Test]
        public void ExecuteChangeSet_WhenSDKThrowsInsufficientCapabilitiesException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new InsufficientCapabilitiesException(""));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InsufficientCapabilities, response.ErrorCode);
        }

        [Test]
        public void ExecuteChangeSet_WhenSDKThrowsChangeSetNotFoundException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.ExecuteChangeSet(
                It.Is<ExecuteChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new ChangeSetNotFoundException(""));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.ExecuteChangeSetResponse response
                = deploymentManager.ExecuteChangeSet(new Core.DeploymentManagement.Models.ExecuteChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.ChangeSetNotFound, response.ErrorCode);
        }

        [Test]
        public void DescribeChangeSet_WhenSDKThrowsChangeSetNotFoundException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string changeSetName = "NonEmpty";
            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(x => x.DescribeChangeSet(
                It.Is<DescribeChangeSetRequest>(p => p.ChangeSetName == changeSetName
                    && p.StackName == stackName)))
                .Throws(new ChangeSetNotFoundException("ChangeSet does not exists"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DescribeChangeSetResponse response
                = deploymentManager.DescribeChangeSet(new Core.DeploymentManagement.Models.DescribeChangeSetRequest
                {
                    ChangeSetName = changeSetName,
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.ChangeSetNotFound, response.ErrorCode);
        }

        [Test]
        public void StackExists_WhenStackExists_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Returns(new DescribeStacksResponse
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                    Stacks = new List<Stack>
                    {
                        new Stack()
                    }
                });

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.StackExistsResponse response
                = deploymentManager.StackExists(new Core.DeploymentManagement.Models.StackExistsRequest
                {
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.Exists);
        }

        [Test]
        public void StackExists_WhenStackDoesNotExists_ReturnsNotExists()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            string stackName = "ValidStackName";

            amazonCloudFomrationClientMock.Setup(
                x => x.DescribeStacks(
                    It.Is<DescribeStacksRequest>(p => p.StackName == stackName)))
                .Throws(new AmazonCloudFormationException("Stack Does not exists"));

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.StackExistsResponse response
                = deploymentManager.StackExists(new Core.DeploymentManagement.Models.StackExistsRequest
                {
                    StackName = stackName
                });

            amazonCloudFomrationClientMock.Verify();
            Assert.IsFalse(response.Exists);
        }

        [Test]
        public void UploadServerBuild_WhenParametersAreEmpty_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.UploadServerBuildResponse response = deploymentManager.UploadServerBuild(new Core.DeploymentManagement.Models.UploadServerBuildRequest());

            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.InvalidParameters, response.ErrorCode);
        }

        [Test]
        public void UploadServerBuild_WhenFileDoesNotExist_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.UploadServerBuildResponse response = deploymentManager.UploadServerBuild(new Core.DeploymentManagement.Models.UploadServerBuildRequest()
            {
                FilePath = "InvalidFilePath",
                BucketName = "NonEmptyBucketName",
                BuildS3Key = "NonEmptyS3Key"
            });

            fileWrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.AreEqual(ErrorCode.FileNotFound, response.ErrorCode);
        }

        [Test]
        public void UploadServerBuild_WhenAmazonS3ExceptionWasThrown_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true).Verifiable();

            s3WrapperMock.Setup(x => x.PutObject(It.IsAny<PutObjectRequest>())).Throws(
                new AmazonS3Exception("S3Exception", Amazon.Runtime.ErrorType.Sender, "S3Exception", 
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.UploadServerBuildResponse response = deploymentManager.UploadServerBuild(new Core.DeploymentManagement.Models.UploadServerBuildRequest()
            {
                FilePath = "NonEmptyFilePath",
                BucketName = "NonEmptyBucketName",
                BuildS3Key = "NonEmptyS3Key"
            });

            fileWrapperMock.Verify();
            s3WrapperMock.Verify();
            Assert.IsFalse(response.Success);
            Assert.IsNotEmpty(response.ErrorMessage);
        }

        [Test]
        public void UploadServerBuild_WhenValidParametersArePassed_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            fileWrapperMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true).Verifiable();

            s3WrapperMock.Setup(x => x.GetRegionEndpoint()).Returns("eu").Verifiable();
            s3WrapperMock.Setup(x => x.PutObject(It.IsAny<PutObjectRequest>())).Returns(new PutObjectResponse()).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.UploadServerBuildResponse response = deploymentManager.UploadServerBuild(new Core.DeploymentManagement.Models.UploadServerBuildRequest()
            {
                FilePath = "NonEmptyFilePath",
                BucketName = "NonEmptyBucketName",
                BuildS3Key = "NonEmptyS3Key"
            });

            fileWrapperMock.Verify();
            s3WrapperMock.Verify();
            Assert.IsTrue(response.Success);
        }

        [Test]
        public void CancelDeployment_WhenValidStackUpdateIsInProgress_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.CancelDeployment(It.IsAny<CancelUpdateStackRequest>())).Returns(new CancelUpdateStackResponse()).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CancelDeploymentResponse cancelResponse = deploymentManager.CancelDeployment(new Core.DeploymentManagement.Models.CancelDeploymentRequest()
            {
                StackName = "ValidStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsTrue(cancelResponse.Success);
        }

        [Test]
        public void CancelDeployment_WhenValidStackDoesNotExist_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.CancelDeployment(It.IsAny<CancelUpdateStackRequest>())).Throws(
                new AmazonCloudFormationException("StackDoesNotExist", Amazon.Runtime.ErrorType.Sender, "StackDoesNotExist", 
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CancelDeploymentResponse cancelResponse = deploymentManager.CancelDeployment(new Core.DeploymentManagement.Models.CancelDeploymentRequest()
            {
                StackName = "InvalidStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(cancelResponse.Success);
            Assert.AreEqual(cancelResponse.ErrorCode, ErrorCode.AwsError);
            Assert.IsNotEmpty(cancelResponse.ErrorMessage);
        }

        [Test]
        public void CancelDeployment_WhenSKDThrowsTokenAlreadyExistsException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.CancelDeployment(It.IsAny<CancelUpdateStackRequest>())).Throws(
                new TokenAlreadyExistsException("TokenAlreadyExists", Amazon.Runtime.ErrorType.Sender, "TokenAlreadyExists", 
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CancelDeploymentResponse cancelResponse = deploymentManager.CancelDeployment(new Core.DeploymentManagement.Models.CancelDeploymentRequest()
            {
                StackName = "InvalidStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(cancelResponse.Success);
            Assert.AreEqual(cancelResponse.ErrorCode, ErrorCode.TokenAlreadyExists);
            Assert.IsNotEmpty(cancelResponse.ErrorMessage);
        }

        [Test]
        public void CancelDeployment_WhenInValidSParametersArePassed_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.CancelDeploymentResponse cancelResponse = deploymentManager.CancelDeployment(new Core.DeploymentManagement.Models.CancelDeploymentRequest()
            {
                StackName = string.Empty
            });

            Assert.IsFalse(cancelResponse.Success);
            Assert.AreEqual(cancelResponse.ErrorCode, ErrorCode.InvalidParameters);
        }

        [Test]
        public void DeleteChangeSet_WhenValidParametersArePassed_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteChangeSet(It.IsAny<DeleteChangeSetRequest>())).Returns(new DeleteChangeSetResponse()).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteChangeSetResponse deleteResponse = deploymentManager.DeleteChangeSet(new Core.DeploymentManagement.Models.DeleteChangeSetRequest()
            {
                StackName = "ValidStackName",
                ChangeSetName = "ValidChangeSetName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsTrue(deleteResponse.Success);
        }

        [Test]
        public void DeleteChangeSet_WhenStackDoesNotExist_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteChangeSet(It.IsAny<DeleteChangeSetRequest>())).Throws(
                new AmazonCloudFormationException("StackDoesNotExist", Amazon.Runtime.ErrorType.Sender, "StackDoesNotExist",
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteChangeSetResponse deleteResponse = deploymentManager.DeleteChangeSet(new Core.DeploymentManagement.Models.DeleteChangeSetRequest()
            {
                StackName = "NonExistingStack",
                ChangeSetName = "NonExistingChangeSet"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.AwsError);
            Assert.IsNotEmpty(deleteResponse.ErrorMessage);
        }

        [Test]
        public void DeleteChangeSet_WhenSDKThowsInvalidChangeSetStatusException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteChangeSet(It.IsAny<DeleteChangeSetRequest>())).Throws(
                new InvalidChangeSetStatusException("InvalidChangeSetStatus", Amazon.Runtime.ErrorType.Sender, "InvalidChangeSetStatus", 
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteChangeSetResponse deleteResponse = deploymentManager.DeleteChangeSet(new Core.DeploymentManagement.Models.DeleteChangeSetRequest()
            {
                StackName = "NonExistingStack",
                ChangeSetName = "NonExistingChangeSet"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.InvalidChangeSetStatus);
            Assert.IsNotEmpty(deleteResponse.ErrorMessage);
        }

        [Test]
        public void DeleteChangeSet_WhenEmptyParametersArePassed_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteChangeSetResponse deleteResponse = deploymentManager.DeleteChangeSet(new Core.DeploymentManagement.Models.DeleteChangeSetRequest()
            {
                StackName = string.Empty,
                ChangeSetName = string.Empty
            });

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.InvalidParameters);
        }

        [Test]
        public void DeleteStack_WhenValidParametersArePassed_IsSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteStack(It.IsAny<DeleteStackRequest>())).Returns(new DeleteStackResponse()).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteStackResponse deleteResponse = deploymentManager.DeleteStack(new Core.DeploymentManagement.Models.DeleteStackRequest()
            {
                StackName = "ValidStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsTrue(deleteResponse.Success);
        }

        [Test]
        public void DeleteStack_WhenStackDoesNotExist_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteStack(It.IsAny<DeleteStackRequest>())).Throws(
                new AmazonCloudFormationException("StackDoesNotExist", Amazon.Runtime.ErrorType.Sender, "StackDoesNotExist",
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteStackResponse deleteResponse = deploymentManager.DeleteStack(new Core.DeploymentManagement.Models.DeleteStackRequest()
            {
                StackName = "NonExistingStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.AwsError);
            Assert.IsNotEmpty(deleteResponse.ErrorMessage);
        }

        [Test]
        public void DeleteStack_WhenSDKThowsTokenAlreadyExistsException_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            amazonCloudFomrationClientMock.Setup(x => x.DeleteStack(It.IsAny<DeleteStackRequest>())).Throws(
                new TokenAlreadyExistsException("TokenAlreadyExists", Amazon.Runtime.ErrorType.Sender, "TokenAlreadyExists",
                "", System.Net.HttpStatusCode.BadRequest)).Verifiable();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteStackResponse deleteResponse = deploymentManager.DeleteStack(new Core.DeploymentManagement.Models.DeleteStackRequest()
            {
                StackName = "NonExistingStackName"
            });

            amazonCloudFomrationClientMock.Verify();

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.TokenAlreadyExists);
            Assert.IsNotEmpty(deleteResponse.ErrorMessage);
        }

        [Test]
        public void DeleteStack_WhenEmptyParametersArePassed_IsNotSuccessful()
        {
            var amazonCloudFomrationClientMock = new Mock<IAmazonCloudFormationWrapper>();
            var s3WrapperMock = new Mock<IAmazonS3Wrapper>();
            var fileWrapperMock = new Mock<IFileWrapper>();
            var fileZipMock = new Mock<IFileZip>();

            var deploymentManager = new DeploymentManager(
                    amazonCloudFomrationClientMock.Object,
                    s3WrapperMock.Object,
                    fileWrapperMock.Object,
                    fileZipMock.Object
                );

            Core.DeploymentManagement.Models.DeleteStackResponse deleteResponse = deploymentManager.DeleteStack(new Core.DeploymentManagement.Models.DeleteStackRequest()
            {
                StackName = string.Empty
            });

            Assert.IsFalse(deleteResponse.Success);
            Assert.AreEqual(deleteResponse.ErrorCode, ErrorCode.InvalidParameters);
        }
    }
}
