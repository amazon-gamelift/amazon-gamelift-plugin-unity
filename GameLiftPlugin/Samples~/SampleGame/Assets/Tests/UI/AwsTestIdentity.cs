// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace SampleTests.UI
{
    internal sealed class AwsTestIdentity
    {
        private const string TestPass = "S1mplep@ss";
        private readonly AmazonCognitoIdentityProviderClient _amazonCognitoIdentityProvider;
        private readonly TestSettings _settings;

        public string Email { get; private set; }

        public string Password => TestPass;

        public AwsTestIdentity(TestSettings settings)
        {
            _settings = settings;

            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.Region);
            _amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(settings.AccessKey, settings.SecretKey, regionEndpoint);
        }

        public void CreateTestUser()
        {
            string timestamp = DateTime.UtcNow.ToBinary().ToString();
            Email = $"valid.email{timestamp}@mail.com";
        }

        public void SignUpTestUser()
        {
            if (Email == null)
            {
                CreateTestUser();
            }

            _amazonCognitoIdentityProvider.AdminCreateUser(new AdminCreateUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = Email,
                TemporaryPassword = TestPass
            });

            _amazonCognitoIdentityProvider.AdminSetUserPassword(new AdminSetUserPasswordRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = Email,
                Password = TestPass,
                Permanent = true
            });
        }

        public void DeleteTestUser()
        {
            if (Email == null)
            {
                return;
            }

            try
            {
                _amazonCognitoIdentityProvider.AdminDeleteUser(new AdminDeleteUserRequest
                {
                    UserPoolId = _settings.UserPoolId,
                    Username = Email
                });
                Email = null;
            }
            catch (UserNotFoundException) { }
        }
    }
}
