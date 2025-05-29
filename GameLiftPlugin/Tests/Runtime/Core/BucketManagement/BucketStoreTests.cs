// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using AmazonGameLiftPlugin.Core.BucketManagement;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using AmazonGameLiftPlugin.Core.Tests.Factories;
using Moq;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Core.Tests.BucketManagement
{
    [TestFixture]
    public class BucketStoreTests
    {
        private BucketStoreFactory Factory { get; set; }

        [SetUp]
        public void Init()
        {
            Factory = new BucketStoreFactory();
        }

        [Test]
        public void CreateBucket_WhenCorrectBucketNameIsPassed_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(
                    x => x.PutBucket(It.IsAny<PutBucketRequest>()))
                .Returns(new PutBucketResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            amazonS3WrapperMock.Setup(
                    x => x.PutBucketVersioning(It.IsAny<PutBucketVersioningRequest>()))
                .Returns(new PutBucketVersioningResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            amazonS3WrapperMock.Setup(
                    x => x.PutBucketEncryption(It.IsAny<PutBucketEncryptionRequest>()))
                .Returns(new PutBucketEncryptionResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();
            amazonS3WrapperMock.Setup(
                    x => x.GetACL(It.IsAny<GetACLRequest>()))
                .Returns(new GetACLResponse()
                {
                    AccessControlList = new S3AccessControlList(),
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();
            amazonS3WrapperMock.Setup(
                    x => x.PutACL(It.IsAny<PutACLRequest>()))
                .Returns(new PutACLResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();
            amazonS3WrapperMock.Setup(
                    x => x.PutBucketLogging(It.IsAny<PutBucketLoggingRequest>()))
                .Returns(new PutBucketLoggingResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";

            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = validRegion,
                Region = validRegion
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsTrue(createBucketResponse.Success);
        }

        [Test]
        public void CreateBucket_WhenEmptyBucketNameIsPassed_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";

            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = string.Empty,
                Region = validRegion
            });

            Assert.IsFalse(createBucketResponse.Success);
            Assert.IsNotEmpty(createBucketResponse.ErrorCode);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.BucketNameIsWrong);
        }

        [Test]
        public void CreateBucket_WhenAWSReturnsError_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(
                x => x.PutBucket(It.IsAny<PutBucketRequest>()))
                .Returns(new PutBucketResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.Unauthorized
                }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";
            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = $"non-empty-name{Guid.NewGuid()}",
                Region = validRegion
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(createBucketResponse.Success);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.AwsError);
        }

        [Test]
        public void CreateBucket_WhenAWSSDKThrowsAmazonS3Exception_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            const string errorCodeFromAWS = "BucketAlreadyOwnedByYou";
            const string errorMessageFromAWS = "NonEmptyMessage";

            amazonS3WrapperMock.Setup(
                x => x.PutBucket(It.IsAny<PutBucketRequest>()))
                .Throws(new AmazonS3Exception(errorMessageFromAWS)
                {
                    ErrorCode = errorCodeFromAWS
                }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";
            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = $"non-empty-name{Guid.NewGuid()}",
                Region = validRegion
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(createBucketResponse.Success);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(createBucketResponse.ErrorMessage, errorMessageFromAWS);
        }

        [Test]
        public void CreateBucket_WhenAWSSDKThrowsWebException_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string exceptionMessage = "NonEmptyMessage";

            amazonS3WrapperMock.Setup(
                x => x.PutBucket(It.IsAny<PutBucketRequest>()))
                .Throws(new WebException(exceptionMessage)).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";
            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = $"non-empty-name{Guid.NewGuid()}",
                Region = validRegion
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(createBucketResponse.Success);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(createBucketResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void CreateBucket_WhenAWSSDKThrowsArgumentNullException_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string exceptionMessage = "Value cannot be null.";

            amazonS3WrapperMock.Setup(
                x => x.PutBucket(It.IsAny<PutBucketRequest>()))
                .Throws(new ArgumentNullException()).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-2";
            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = $"non-empty-name{Guid.NewGuid()}",
                Region = validRegion
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(createBucketResponse.Success);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(createBucketResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void GetBucketsList_WhenBucketsExist_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.ListBuckets()).Returns(new ListBucketsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Buckets = new System.Collections.Generic.List<S3Bucket>()
                {
                    new S3Bucket()
                    {
                        BucketName = $"non-empty-name{Guid.NewGuid()}",
                        CreationDate = DateTime.Now
                    }
                }
            }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse createBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest());

            amazonS3WrapperMock.VerifyAll();
            Assert.IsTrue(createBucketResponse.Success);
            Assert.IsTrue(createBucketResponse.Buckets.Any());
        }

        [Test]
        public void GetBucketsList_WhenAWSReturnsError_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.ListBuckets()).Returns(new ListBucketsResponse() { HttpStatusCode = System.Net.HttpStatusCode.Unauthorized }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse getBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest());

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(getBucketResponse.Success);
        }

        [Test]
        public void GetBucketsList_WhenAWSSDKThrowsAmazonS3Exception_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string errorCodeFromAWS = "NonEmptyMessage";
            amazonS3WrapperMock.Setup(x => x.ListBuckets())
                .Throws(new AmazonS3Exception("NonEmptyMessage")
                {
                    ErrorCode = errorCodeFromAWS
                }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse getBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest());

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(getBucketResponse.Success);
            Assert.AreEqual(getBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(getBucketResponse.ErrorMessage, errorCodeFromAWS);

        }

        [Test]
        public void GetBucketsList_WhenAWSSDKThrowsArgumentNullException_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string exceptionMessage = "Value cannot be null.";

            amazonS3WrapperMock.Setup(x => x.ListBuckets())
                .Throws(new ArgumentNullException()).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse getBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest());

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(getBucketResponse.Success);
            Assert.AreEqual(getBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(getBucketResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void GetBucketsList_WhenAWSSDKThrowsWebException_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string exceptionMessage = "NonEmptyMessage";

            amazonS3WrapperMock.Setup(x => x.ListBuckets())
                .Throws(new WebException(exceptionMessage)).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse getBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest());

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(getBucketResponse.Success);
            Assert.AreEqual(getBucketResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(getBucketResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void GetBucketsList_WhenRegionIsPassed_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.ListBuckets()).Returns(new ListBucketsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Buckets = new System.Collections.Generic.List<S3Bucket>()
                {
                    new S3Bucket()
                    {
                        BucketName = $"non-empty-name{Guid.NewGuid()}",
                        CreationDate = DateTime.Now
                    }
                }
            }).Verifiable();

            amazonS3WrapperMock.Setup(x => x.GetBucketLocation(It.IsAny<GetBucketLocationRequest>())).Returns(new GetBucketLocationResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                Location = "us-west-1"
            }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse createBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest()
            {
                Region = "us-west-1"
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsTrue(createBucketResponse.Success);
            Assert.IsTrue(createBucketResponse.Buckets.Any());
        }

        [Test]
        public void GetBucketsList_WhenBucketsDoNotExist_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.ListBuckets()).Returns(new ListBucketsResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketsResponse createBucketResponse = bucketStore.GetBuckets(new GetBucketsRequest()
            {

            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsTrue(createBucketResponse.Success);
        }

        [Test]
        public void CreateBucket_WhenBucketNameAlreadyExists_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            string bucketName = "existing-bucket";

            amazonS3WrapperMock.Setup(
                    x => x.DoesBucketExist(bucketName)
                ).Returns(true);

            IBucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            string validRegion = "us-west-1";
            CreateBucketResponse createBucketResponse = bucketStore.CreateBucket(new CreateBucketRequest()
            {
                BucketName = bucketName,
                Region = validRegion
            });

            Assert.IsFalse(createBucketResponse.Success);
            Assert.IsNotEmpty(createBucketResponse.ErrorCode);
            Assert.AreEqual(createBucketResponse.ErrorCode, ErrorCode.BucketNameAlreadyExists);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenBucketExists_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.GetLifecycleConfiguration(It.IsAny<string>()))
                .Returns(new GetLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            amazonS3WrapperMock.Setup(x => x.PutLifecycleConfiguration(It.IsAny<Amazon.S3.Model.PutLifecycleConfigurationRequest>()))
                .Returns(new Amazon.S3.Model.PutLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable(); ;


            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = BucketPolicy.SevenDaysLifecycle
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsTrue(putLifecycleConfigurationResponse.Success);
            Assert.Null(putLifecycleConfigurationResponse.ErrorCode);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenAwsReturnsError_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.GetLifecycleConfiguration(It.IsAny<string>()))
                .Returns(new GetLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            amazonS3WrapperMock.Setup(x => x.PutLifecycleConfiguration(It.IsAny<Amazon.S3.Model.PutLifecycleConfigurationRequest>()))
                .Returns(new Amazon.S3.Model.PutLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.Unauthorized
                }).Verifiable(); ;


            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = BucketPolicy.SevenDaysLifecycle
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(putLifecycleConfigurationResponse.Success);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorCode, ErrorCode.AwsError);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenAWSSDKThrowsAmazonS3Exception_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.GetLifecycleConfiguration(It.IsAny<string>()))
                .Returns(new GetLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            string errorCodeFromAWS = "NonEmptyMessage";
            amazonS3WrapperMock.Setup(x => x.PutLifecycleConfiguration(It.IsAny<Amazon.S3.Model.PutLifecycleConfigurationRequest>()))
                .Throws(new AmazonS3Exception("NonEmptyMessage")
                {
                    ErrorCode = errorCodeFromAWS
                }).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = BucketPolicy.SevenDaysLifecycle
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(putLifecycleConfigurationResponse.Success);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorMessage, errorCodeFromAWS);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenAWSSDKThrowsWebException_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.GetLifecycleConfiguration(It.IsAny<string>()))
                .Returns(new GetLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            string exceptionMessage = "NonEmptyMessage";
            amazonS3WrapperMock.Setup(x => x.PutLifecycleConfiguration(It.IsAny<Amazon.S3.Model.PutLifecycleConfigurationRequest>()))
                .Throws(new WebException(exceptionMessage)).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = BucketPolicy.SevenDaysLifecycle
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(putLifecycleConfigurationResponse.Success);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenAWSSDKThrowsArgumentNullException_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            amazonS3WrapperMock.Setup(x => x.GetLifecycleConfiguration(It.IsAny<string>()))
                .Returns(new GetLifecycleConfigurationResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            string exceptionMessage = "Value cannot be null.";
            amazonS3WrapperMock.Setup(x => x.PutLifecycleConfiguration(It.IsAny<Amazon.S3.Model.PutLifecycleConfigurationRequest>()))
                .Throws(new ArgumentNullException()).Verifiable();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = BucketPolicy.SevenDaysLifecycle
            });

            amazonS3WrapperMock.VerifyAll();
            Assert.IsFalse(putLifecycleConfigurationResponse.Success);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorCode, ErrorCode.AwsError);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorMessage, exceptionMessage);
        }

        [Test]
        public void PutLifecycleConfiguration_WhenInvalidBucketPolicyPassed_IsNotSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            int invalidBucketPolicy = -12;

            Core.BucketManagement.Models.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse = bucketStore.PutLifecycleConfiguration(new Core.BucketManagement.Models.PutLifecycleConfigurationRequest()
            {
                BucketName = "ValidBucketName",
                BucketPolicy = (BucketPolicy)invalidBucketPolicy
            }); ;

            Assert.IsFalse(putLifecycleConfigurationResponse.Success);
            Assert.NotNull(putLifecycleConfigurationResponse.ErrorCode);
            Assert.AreEqual(putLifecycleConfigurationResponse.ErrorCode, ErrorCode.InvalidBucketPolicy);
        }

        [Test]
        public void GetAvailableRegions_WhenRegionsReturned_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetAvailableRegionsResponse createBucketResponse = bucketStore.GetAvailableRegions(new GetAvailableRegionsRequest());

            Assert.IsTrue(createBucketResponse.Success);

            Assert.NotNull(createBucketResponse.Regions);
        }

        [Test]
        public void GetBucketPolicies_WhenPoliciesReturned_IsSuccessful()
        {
            var amazonS3WrapperMock = new Mock<IAmazonS3Wrapper>();

            BucketStore bucketStore = Factory.CreateBucketStore(amazonS3WrapperMock.Object);

            GetBucketPoliciesResponse getBucketPoliciesResponse = bucketStore.GetBucketPolicies(new GetBucketPoliciesRequest());

            Assert.IsTrue(getBucketPoliciesResponse.Success);

            Assert.NotNull(getBucketPoliciesResponse.Policies);
        }
    }
}
