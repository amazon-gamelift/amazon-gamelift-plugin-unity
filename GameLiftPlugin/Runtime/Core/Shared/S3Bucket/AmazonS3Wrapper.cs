// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.S3;
using Amazon.S3.Model;

namespace AmazonGameLiftPlugin.Core.Shared.S3Bucket
{
    public class AmazonS3Wrapper : IAmazonS3Wrapper
    {
        private readonly IAmazonS3 _amazonS3Client;

        public AmazonS3Wrapper(IAmazonS3 amazonS3Client)
        {
            _amazonS3Client = amazonS3Client;
        }

        public AmazonS3Wrapper(string accessKey, string secretKey, string region)
        {
            _amazonS3Client = new AmazonS3Client(accessKey, secretKey, AwsRegionMapper.GetRegionEndpoint(region));
        }

        public ListBucketsResponse ListBuckets()
            => _amazonS3Client.ListBuckets();

        public GetBucketLocationResponse GetBucketLocation(GetBucketLocationRequest request)
            => _amazonS3Client.GetBucketLocation(request);

        public PutBucketResponse PutBucket(string bucketName)
            => _amazonS3Client.PutBucket(bucketName);

        public PutBucketEncryptionResponse PutBucketEncryption(PutBucketEncryptionRequest request)
            => _amazonS3Client.PutBucketEncryption(request);

        public PutBucketVersioningResponse PutBucketVersioning(PutBucketVersioningRequest request)
            => _amazonS3Client.PutBucketVersioning(request);

        public PutBucketLoggingResponse PutBucketLogging(PutBucketLoggingRequest request)
            => _amazonS3Client.PutBucketLogging(request);

        public PutACLResponse PutACL(PutACLRequest request)
            => _amazonS3Client.PutACL(request);

        public GetACLResponse GetACL(GetACLRequest request)
            => _amazonS3Client.GetACL(request);

        public PutBucketResponse PutBucket(PutBucketRequest request)
        => _amazonS3Client.PutBucket(request);

        public PutLifecycleConfigurationResponse PutLifecycleConfiguration(PutLifecycleConfigurationRequest request)
            => _amazonS3Client.PutLifecycleConfiguration(request);

        public GetLifecycleConfigurationResponse GetLifecycleConfiguration(string bucketName)
            => _amazonS3Client.GetLifecycleConfiguration(bucketName);

        public PutBucketPolicyResponse PutBucketPolicy(PutBucketPolicyRequest request)
            => _amazonS3Client.PutBucketPolicy(request);

        public bool DoesBucketExist(string bucketName)
            => _amazonS3Client.DoesS3BucketExist(bucketName);

        public PutObjectResponse PutObject(PutObjectRequest request)
            => _amazonS3Client.PutObject(request);

        public string GetRegionEndpoint()
        {
            return _amazonS3Client.Config.RegionEndpoint.SystemName;
        }
    }
}
