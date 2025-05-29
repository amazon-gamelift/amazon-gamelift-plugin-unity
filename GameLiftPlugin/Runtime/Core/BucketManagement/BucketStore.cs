// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using Serilog;

namespace AmazonGameLiftPlugin.Core.BucketManagement
{
    public class BucketStore : IBucketStore
    {
        /// <summary>
        /// Regex pattern matches with rule defined in the page https://docs.aws.amazon.com/AmazonS3/latest/userguide/bucketnamingrules.html
        /// Described here https://stackoverflow.com/questions/50480924/regex-for-s3-bucket-name/50484916
        /// </summary>
        private static readonly string s_s3BucketNamePattern = @"(?=^.{3,63}$)(?!^(\d+\.)+\d+$)(^(([a-z0-9]|[a-z0-9][a-z0-9\-]*[a-z0-9])\.)*([a-z0-9]|[a-z0-9][a-z0-9\-]*[a-z0-9])$)";

        private readonly IAmazonS3Wrapper _amazonS3Wrapper;

        public BucketStore(IAmazonS3Wrapper amazonS3Wrapper)
        {
            _amazonS3Wrapper = amazonS3Wrapper;
        }

        public CreateBucketResponse CreateBucket(CreateBucketRequest request)
        {
            ValidationResult validationResult = Validate(request);

            if (!validationResult.IsValid)
            {
                return Response.Fail(new CreateBucketResponse
                {
                    ErrorCode = validationResult.ErrorCode
                });
            }

            try
            {
                // Create bootstrap bucket
                PutBucketResponse putBucketResponse = _amazonS3Wrapper.PutBucket(new PutBucketRequest
                {
                    BucketName = request.BucketName,
                    BucketRegionName = request.Region,
                });

                if (putBucketResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putBucketResponse.HttpStatusCode}"
                    });
                }

                // TODO (#17): Allow users to toggle audit-logging, versioning and encryption on bootstrap bucket

                // Enable bootstrap bucket versioning
                PutBucketVersioningResponse putBucketVersioningRequest = _amazonS3Wrapper.PutBucketVersioning(new PutBucketVersioningRequest
                {
                    BucketName = request.BucketName,
                    VersioningConfig = new S3BucketVersioningConfig
                    {
                        Status = VersionStatus.Enabled
                    }
                });

                if (putBucketVersioningRequest.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putBucketVersioningRequest.HttpStatusCode}"
                    });
                }

                // Enable bootstrap bucket server-side encryption
                PutBucketEncryptionResponse putBucketEncryptionRequest = _amazonS3Wrapper.PutBucketEncryption(new PutBucketEncryptionRequest
                {
                    BucketName = request.BucketName,
                    ServerSideEncryptionConfiguration = new ServerSideEncryptionConfiguration
                    {
                        ServerSideEncryptionRules = new List<ServerSideEncryptionRule>
                        {
                            new ServerSideEncryptionRule
                            {
                                ServerSideEncryptionByDefault = new ServerSideEncryptionByDefault
                                {
                                    ServerSideEncryptionAlgorithm = ServerSideEncryptionMethod.AES256
                                }
                            }
                        }
                    }
                });

                if (putBucketEncryptionRequest.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putBucketEncryptionRequest.HttpStatusCode}"
                    });
                }

                // Create logging bucket for the bootstrap bucket
                string loggingBucketName = request.BucketName + "-log";

                PutBucketResponse putLoggingBucketResponse = _amazonS3Wrapper.PutBucket(new PutBucketRequest
                {
                    BucketName = loggingBucketName,
                    BucketRegionName = request.Region,
                    ObjectOwnership = ObjectOwnership.BucketOwnerPreferred
                });

                if (putLoggingBucketResponse.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putLoggingBucketResponse.HttpStatusCode}"
                    });
                }
                
                var logPrefix = "GameLiftBootstrap";
                var policy = new FileWrapper().ReadAllText(GetBucketPolicyPath());
                var formattedPolicy = policy
                    .Replace("{0}", loggingBucketName)
                    .Replace("{1}", logPrefix)
                    .Replace("{2}", request.BucketName)
                    .Replace("{3}", request.AccountId);
                
                PutBucketPolicyRequest policyRequest = new PutBucketPolicyRequest()
                {
                    BucketName = loggingBucketName,
                    Policy = formattedPolicy
                };
                var putBucketPolicyResponse = _amazonS3Wrapper.PutBucketPolicy(policyRequest);
                if (putBucketPolicyResponse.HttpStatusCode != HttpStatusCode.NoContent)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putBucketPolicyResponse.HttpStatusCode}"
                    });
                }

                // Enable access logging on the bootstrap bucket using the newly created logging bucket
                PutBucketLoggingResponse putBucketLoggingRequest = _amazonS3Wrapper.PutBucketLogging(new PutBucketLoggingRequest
                {
                    BucketName = request.BucketName,
                    LoggingConfig = new S3BucketLoggingConfig
                    {
                        TargetBucketName = loggingBucketName,
                        TargetPrefix = logPrefix,
                    }
                });

                if (putBucketLoggingRequest.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateBucketResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putBucketLoggingRequest.HttpStatusCode}"
                    });
                }

                return Response.Ok(new CreateBucketResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return HandleAwsException(ex, () => new CreateBucketResponse());
            }
        }

        private ValidationResult Validate(CreateBucketRequest request)
        {
            if (!AwsRegionMapper.IsValidRegion(request.Region))
            {
                return ValidationResult.Invalid(ErrorCode.InvalidRegion);
            }

            bool bucketAlreadyExists = _amazonS3Wrapper.DoesBucketExist(request.BucketName);

            if (bucketAlreadyExists)
            {
                return ValidationResult.Invalid(ErrorCode.BucketNameAlreadyExists);
            }

            if (!Regex.Match(request.BucketName, s_s3BucketNamePattern).Success)
            {
                return ValidationResult.Invalid(ErrorCode.BucketNameIsWrong);
            }

            return ValidationResult.Valid();
        }

        public Models.PutLifecycleConfigurationResponse PutLifecycleConfiguration(Models.PutLifecycleConfigurationRequest request)
        {
            if (!Enum.IsDefined(typeof(BucketPolicy), request.BucketPolicy) || request.BucketPolicy == BucketPolicy.None)
            {
                return Response.Fail(new Models.PutLifecycleConfigurationResponse()
                {
                    ErrorCode = ErrorCode.InvalidBucketPolicy
                });
            }

            GetLifecycleConfigurationResponse lifecycleResponse =
                _amazonS3Wrapper.GetLifecycleConfiguration(request.BucketName);
            LifecycleConfiguration lifecycleConfiguration = lifecycleResponse.Configuration;

            if (lifecycleResponse.HttpStatusCode == HttpStatusCode.NotFound)
            {
                lifecycleConfiguration = new LifecycleConfiguration
                {
                    Rules = new List<LifecycleRule>()
                };
            }

            lifecycleConfiguration.Rules.Add(new LifecycleRule
            {
                Id = $"GameLiftBootstrapBucketRule_{Guid.NewGuid()}",
                Filter = new LifecycleFilter(),
                Expiration = new LifecycleRuleExpiration
                {
                    Days = (int)request.BucketPolicy
                },
                Status = new LifecycleRuleStatus("Enabled")
            });

            try
            {
                Amazon.S3.Model.PutLifecycleConfigurationResponse putLifecycleConfigurationResponse =
                _amazonS3Wrapper.PutLifecycleConfiguration(new Amazon.S3.Model.PutLifecycleConfigurationRequest
                {
                    BucketName = request.BucketName,
                    Configuration = lifecycleConfiguration
                });

                if (putLifecycleConfigurationResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    return Response.Ok(new Models.PutLifecycleConfigurationResponse());
                }
                else
                {
                    return Response.Fail(new Models.PutLifecycleConfigurationResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {putLifecycleConfigurationResponse.HttpStatusCode}"
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return HandleAwsException(ex, () => new Models.PutLifecycleConfigurationResponse());
            }
        }

        public GetBucketsResponse GetBuckets(GetBucketsRequest request)
        {
            try
            {
                ListBucketsResponse response = _amazonS3Wrapper.ListBuckets();

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    var buckets = new List<string>();

                    if (!string.IsNullOrEmpty(request.Region))
                    {
                        foreach (string bucketName in response.Buckets.Select(bucket => bucket.BucketName))
                        {
                            GetBucketLocationResponse locationResponse = _amazonS3Wrapper.GetBucketLocation(new GetBucketLocationRequest
                            {
                                BucketName = bucketName
                            });

                            if (locationResponse.HttpStatusCode == HttpStatusCode.OK && ToBucketRegion(locationResponse) == request.Region)
                            {
                                buckets.Add(bucketName);
                            }
                        }
                    }
                    else
                    {
                        buckets = response.Buckets.Select(bucket => bucket.BucketName).ToList();
                    }

                    return Response.Ok(new GetBucketsResponse()
                    {
                        Buckets = buckets
                    });
                }
                else
                {
                    return Response.Fail(new GetBucketsResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {response.HttpStatusCode}"
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return HandleAwsException(ex, () => new GetBucketsResponse());
            }
        }
        private string ToBucketRegion(GetBucketLocationResponse getBucketLocationResponse)
        {
            string locationValue = getBucketLocationResponse.Location.Value;
            // See: https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/TS3Region.html
            switch (locationValue)
            {
                case "":
                    return "us-east-1";
                case "EU":
                    return "eu-west-1";
                default:
                    return locationValue;
            }
        }

        public GetAvailableRegionsResponse GetAvailableRegions(GetAvailableRegionsRequest request)
        {
            return Response.Ok(new GetAvailableRegionsResponse
            {
                Regions = AwsRegionMapper.AvailableRegions()
            });
        }

        public GetBucketPoliciesResponse GetBucketPolicies(GetBucketPoliciesRequest request)
        {
            return Response.Ok(new GetBucketPoliciesResponse
            {
                Policies = (IEnumerable<BucketPolicy>)Enum.GetValues(typeof(BucketPolicy))
            });
        }

        private T HandleAwsException<T>(Exception ex, Func<T> responseObject) where T : Response
        {
            T response = responseObject();

            if (ex is AmazonS3Exception exception)
            {
                response.ErrorCode = ErrorCode.AwsError;
                response.ErrorMessage = exception.Message;
            }
            else if (ex is WebException
                || ex is ArgumentNullException)
            {
                response.ErrorCode = ErrorCode.AwsError;
                response.ErrorMessage = ex.Message;
            }
            else
            {
                throw ex;
            }

            return Response.Fail(response);
        }
        
        public virtual string GetBucketPolicyPath()
        {
            string packageName = "com.amazonaws.gamelift";
            string policyPath = "Runtime/Core/BucketManagement/BucketAccessPolicy.json";
            string internalPath = $"{packageName}/{policyPath}";
            string packagePath = $"Packages/{internalPath}";
            string fullPath = Path.GetFullPath(packagePath);

            return fullPath;
        }

    }
}
