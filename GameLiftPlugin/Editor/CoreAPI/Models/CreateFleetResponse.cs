// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class CreateFleetResponse : Response
    {
        public string FleetId { get; set; }
        public string FleetName { get; set; }
    }
}