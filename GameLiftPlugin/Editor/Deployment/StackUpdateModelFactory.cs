// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace AmazonGameLift.Editor
{
    internal sealed class StackUpdateModelFactory
    {
        private readonly ChangeSetUrlFormatter _urlFormatter;

        /// <exception cref="ArgumentNullException"></exception>
        public StackUpdateModelFactory(ChangeSetUrlFormatter urlFormatter) =>
            _urlFormatter = urlFormatter ?? throw new ArgumentNullException(nameof(urlFormatter));

        /// <exception cref="ArgumentNullException"></exception>
        public StackUpdateModel Create(ConfirmChangesRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string url = _urlFormatter.Format(request);
            return new StackUpdateModel(request, url);
        }
    }
}
