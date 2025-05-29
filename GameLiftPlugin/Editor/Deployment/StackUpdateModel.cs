// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;

namespace AmazonGameLift.Editor
{
    internal sealed class StackUpdateModel
    {
        private const string RemoveChangeAction = "Remove";

        public IReadOnlyDictionary<string, Change[]> ChangesByAction { get; }

        public IEnumerable<Change> RemovalChanges { get; }

        public bool HasRemovalChanges { get; }

        public string CloudFormationUrl { get; internal set; }

        /// <exception cref="ArgumentNullException"></exception>
        public StackUpdateModel(ConfirmChangesRequest request, string cloudFormationUrl)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ChangesByAction = request.Changes
                .GroupBy(change => change.Action)
                .ToDictionary(change => change.Key, group => group.ToArray());

            RemovalChanges = request.Changes
                .Where(change => change.Action == RemoveChangeAction)
                .ToArray();
            HasRemovalChanges = RemovalChanges.Any();
            CloudFormationUrl = cloudFormationUrl ?? throw new ArgumentNullException(nameof(cloudFormationUrl));
        }
    }
}
