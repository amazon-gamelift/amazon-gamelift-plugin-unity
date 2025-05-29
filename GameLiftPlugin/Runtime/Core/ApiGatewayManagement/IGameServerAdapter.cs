// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.ApiGatewayManagement.Models;

namespace AmazonGameLiftPlugin.Core.ApiGatewayManagement
{
    public interface IGameServerAdapter
    {
        Task<GetGameConnectionResponse> GetGameConnection(GetGameConnectionRequest request);

        Task<StartGameResponse> StartGame(StartGameRequest request);
    }
}
