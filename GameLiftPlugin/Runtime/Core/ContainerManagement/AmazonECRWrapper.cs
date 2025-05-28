// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using Amazon.ECR;
using Amazon.ECR.Model;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.ContainerManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Castle.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AmazonGameLiftPlugin.Core.Shared.Logging;

namespace AmazonGameLiftPlugin.Core.ContainerManagement
{
    public class AmazonECRWrapper : IAmazonECRWrapper
    {
        private readonly IAmazonECR _amazonECR;

        public AmazonECRWrapper(string accessKey, string secretKey, string region)
        {
            _amazonECR = new AmazonECRClient(
                    accessKey,
                    secretKey,
                    AwsRegionMapper.GetRegionEndpoint(region)
                );
        }

        public DescribeECRRepositoriesResponse DescribeECRRepositories(List<string> RepositoryNames = null)
        {
            try
            {
                List<Repository> rawRepositories = new List<Repository>();
                string nextToken = null;

                do
                {
                    var request = new DescribeRepositoriesRequest()
                    {
                        RepositoryNames = RepositoryNames.IsNullOrEmpty() ? null : RepositoryNames,
                        NextToken = nextToken
                    };
                    var response = _amazonECR.DescribeRepositories(request);
                    nextToken = response.NextToken;
                    rawRepositories.AddRange(response.Repositories);
                } while (!nextToken.IsNullOrEmpty());

                return Response.Ok(new DescribeECRRepositoriesResponse()
                {
                    ECRRepositories = rawRepositories
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return HandleAwsException(ex, () => new DescribeECRRepositoriesResponse());
            }
        }

        public ListECRImagesResponse ListECRImages(string repositoryName)
        {
            try
            {
                List<ImageIdentifier> rawImageIds = new List<ImageIdentifier>();
                string nextToken = null;

                do
                {
                    var request = new ListImagesRequest() { RepositoryName = repositoryName, NextToken = nextToken };
                    var response = _amazonECR.ListImages(request);
                    nextToken = response.NextToken;
                    rawImageIds.AddRange(response.ImageIds);
                } while (!nextToken.IsNullOrEmpty());

                return Response.Ok(new ListECRImagesResponse()
                {
                    ECRImages = rawImageIds.Select(image => new ECRImage()
                        {
                            ImageTag = image.ImageTag,
                            ImageDigest = image.ImageDigest
                        })
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return HandleAwsException(ex, () => new ListECRImagesResponse());
            }
        }

        public CreateECRRepositoryResponse CreateRepository(string repositoryName)
        {
            var request = new CreateRepositoryRequest() { RepositoryName = repositoryName };
            try
            {
                var response = _amazonECR.CreateRepository(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    return Response.Fail(new CreateECRRepositoryResponse()
                    {
                        ErrorCode = ErrorCode.AwsError,
                        ErrorMessage = $"HTTP Status Code {response.HttpStatusCode}"
                    });
                }
                return Response.Ok(new CreateECRRepositoryResponse()
                {
                    RepositoryUri = response.Repository.RepositoryUri
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return HandleAwsException(ex, () => new CreateECRRepositoryResponse());
            }
        }

        private T HandleAwsException<T>(Exception ex, Func<T> responseObject) where T : Response
        {
            T response = responseObject();

            if (ex is AmazonECRException exception)
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
    }
}
