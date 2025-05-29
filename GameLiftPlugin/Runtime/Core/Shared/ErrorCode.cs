// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLiftPlugin.Core.Shared
{
    public sealed class ErrorCode
    {
        public static readonly string NoProfileFound = "NOPROFILEFOUND";

        public static readonly string ProfileAlreadyExists = "PROFILEALREADYEXISTS";

        public static readonly string BucketNameCanNotBeEmpty = "BUCKETNAMECANNOTBEEMPTY";

        public static readonly string BucketNameAlreadyExists = "BUCKETNAMEALREADYEXISTS";

        public static readonly string BucketNameIsWrong = "BUCKETNAMEISWRONG";

        public static readonly string InvalidRegion = "INVALIDREGION";

        public static readonly string NoSettingsKeyFound = "NOSETTINGSKEYFOUND";

        public static readonly string NoSettingsFileFound = "NOSETTINGSFILEFOUND";

        public static readonly string InvalidParameters = "INVALIDPARAMETERS";

        public static readonly string InvalidSettingsFile = "INVALIDSETTINGSFILE";

        public static readonly string InvalidBucketPolicy = "INVALIDBUCKETPOLICY";

        public static readonly string AwsError = "AWSERROR";

        public static readonly string InvalidCfnTemplate = "INVALIDCFNTEMPLATE";

        public static readonly string StackDoesNotExist = "STACKDOESNOTEXIST";

        public static readonly string ParametersFileNotFound = "PARAMETERSFILENOTFOUND";

        public static readonly string TemplateFileNotFound = "TEMPLATEFILENOTFOUND";

        public static readonly string StackDoesNotHaveChanges = "STACKDOESNOTHAVECHANGES";

        public static readonly string BucketDoesNotExist = "BUCKETDOESNOTEXIST";

        public static readonly string ChangeSetNotFound = "CHANGESETNOTFOUND";

        public static readonly string ChangeSetAlreadyExists = "CHANGESETALREADYEXISTS";

        public static readonly string InvalidParametersFile = "INVALIDPARAMETERSFILE";

        public static readonly string UnknownError = "UNKNOWNERROR";

        public static readonly string FileNotFound = "FILENOTFOUND";

        public static readonly string UserNotConfirmed = "USERNOTCONFIRMED";

        public static readonly string InvalidIdToken = "INVALIDIDTOKEN";

        public static readonly string ApiGatewayRequestError = "APIGATEWAYREQUESTERROR";

        public static readonly string ConflictError = "CONFLICTERROR";

        public static readonly string NoGameSessionWasFound = "NOGAMESESSIONWASFOUND";

        public static readonly string ResourceWithTheNameRequestetAlreadyExists = "RESOURCEWITHTHENAMEREQUESTETALREADYEXISTS";

        public static readonly string InsufficientCapabilities = "INSUFFICIENTCAPABILITIES";

        public static readonly string LimitExceeded = "LIMITEXCEEDED";

        public static readonly string InvalidChangeSetStatus = "INVALIDCHANGESETSTATUS";

        public static readonly string TokenAlreadyExists = "TOKENALREADTEXISTS";

        public static readonly string InvalidCredentialsFile = "INVALIDCREDENTIALSFILE";
    }
}
