﻿using Amazon;
using Amazon.GameLift;
using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core;

namespace Editor.CoreAPI
{
    public interface IAmazonGameLiftWrapperFactory
    {
        IAmazonGameLiftWrapper Get(string profile);
    }

    public class AmazonGameLiftWrapperFactory : IAmazonGameLiftWrapperFactory
    {
        private readonly CoreApi _coreApi;

        public AmazonGameLiftWrapperFactory(CoreApi coreApi)
        {
            _coreApi = coreApi;
        }

        public IAmazonGameLiftWrapper Get(string profile)
        {
            var credentialsResponse = _coreApi.RetrieveAwsCredentials(profile);
            if (credentialsResponse.Success)
            {
                var client = new AmazonGameLiftClient(credentialsResponse.AccessKey, credentialsResponse.SecretKey,
                    RegionEndpoint.GetBySystemName(credentialsResponse.Region));

                return new AmazonGameLiftWrapper(client);
            }

            return null;
        }
    }
}