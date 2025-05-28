// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.Shared
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorCode { get; private set; }

        public static ValidationResult Valid()
            => new ValidationResult { IsValid = true };
        public static ValidationResult Invalid(string errorCode)
            => new ValidationResult { IsValid = false, ErrorCode = errorCode };
    }
}
