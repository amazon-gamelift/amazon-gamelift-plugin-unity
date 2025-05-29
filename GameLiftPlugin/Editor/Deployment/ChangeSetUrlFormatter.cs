// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Networking;

namespace AmazonGameLift.Editor
{
    internal sealed class ChangeSetUrlFormatter
    {
        /// <exception cref="ArgumentNullException"></exception>
        public string Format(ConfirmChangesRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string stackIdUrlEncoded = UnityWebRequest.EscapeURL(request.StackId);
            string changeSetIdUrlEncoded = UnityWebRequest.EscapeURL(request.ChangeSetId);
            return string.Format(Urls.AwsCloudFormationChangeSetTemplate, request.Region, stackIdUrlEncoded, changeSetIdUrlEncoded);
        }
    }
}
