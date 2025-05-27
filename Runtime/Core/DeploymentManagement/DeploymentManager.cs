// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.S3;
using Amazon.S3.Model;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.FileZip;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.S3Bucket;
using Newtonsoft.Json;
using CreateChangeSetRequest = AmazonGameLiftPlugin.Core.DeploymentManagement.Models.CreateChangeSetRequest;
using CreateChangeSetResponse = AmazonGameLiftPlugin.Core.DeploymentManagement.Models.CreateChangeSetResponse;
using ExecuteChangeSetRequest = AmazonGameLiftPlugin.Core.DeploymentManagement.Models.ExecuteChangeSetRequest;
using ExecuteChangeSetResponse = AmazonGameLiftPlugin.Core.DeploymentManagement.Models.ExecuteChangeSetResponse;

namespace AmazonGameLiftPlugin.Core.DeploymentManagement
{
    public class DeploymentManager : IDeploymentManager
    {
        private static readonly string s_gameNameParameter = "GameNameParameter";
        private static readonly string s_lambdaZipS3KeyParameter = "LambdaZipS3KeyParameter";
        private static readonly string s_lambdaZipS3BucketParameter = "LambdaZipS3BucketParameter";

        private static readonly string s_buildS3BucketParameter = "BuildS3BucketParameter";
        private static readonly string s_buildS3KeyParameter = "BuildS3KeyParameter";

        private static readonly DeploymentFormatter s_formatter = new DeploymentFormatter();

        private readonly IAmazonCloudFormationWrapper _amazonCloudFormation;
        private readonly IAmazonS3Wrapper _amazonS3Wrapper;
        private readonly IFileWrapper _fileWrapper;
        private readonly IFileZip _fileZip;

        public DeploymentManager(
            IAmazonCloudFormationWrapper amazonCloudFormation,
            IAmazonS3Wrapper amazonS3Wrapper,
            IFileWrapper fileWrapper,
            IFileZip fileZip)
        {
            _amazonCloudFormation = amazonCloudFormation;
            _amazonS3Wrapper = amazonS3Wrapper;
            _fileWrapper = fileWrapper;
            _fileZip = fileZip;
        }

        private (bool success, string errorMessage, List<Parameter> parameters) DeserializeParameters(string parametersFilePath)
        {
            try
            {
                List<Parameter> result = JsonConvert.DeserializeObject<List<Parameter>>(_fileWrapper.ReadAllText(
                        parametersFilePath
                    ));

                return (true, string.Empty, result);
            }
            catch (JsonException ex)
            {
                return (false, ex.Message, null);
            }
        }

        private void InjectBuildParameters(
            List<Parameter> parameters,
            string buildS3Bucket,
            string buildS3Key)
        {
            if (string.IsNullOrEmpty(buildS3Bucket) || string.IsNullOrEmpty(buildS3Key))
            {
                return;
            }

            parameters.Add(new Parameter
            {
                ParameterKey = s_buildS3BucketParameter,
                ParameterValue = buildS3Bucket
            });

            parameters.Add(new Parameter
            {
                ParameterKey = s_buildS3KeyParameter,
                ParameterValue = buildS3Key
            });
        }

        public ValidateCfnTemplateResponse ValidateCfnTemplate(ValidateCfnTemplateRequest request)
        {
            if (!_fileWrapper.FileExists(request.TemplateFilePath))
            {
                return Response.Fail(new ValidateCfnTemplateResponse
                {
                    ErrorCode = ErrorCode.TemplateFileNotFound
                });
            }

            try
            {
                ValidateTemplateResponse validateTemplateResponse = _amazonCloudFormation.ValidateTemplate(new ValidateTemplateRequest
                {
                    TemplateBody = _fileWrapper.ReadAllText(request.TemplateFilePath),
                });

                return Response.Ok(new ValidateCfnTemplateResponse());
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ValidateCfnTemplateResponse
                {
                    ErrorCode = ErrorCode.InvalidCfnTemplate,
                    ErrorMessage = ex.Message
                });
            }
        }

        public DescribeStackResponse DescribeStack(DescribeStackRequest request)
        {
            try
            {
                DescribeStacksResponse describeStacksResponse = _amazonCloudFormation.DescribeStacks(new DescribeStacksRequest()
                {
                    StackName = request.StackName
                });

                if (!describeStacksResponse.Stacks.Any())
                {
                    return Response.Fail(new DescribeStackResponse()
                    {
                        ErrorCode = ErrorCode.StackDoesNotExist
                    });
                }

                Stack stack = describeStacksResponse.Stacks[0];
                
                string gameName = stack.Parameters.Find(target => target.ParameterKey == s_gameNameParameter)?.ParameterValue;

                var response = new DescribeStackResponse
                {
                    StackId = stack.StackId,
                    LastUpdatedTime = stack.LastUpdatedTime,
                    StackStatus = stack.StackStatus,
                    GameName = gameName
                };

                if (request.OutputKeys != null)
                {
                    var outputs =
                        stack.Outputs.Where(x => request.OutputKeys.Contains(x.OutputKey))
                        .ToDictionary(key => key.OutputKey, value => value.OutputValue);

                    response.Outputs = outputs;
                }

                return Response.Ok(response);
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new DescribeStackResponse()
                {
                    ErrorCode = ErrorCode.StackDoesNotExist,
                    ErrorMessage = ex.Message
                });
            }
        }

        public Models.DescribeStackResourceResponse DescribeStackResource(Models.DescribeStackResourceRequest request)
        {
            try
            {
                Amazon.CloudFormation.Model.DescribeStackResourceResponse describeStackResourceResponse = 
                    _amazonCloudFormation.DescribeStackResource(new Amazon.CloudFormation.Model.DescribeStackResourceRequest()
                {
                    LogicalResourceId = request.LogicalResourceId,
                    StackName = request.StackName
                });

                StackResourceDetail stackResourceDetail = describeStackResourceResponse.StackResourceDetail;

                var response = new Models.DescribeStackResourceResponse
                {
                    StackId = stackResourceDetail.StackId,
                    PhysicalResourceId = stackResourceDetail.PhysicalResourceId
                };

                return Response.Ok(response);
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return null;
            }
        }

        public CreateChangeSetResponse CreateChangeSet(CreateChangeSetRequest request)
        {
            try
            {
                if (!_fileWrapper.FileExists(request.ParametersFilePath))
                {
                    return Response.Fail(new CreateChangeSetResponse
                    {
                        ErrorCode = ErrorCode.ParametersFileNotFound
                    });
                }

                (bool success, string errorMessage, List<Parameter> parameters) deserializedParameters
                    = DeserializeParameters(request.ParametersFilePath);

                if (!deserializedParameters.success)
                {
                    return Response.Fail(new CreateChangeSetResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters,
                        ErrorMessage = deserializedParameters.errorMessage
                    });
                }

                bool exists = _amazonS3Wrapper.DoesBucketExist(request.BootstrapBucketName);
                if (!exists)
                {
                    return Response.Fail(new CreateChangeSetResponse
                    {
                        ErrorCode = ErrorCode.BucketDoesNotExist
                    });
                }

                string fileName = Path.GetFileName(request.TemplateFilePath);
                string key = s_formatter.GetCloudFormationFileKey(fileName);

                (bool success, string fileUrl, _)
                    = UploadFile(request.BootstrapBucketName, request.TemplateFilePath, key);

                if (!success)
                {
                    return Response.Fail(new CreateChangeSetResponse
                    {
                        ErrorCode = ErrorCode.AwsError
                    });
                }

                InjectLambdaParameters(
                            deserializedParameters.parameters,
                            request.BootstrapBucketName,
                            request.GameName,
                            request.LambdaSourcePath
                        );

                InjectBuildParameters(
                        deserializedParameters.parameters,
                        request.BootstrapBucketName,
                        request.BuildS3Key
                    );

                StackExistsResponse stackExistsResponse = StackExists(new StackExistsRequest
                {
                    StackName = request.StackName
                });

                if (!stackExistsResponse.Success)
                {
                    return Response.Fail(new CreateChangeSetResponse
                    {
                        ErrorCode = stackExistsResponse.ErrorCode
                    });
                }

                ChangeSetType changeSetType =
                    stackExistsResponse.Exists ? ChangeSetType.UPDATE : ChangeSetType.CREATE;

                string changeSetName = s_formatter.GetChangeSetName();

                Amazon.CloudFormation.Model.CreateChangeSetResponse createChangeSetResponse =
                    _amazonCloudFormation.CreateChangeSet(new Amazon.CloudFormation.Model.CreateChangeSetRequest()
                    {
                        ChangeSetName = changeSetName,
                        StackName = request.StackName,
                        Parameters = deserializedParameters.parameters,
                        TemplateURL = fileUrl,
                        Capabilities = new List<string> { "CAPABILITY_NAMED_IAM" },
                        ChangeSetType = changeSetType
                    });

                return Response.Ok(new CreateChangeSetResponse
                {
                    CreatedChangeSetName = changeSetName
                });
            }
            catch (AlreadyExistsException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.ResourceWithTheNameRequestetAlreadyExists,
                    ErrorMessage = ex.Message
                });
            }
            catch (InsufficientCapabilitiesException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.InsufficientCapabilities,
                    ErrorMessage = ex.Message
                });
            }
            catch (LimitExceededException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.LimitExceeded,
                    ErrorMessage = ex.Message
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.Message
                });
            }
            catch (AmazonS3Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CreateChangeSetResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public Models.DescribeChangeSetResponse DescribeChangeSet(Models.DescribeChangeSetRequest request)
        {
            try
            {
                Amazon.CloudFormation.Model.DescribeChangeSetResponse describeChangeSetResponse =
                _amazonCloudFormation.DescribeChangeSet(new Amazon.CloudFormation.Model.DescribeChangeSetRequest
                {
                    ChangeSetName = request.ChangeSetName,
                    StackName = request.StackName
                });

                var response = new Models.DescribeChangeSetResponse
                {
                    StackId = describeChangeSetResponse.StackId,
                    ChangeSetId = describeChangeSetResponse.ChangeSetId,
                    ExecutionStatus = describeChangeSetResponse.ExecutionStatus?.Value
                };

                if (describeChangeSetResponse.Changes != null)
                {
                    response.Changes = describeChangeSetResponse
                        .Changes
                        .Select(x => new Models.Change
                        {
                            Action = x.ResourceChange?.Action?.Value,
                            LogicalId = x.ResourceChange?.LogicalResourceId,
                            Module = x.ResourceChange?.ModuleInfo?.LogicalIdHierarchy,
                            PhysicalId = x.ResourceChange?.PhysicalResourceId,
                            Replacement = x.ResourceChange?.Replacement?.Value,
                            ResourceType = x.ResourceChange?.ResourceType,
                        }).ToList();
                }

                return Response.Ok(response);
            }
            catch (ChangeSetNotFoundException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DescribeChangeSetResponse
                {
                    ErrorCode = ErrorCode.ChangeSetNotFound,
                    ErrorMessage = ex.Message
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DescribeChangeSetResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public ExecuteChangeSetResponse ExecuteChangeSet(ExecuteChangeSetRequest request)
        {
            try
            {
                Amazon.CloudFormation.Model.ExecuteChangeSetResponse executeChangeSetResponse =
                _amazonCloudFormation.ExecuteChangeSet(new Amazon.CloudFormation.Model.ExecuteChangeSetRequest
                {
                    StackName = request.StackName,
                    ChangeSetName = request.ChangeSetName
                });

                return Response.Ok(new ExecuteChangeSetResponse());
            }
            catch (TokenAlreadyExistsException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ExecuteChangeSetResponse
                {
                    ErrorCode = ErrorCode.TokenAlreadyExists,
                    ErrorMessage = ex.Message
                });
            }
            catch (InvalidChangeSetStatusException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ExecuteChangeSetResponse
                {
                    ErrorCode = ErrorCode.InvalidChangeSetStatus,
                    ErrorMessage = ex.Message
                });
            }
            catch (InsufficientCapabilitiesException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ExecuteChangeSetResponse
                {
                    ErrorCode = ErrorCode.InsufficientCapabilities,
                    ErrorMessage = ex.Message
                });
            }
            catch (ChangeSetNotFoundException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ExecuteChangeSetResponse
                {
                    ErrorCode = ErrorCode.ChangeSetNotFound,
                    ErrorMessage = ex.Message
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new ExecuteChangeSetResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public StackExistsResponse StackExists(StackExistsRequest request)
        {
            if (string.IsNullOrEmpty(request.StackName))
            {
                return Response.Fail(new StackExistsResponse
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            return Response.Ok(new StackExistsResponse
            {
                Exists = DescribeStack(new DescribeStackRequest
                {
                    StackName = request.StackName
                }).Success
            });
        }

        public UploadServerBuildResponse UploadServerBuild(UploadServerBuildRequest request)
        {
            if (string.IsNullOrEmpty(request.BucketName) || string.IsNullOrEmpty(request.BuildS3Key) || string.IsNullOrEmpty(request.FilePath))
            {
                return Response.Fail(new UploadServerBuildResponse
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            if (!_fileWrapper.FileExists(request.FilePath))
            {
                return Response.Fail(new UploadServerBuildResponse
                {
                    ErrorCode = ErrorCode.FileNotFound
                });
            }

            (bool success, string fileUrl, string uploadErrorMessage) uploadResult = UploadFile(request.BucketName, request.FilePath, request.BuildS3Key);

            if (uploadResult.success)
            {
                return Response.Ok(new UploadServerBuildResponse());
            }

            return Response.Fail(new UploadServerBuildResponse
            {
                ErrorMessage = uploadResult.uploadErrorMessage
            });
        }

        public CancelDeploymentResponse CancelDeployment(CancelDeploymentRequest request)
        {
            if (string.IsNullOrEmpty(request.StackName))
            {
                return Response.Fail(new CancelDeploymentResponse()
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            try
            {
                _amazonCloudFormation.CancelDeployment(new CancelUpdateStackRequest
                {
                    StackName = request.StackName,
                    ClientRequestToken = request.ClientRequestToken ?? Guid.NewGuid().ToString()
                });

                return Response.Ok(new CancelDeploymentResponse());
            }
            catch (TokenAlreadyExistsException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CancelDeploymentResponse
                {
                    ErrorCode = ErrorCode.TokenAlreadyExists,
                    ErrorMessage = ex.ErrorCode
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CancelDeploymentResponse
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.ErrorCode
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new CancelDeploymentResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public Models.DeleteChangeSetResponse DeleteChangeSet(Models.DeleteChangeSetRequest request)
        {
            if (string.IsNullOrEmpty(request.ChangeSetName) || string.IsNullOrEmpty(request.StackName))
            {
                return Response.Fail(new Models.DeleteChangeSetResponse()
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            try
            {
                _amazonCloudFormation.DeleteChangeSet(new Amazon.CloudFormation.Model.DeleteChangeSetRequest
                {
                    ChangeSetName = request.ChangeSetName,
                    StackName = request.StackName
                });

                return Response.Ok(new Models.DeleteChangeSetResponse());
            }
            catch (InvalidChangeSetStatusException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DeleteChangeSetResponse()
                {
                    ErrorCode = ErrorCode.InvalidChangeSetStatus,
                    ErrorMessage = ex.ErrorCode
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DeleteChangeSetResponse()
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.ErrorCode
                });
            }
        }

        public Models.DeleteStackResponse DeleteStack(Models.DeleteStackRequest request)
        {
            if (string.IsNullOrEmpty(request.StackName))
            {
                return Response.Fail(new Models.DeleteStackResponse()
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            try
            {
                _amazonCloudFormation.DeleteStack(new Amazon.CloudFormation.Model.DeleteStackRequest
                {
                    StackName = request.StackName
                });

                return Response.Ok(new Models.DeleteStackResponse());
            }
            catch (TokenAlreadyExistsException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DeleteStackResponse()
                {
                    ErrorCode = ErrorCode.TokenAlreadyExists,
                    ErrorMessage = ex.ErrorCode
                });
            }
            catch (AmazonCloudFormationException ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new Models.DeleteStackResponse()
                {
                    ErrorCode = ErrorCode.AwsError,
                    ErrorMessage = ex.ErrorCode
                });
            }
        }

        private (bool success, string fileUrl, string uploadErrorMessage) UploadFile(string bucketName, string filePath, string key)
        {
            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    Key = key
                };

                PutObjectResponse putObjectResponse = _amazonS3Wrapper.PutObject(putObjectRequest);

                string region = _amazonS3Wrapper.GetRegionEndpoint();
                string s3Url = s_formatter.GetS3Url(bucketName, region, key);

                return (true, s3Url, string.Empty);
            }
            catch (AmazonS3Exception ex)
            {
                return (false, string.Empty, ex.ErrorCode);
            }
        }

        private void InjectLambdaParameters(
                List<Parameter> parameters,
                string bootstrapBucketName,
                string gameName,
                string lambdaSourcePath
            )
        {
            if (string.IsNullOrEmpty(lambdaSourcePath))
            {
                return;
            }

            string s3LambdaKey = UploadLambda(
                bootstrapBucketName,
                gameName,
                lambdaSourcePath
            );

            parameters.Add(new Parameter
            {
                ParameterKey = s_lambdaZipS3BucketParameter,
                ParameterValue = bootstrapBucketName
            });

            parameters.Add(new Parameter
            {
                ParameterKey = s_lambdaZipS3KeyParameter,
                ParameterValue = s3LambdaKey
            });
        }

        private string UploadLambda(
            string bootstrapBucketName,
            string gameName,
            string lambdaSourcePath)
        {
            string zipFilePath = _fileWrapper.GetUniqueTempFilePath();
            _fileZip.Zip(lambdaSourcePath, zipFilePath);

            string lambdaS3Key = s_formatter.GetLambdaS3Key(gameName);

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bootstrapBucketName,
                FilePath = zipFilePath,
                Key = lambdaS3Key
            };

            PutObjectResponse putObjectResponse =
                _amazonS3Wrapper.PutObject(putObjectRequest);

            if (_fileWrapper.FileExists(zipFilePath))
            {
                _fileWrapper.Delete(zipFilePath);
            }

            return lambdaS3Key;
        }
    }
}
