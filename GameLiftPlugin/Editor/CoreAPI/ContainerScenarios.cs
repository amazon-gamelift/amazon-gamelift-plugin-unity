// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    public enum ContainerScenarios
    {
        NoContainerImageNoExistingEcrRepo = 0,
        NoContainerImageUseExistingEcrRepo = 1,
        HaveContainerImageInDocker = 2, 
        HaveContainerImageInEcr = 3,
    }
}