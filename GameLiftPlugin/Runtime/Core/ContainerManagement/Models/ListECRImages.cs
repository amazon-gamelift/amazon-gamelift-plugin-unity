// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.Shared;
using System.Collections.Generic;

namespace AmazonGameLiftPlugin.Core.ContainerManagement.Models
{
    public class ECRImage
    {
        public string ImageTag;
        public string ImageDigest;
    }

    public class ListECRImagesRequest
    {
    }

    public class ListECRImagesResponse : Response
    {
        public IEnumerable<ECRImage> ECRImages { get; set; }
    }
}

