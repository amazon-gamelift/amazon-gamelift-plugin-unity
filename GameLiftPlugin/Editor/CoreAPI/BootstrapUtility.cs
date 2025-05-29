// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    public class BootstrapUtility
    {
        private readonly CoreApi _coreApi;

        public static BootstrapUtility SharedInstance { get; } = new BootstrapUtility();

        internal BootstrapUtility(CoreApi coreApi = null) => _coreApi = coreApi ?? CoreApi.SharedInstance;

        public virtual GetBootstrapDataResponse GetBootstrapData()
        {
            GetSettingResponse currentRegionResponse = _coreApi.GetSetting(SettingsKeys.CurrentRegion);

            if (!currentRegionResponse.Success)
            {
                return GetFailResponse(currentRegionResponse);
            }

            GetSettingResponse currentProfileResponse = _coreApi.GetSetting(SettingsKeys.CurrentProfileName);

            if (!currentProfileResponse.Success)
            {
                return GetFailResponse(currentProfileResponse);
            }

            string profileName = currentProfileResponse.Value;
            string region = currentRegionResponse.Value;
            return Response.Ok(new GetBootstrapDataResponse(profileName, region));
        }

        internal static GetBootstrapDataResponse GetFailResponse(Response internalErrorResponse = null)
        {
            var response = new GetBootstrapDataResponse()
            {
                ErrorCode = internalErrorResponse?.ErrorCode,
                ErrorMessage = internalErrorResponse?.ErrorMessage
            };
            return Response.Fail(response);
        }
    }
}
