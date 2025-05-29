// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    /// <summary>
    /// Waits until a stack change is applied and allows to cancel.
    /// </summary>
    public class DeploymentWaiter
    {
        private const int StackPollPeriodMs = 5000;
        private readonly Delay _delay;
        private DeploymentId _currentRequest;
        private string _currentToken;
        private bool _isCreating;
        private bool _isWaiting;

        public virtual bool CanCancel { get; private set; }

        protected CoreApi GameLiftCoreApi { get; }

        public virtual event Action<DeploymentInfo> InfoUpdated;

        public DeploymentWaiter()
        {
            _delay = new Delay();
            GameLiftCoreApi = CoreApi.SharedInstance;
        }

        internal DeploymentWaiter(Delay delay, CoreApi coreApi)
        {
            _delay = delay ?? throw new ArgumentNullException(nameof(delay));
            GameLiftCoreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
        }

        public virtual Response CancelDeployment()
        {
            if (!CanCancel)
            {
                return Response.Fail(new Response() { ErrorCode = ErrorCode.OperationInvalid });
            }

            Response response;

            if (_isCreating)
            {
                response = GameLiftCoreApi.DeleteStack(_currentRequest.Profile,
                    _currentRequest.Region, _currentRequest.StackName);
            }
            else
            {
                response = GameLiftCoreApi.CancelDeployment(_currentRequest.Profile,
                    _currentRequest.Region, _currentRequest.StackName, _currentToken);
            }

            if (response.Success)
            {
                CanCancel = false;
            }

            return response;
        }

        public virtual async Task<DeploymentResponse> WaitUntilDone(DeploymentId deploymentId)
        {
            if (_isWaiting)
            {
                return Response.Fail(new DeploymentResponse(ErrorCode.OperationInvalid));
            }

            _isWaiting = true;
            _currentRequest = deploymentId;

            try
            {
                return await PollStatusUntilDone(deploymentId);
            }
            finally
            {
                CleanUpCurrentDeployment();
            }
        }

        public virtual Response CancelWaiting()
        {
            if (!_isWaiting)
            {
                return Response.Fail(new Response { ErrorCode = ErrorCode.OperationInvalid });
            }

            CleanUpCurrentDeployment();
            return Response.Ok(new Response());
        }

        private async Task<DeploymentResponse> PollStatusUntilDone(DeploymentId deploymentId)
        {
            var poller = new UntilResponseFailurePoller(_delay);

            DescribeStackResponse describeStackResponse = await poller.Poll(StackPollPeriodMs,
                () =>
                {
                    DescribeStackResponse response = GameLiftCoreApi.DescribeStack(deploymentId.Profile, deploymentId.Region, deploymentId.StackName);

                    if (!response.Success)
                    {
                        return response;
                    }

                    if (_currentToken == null
                        && (response.StackStatus == StackStatus.UpdateInProgress || response.StackStatus == StackStatus.CreateInProgress))
                    {
                        CanCancel = true;
                        _currentToken = Guid.NewGuid().ToString();
                        _isCreating = response.StackStatus == StackStatus.CreateInProgress;
                    }

                    InfoUpdated?.Invoke(new DeploymentInfo(deploymentId, response, deploymentId.ScenarioName));
                    return response;
                },
                stopCondition: target => !_isWaiting || target.StackStatus.IsStackStatusOperationDone());

            if (!describeStackResponse.Success)
            {
                return Response.Fail(new DeploymentResponse(describeStackResponse));
            }

            if (!_isWaiting)
            {
                return Response.Fail(new DeploymentResponse(ErrorCode.OperationCancelled));
            }

            if (describeStackResponse.StackStatus != StackStatus.CreateComplete
                && describeStackResponse.StackStatus != StackStatus.UpdateComplete
                && describeStackResponse.StackStatus != StackStatus.UpdateCompleteCleanUpInProgress)
            {
                return Response.Fail(new DeploymentResponse(ErrorCode.StackStatusInvalid, $"The '{deploymentId.StackName}' stack status is {describeStackResponse.StackStatus}"));
            }

            return Response.Ok(new DeploymentResponse(_currentRequest));
        }

        private void CleanUpCurrentDeployment()
        {
            _isWaiting = false;
            _currentToken = null;
            CanCancel = false;
            _isCreating = false;
            _currentRequest = default;
        }
    }
}
