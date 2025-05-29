// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLiftPlugin.Core.UserIdentityManagement.Models
{
    public class SignUpRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
    }

    public class SignUpResponse : Response
    {

    }
}
