// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    internal sealed class TextProvider
    {
        private readonly Dictionary<string, string> _errorMessagesByCode = new Dictionary<string, string>
        {
            { CoreErrorCode.AwsError, "AWS error."},
            { CoreErrorCode.BucketDoesNotExist, "The bucket does not exist."},
            { CoreErrorCode.BucketNameAlreadyExists, "The bucket already exists."},
            { CoreErrorCode.BucketNameCanNotBeEmpty, "The bucket name cannot be empty."},
            { CoreErrorCode.BucketNameIsWrong, "There was a problem with the bucket name."},
            { CoreErrorCode.ChangeSetAlreadyExists, "The changeset already exists."},
            { CoreErrorCode.ChangeSetNotFound, "The changeset was not found."},
            { CoreErrorCode.ConflictError, "There was a conflict."},
            { CoreErrorCode.FileNotFound, "The file was not found."},
            { CoreErrorCode.InsufficientCapabilities, "Insufficient capabilities."},
            { CoreErrorCode.InvalidBucketPolicy, "There was a problem with the bucket policy."},
            { CoreErrorCode.InvalidCfnTemplate, "There was a problem with the template."},
            { CoreErrorCode.InvalidChangeSetStatus, "There was a problem with the changeset status."},
            { CoreErrorCode.InvalidCredentialsFile, "There was a problem with the credentials file." },
            { CoreErrorCode.InvalidIdToken, "There was a problem with the ID token."},
            { CoreErrorCode.InvalidParameters, "There was a problem with the parameters."},
            { CoreErrorCode.InvalidParametersFile, "There was a problem with the parameters file."},
            { CoreErrorCode.InvalidRegion, "There was a problem with the Region."},
            { CoreErrorCode.InvalidSettingsFile, "There was a problem with the settings file."},
            { CoreErrorCode.LimitExceeded, "Limit exceeded."},
            { CoreErrorCode.NoGameSessionWasFound, "No game session was found."},
            { CoreErrorCode.NoProfileFound, "The profile was not found."},
            { CoreErrorCode.NoSettingsFileFound, "The settings file was not found."},
            { CoreErrorCode.NoSettingsKeyFound, "The settings key was not found."},
            { CoreErrorCode.ParametersFileNotFound, "The parameters file was not found."},
            { CoreErrorCode.ProfileAlreadyExists, "The profile already exists."},
            { CoreErrorCode.ResourceWithTheNameRequestetAlreadyExists, "A resource with the requested name already exists."},
            { CoreErrorCode.StackDoesNotExist, "The stack does not exist."},
            { CoreErrorCode.StackDoesNotHaveChanges, "The stack does not have changes."},
            { CoreErrorCode.TemplateFileNotFound, "Template file was not found."},
            { CoreErrorCode.TokenAlreadyExists, "The token already exists."},
            { CoreErrorCode.UnknownError, "Unknown error."},
            { CoreErrorCode.UserNotConfirmed, "The user did not confirm registration."},
            { ErrorCode.DeregisterComputeFailed, "There was a problem deregistering existing compute"},
            { ErrorCode.ChangeSetStatusInvalid, "There was a problem with the changeset status."},
            { ErrorCode.OperationCancelled, "The operation was cancelled."},
            { ErrorCode.OperationInvalid, "There was a problem with the operation."},
            { ErrorCode.ReadingFileFailed, "There was a problem reading the file."},
            { ErrorCode.StackStatusInvalid, "Something went wrong  with the deployment that requires attention. Go to the AWS CloudFormation console and view details for the failing stack. After resolving the problem, delete and re-create the deployment."},
            { ErrorCode.ValueInvalid, "There was a problem with the value."},
            { ErrorCode.WritingFileFailed, "There was a problem writing to the file."},
            { ErrorCode.GameLiftClientSettingsNotFoundText, "No GameLiftClientSettings asset found. Please import the sample game or create one custom."},
            { ErrorCode.GameLiftClientSettingsMoreThanOneFoundTemplate, "More than one GameLiftClientSettings asset was found. Using asset found at \"{0}\"."},
        };

        private readonly Dictionary<string, string> _textsByKey = new Dictionary<string, string>
        {
            { Strings.LabelBootstrapBucketCosts, "Using S3 will incur costs to your account via Storage, Transfer, etc. See S3 Pricing Plan (https://aws.amazon.com/s3/pricing/) for details."},
            { Strings.LabelBootstrapBucketName, "Bucket Name"},
            { Strings.LabelBootstrapCreateButton, "Create"},
            { Strings.LabelBootstrapCreateMode, "Create a new S3 bucket"},
            { Strings.LabelBootstrapBucketLifecycle, "Policy"},
            { Strings.LabelBootstrapLifecycleWarning, "With lifecycle policy configured on the S3 bucket, stale build artifacts in S3 will be deleted automatically, and will cause fleet creation to fail when created with a build referencing the deleted artifacts."},
            { Strings.LabelBootstrapBucketSelectionLoading, "Loading S3 Buckets..."},
            { Strings.LabelBootstrapRegion, "AWS Region"},
            { Strings.LabelBootstrapSelectButton, "Update"},
            { Strings.LabelBootstrapSelectMode, "Choose existing S3 bucket"},
            { Strings.LabelBootstrapCurrentBucket, "Current S3 bucket"},
            { Strings.LabelBootstrapS3Console, "Go to S3 console"},
            { Strings.LabelCredentialsCreateButton, "Create Credentials Profile"},
            { Strings.LabelCredentialsCreateMode, "Create new credentials profile"},
            { Strings.LabelCredentialsUpdateButton, "Update Credentials Profile"},
            { Strings.LabelCredentialsUpdateMode, "Choose existing credentials profile"},
            { Strings.LabelCredentialsHelp, "How do I create AWS credentials?"},
            { Strings.LabelCredentialsCreateProfileName, "New Profile Name"},
            { Strings.LabelCredentialsCurrentProfileName, "Current Profile"},
            { Strings.LabelCredentialsSelectProfileName, "Profile Name"},
            { Strings.LabelCredentialsAccessKey, "AWS Access Key ID"},
            { Strings.LabelCredentialsRegion, "AWS Region"},
            { Strings.LabelCredentialsSecretKey, "AWS Secret Key"},
            { Strings.LabelDefaultFolderName, "New"},
            { Strings.LabelDeploymentApiGateway, "API Gateway Endpoint"},
            { Strings.LabelDeploymentBootstrapWarning, "You must configure your AWS credentials and a bootstrapping location before deploying a scenario.."},
            { Strings.LabelDeploymentBucket, "S3 Bucket"},
            { Strings.LabelDeploymentBuildFilePath, "Game Server Executable Path"},
            { Strings.LabelDeploymentBuildFolderPath, "Game Server Build Folder Path"},
            { Strings.LabelDeploymentCancelDialogBody, "Are you sure you want to cancel?"},
            { Strings.LabelDeploymentCancelDialogCancelButton, "No"},
            { Strings.LabelDeploymentCancelDialogOkButton, "Yes"},
            { Strings.LabelDeploymentCloudFormationConsole, "View AWS CloudFormation Console"},
            { Strings.LabelDeploymentCosts, "Deploying and running various AWS resources in the CloudFormation template will incur costs to your account. See AWS Pricing Plan (https://aws.amazon.com/pricing/) for details on the cost of each resource in the CloudFormation template."},
            { Strings.LabelDeploymentCognitoClientId, "Cognito Client ID"},
            { Strings.LabelDeploymentCustomMode, "Custom scenario"},
            { Strings.LabelDeploymentHelp, "How to deploy your first game?"},
            { Strings.LabelDeploymentGameName, "Game Name"},
            { Strings.LabelDeploymentCurrentStack, "Deployment Status"},
            { Strings.LabelDeploymentRegion, "AWS Region"},
            { Strings.LabelDeploymentScenarioHelp, "Learn more"},
            { Strings.LabelDeploymentScenarioPath, "Scenario Path"},
            { Strings.LabelDeploymentSelectionMode, "Scenario"},
            { Strings.LabelDeploymentStartButton, "Start Deployment"},
            { Strings.LabelDeploymentCancelButton, "Cancel Deployment"},
            { Strings.LabelSettingsAwsCredentialsSetUpButton, "AWS Credentials"},
            { Strings.LabelAwsCredentialsUpdateButton, "Update AWS Credentials"},
            { Strings.LabelPasswordHide, "Hide"},
            { Strings.LabelPasswordShow, "Show"},
            { Strings.LabelSettingsAllConfigured, "The GameLift Plugin is configured. You can build the sample game client and server and deploy a sample deployment scenario."},
            { Strings.LabelSettingsAwsCredentialsTitle, "AWS Credentials"},
            { Strings.LabelSettingsBootstrapTitle, "AWS Account Bootstrapping"},
            { Strings.LabelSettingsBootstrapButton, "Update Account Bootstrap"},
            { Strings.LabelSettingsBootstrapWarning, "To bootstrap your AWS account, configure your  AWS credentials"},
            { Strings.LabelSettingsConfigured, "Configured"},
            { Strings.LabelSettingsNotConfigured, "Not Configured"},
            { Strings.LabelSettingsDotNetButton, "Use .NET 4.x"},
            { Strings.LabelSettingsDotNetTitle, ".NET Settings"},
            { Strings.LabelSettingsJavaButton, "Download JRE"},
            { Strings.LabelSettingsJavaTitle, "JRE"},
            { Strings.LabelSettingsSdkTab, "SDK"},
            { Strings.LabelSettingsDeployTab, "Deploy"},
            { Strings.LabelSettingsTestTab, "Test"},
            { Strings.LabelSettingsHelpTab, "Help"},
            { Strings.LabelSettingsOpenAwsHelp, "Open AWS documentation"},
            { Strings.LabelSettingsOpenForums, "Open AWS GameTech Forums"},
            { Strings.LabelSettingsOpenDeployment, "Open Deployment UI"},
            { Strings.LabelSettingsPingSdk, "Show SDK DLL Files"},
            { Strings.LabelSettingsReportSecurity, "Report security vulnerabilities"},
            { Strings.LabelSettingsReportBugs, "Report bugs/issues"},
            { Strings.LabelStackUpdateCancelButton, "Cancel"},
            { Strings.LabelStackUpdateCountTemplate, "Resources with action {0}: {1}"},
            { Strings.LabelStackUpdateConfirmButton, "Deploy"},
            { Strings.LabelStackUpdateConsole, "View the changeset in CloudFormation console"},
            { Strings.LabelStackUpdateConsoleWarning, "Do not \"Execute\" the changeset when previewing it in the AWS CloudFormation console."},
            { Strings.LabelStackUpdateQuestion, "Do you want to redeploy your current stack?"},
            { Strings.LabelStackUpdateRemovalHeader, "The following resources will be removed during deployment:"},
            { Strings.LifecycleNone, "No Lifecycle Policy"},
            { Strings.LifecycleSevenDays, "7 days"},
            { Strings.LifecycleThirtyDays, "30 days"},
            { Strings.LoremIpsum, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." },
            { Strings.StatusDeploymentExePathInvalid, "There was a problem with deployment. The build exe is not in the build folder."},
            { Strings.StatusDeploymentFailure, "There was a problem with deployment. Error: {0}."},
            { Strings.StatusDeploymentStarting, "Preparing to deploy the scenario..."},
            { Strings.StatusDeploymentSuccess, "Deployment success."},
            { Strings.StatusExceptionThrown, "An exception was thrown. See the exception details in the Console."},
            { Strings.StatusNothingDeployed, "Nothing is deployed. Click on the «Deploy» button below."},
            { Strings.StatusProfileCreated, "Profile Created. Please bootstrap the account"},
            { Strings.StatusProfileUpdated, "Credentials Updated. Please bootstrap account if necessary"},
            { Strings.StatusBootstrapComplete, "Account bootstrap S3 bucket created!"},
            { Strings.StatusBootstrapUpdateComplete, "Account bootstrapped."},
            { Strings.StatusBootstrapFailedTemplate, "{0} {1}"},
            { Strings.StatusGetProfileFailed, "There was a problem getting the current AWS profile."},
            { Strings.StatusGetRegionFailed, "There was a problem getting the current AWS Region."},
            { Strings.StatusRegionUpdateFailedWithError, "There was a problem updating the AWS Region: {0}."},
            { Strings.TitleAwsCredentials, "AWS Credentials"},
            { Strings.TitleBootstrap, "Account Bootstrapping"},
            { Strings.TitleDeployment, "Deployment"},
            { Strings.TitleDeploymentCancelDialog, "Cancel Current Deployment?"},
            { Strings.TitleDeploymentServerFileDialog, "Select a server build executable file"},
            { Strings.TitleDeploymentServerFolderDialog, "Select a server build folder"},
            { Strings.TitleSettings, "GameLift Plugin Settings"},
            { Strings.TitleStackUpdateDialog, "Review Deployment"},
            { Strings.TooltipCredentialsAccessKey, "Enter the key ID."},
            { Strings.TooltipCredentialsCreateProfile, "Enter your AWS account name."},
            { Strings.TooltipCredentialsRegion, "Select the AWS Region to create the S3 bucket."},
            { Strings.TooltipCredentialsSecretKey, "Enter the secret key."},
            { Strings.TooltipCredentialsSelectProfile, "Select the AWS account name."},
            { Strings.TooltipDeploymentBucket, "You can change this parameter in the Plugin Settings > Update Account Bootstrap menu if needed"},
            { Strings.TooltipDeploymentRegion, "You can change this parameter in the Plugin Settings > Update AWS Credentials menu if needed"},
            { Strings.TooltipSettingsAwsCredentials, "Setup credentials used for account bootstrapping and CloudFormation deployment."},
            { Strings.TooltipSettingsBootstrap, "Create an S3 bucket to store GameLift build artifacts and lambda function source code."},
            { Strings.TooltipSettingsDotNet, "Opt in to change \"API Compatibility Level\" of the project to 4.x, which is the latest version of .NET Framework API that the current GameLift Server SDK supports."},
            { Strings.TooltipSettingsJava, "Download and install the latest Java Runtime Environment (JRE) in order to run GameLift Local.\nNOTE: If you have JDK installed, this may show as \"Not Configured\", but local testing will work as long as you have \"java\" in your PATH environment variable"},
            { Strings.TooltipBootstrapBucketLifecycle, "Specify an expiration date of your S3 bucket."},
            { Strings.TooltipBootstrapCurrentBucket, "This is the name of the S3 bucket which is used currently."},
            { Strings.StackDetailsTemplate, "Scenario: {1}\r\nGame Name: {2}\r\nAWS Region: {0}\r\nLast Updated: {3}"},
            { Strings.StackStatusTemplate, "Deployment Status: {0}"},
            { Strings.SettingsUIDeployNextStepLabel, "The Deployment settings are configured. You can continue by building the sample game server then deploying a sample deployment scenario."},
            { Strings.SettingsUITestNextStepLabel, "The Test settings are configured. You can continue by building the sample game server, and the sample client with local testing configurations, then connect the client to the local server."},
            { Strings.SettingsUISdkNextStepLabel, "The SDK settings are configured. You can find an usage example by going to \"(Top Menu Bar) > GameLift > Import Sample Game\", and look at \"Assets\\Scripts\\Server\\GameLiftServer.cs\"."},
            { Strings.LabelOpenSdkIntegrationDoc, "Open SDK Integration Guide"},
            { Strings.LabelOpenSdkApiDoc, "Open SDK API Reference"},
            
            { Strings.TabLanding, "Amazon GameLift" },
            { Strings.TabContainers, "Managed Containers" },
            { Strings.TabCredentials, "AWS Account Access" },
            { Strings.TabAnywhere, "Anywhere" },
            { Strings.TabManagedEC2, "Managed EC2" },
            { Strings.TabHelp, "Learning Resources" },

            { Strings.LandingPageHeader, "Amazon GameLift" },
            { Strings.LandingPageDescription, "Amazon GameLift provides solutions for hosting session-based multiplayer game servers in the cloud. This plugin contains libraries and native UI elements that make it easier to integrate Amazon GameLift into your game and to manage your hosting resources. Use the plugin to access the Amazon GameLift APIs and deploy AWS CloudFormation templates for common deployment scenarios. \nBuilt on AWS global computing infrastructure, Amazon GameLift helps you deliver high-performance, high-reliability, low-cost game servers that scale to meet player demand." },
            { Strings.LandingPageNoAccountCardText, "I don't have an AWS account" },
            { Strings.LandingPageNoAccountCardButton, "Create an AWS Account" },
            { Strings.LandingPageAccountCardText, "I have an AWS account" },
            { Strings.LandingPageAccountCardButton, "Create a User Profile" },
            { Strings.LandingPageSampleHeader, "Try our Sample Game" },
            { Strings.LandingPageSampleDescription, "Explore Amazon GameLift with our sample multiplayer game. View integration code, set up hosting with Amazon GameLift Anywhere or Managed EC2 fleets, and experiment with hosting features. Import the sample game into your project, and look for it in the project Assets." },
            { Strings.LandingPageSampleButton, "Import Sample Game" },
            { Strings.LandingPageInfoStatusBoxText, "You will need to configure an AWS account profile to use Amazon GameLift." },
            { Strings.LandingPageWarningStatusBoxText, "Profile configuration is incomplete, navigate to AWS User Profiles for next steps." },

            { Strings.LandingPageComputeHeader, "Choose a hosting option"},
            { Strings.LandingPageComputeDescription,
                "To get started, choose a hosting solution to work with. Each solution provides a step-by-step" +
                " workflow to deploy your game server and run game sessions."},
            { Strings.LandingPageAnywhereTitle, "Anywhere"},
            { Strings.LandingPageAnywhereDescription,
                "Create an Anywhere fleet with your local workstation. Prepare your game server for hosting with" +
                " Amazon GameLift and run it locally. Use a locally running game client to start a new game session" +
                " and join to play the game."},
            { Strings.LandingPageAnywhereButton, "Test locally with Anywhere"},
            { Strings.LandingPageManagedTitle, "Managed EC2"},
            { Strings.LandingPageManagedDescription,
                "Build a fleet of EC2 instances for cloud-based hosting managed by Amazon GameLift. Deploy your game" +
                " server to the fleet, start a game session, and join from a game client on any supported device."},
            { Strings.LandingPageManagedButton, "Host with Managed EC2"},
            { Strings.LandingPageContainerTitle, "Managed containers"},
            { Strings.LandingPageContainerDescription,
                "Set up a container fleet with your game server for cloud-based hosting. Deploy your containerized" +
                " game server to EC2 instances managed by Amazon GameLift. Start a game session and join from a" +
                " game client on any supported device."},
            { Strings.LandingPageContainerButton, "Host with Managed containers"},

            { Strings.UserProfilePageTitle, "Set up your user profile"},
            { Strings.UserProfilePageDescription, "An AWS profile identifies a particular user in an AWS account. It secures the user's access to Amazon GameLift resources and related AWS services. You can have multiple profiles for different AWS accounts and users. Profiles are stored on your local device."},
            { Strings.UserProfilePageAccountCardNoAccountTitle, "I need a new AWS account and/or user"},
            { Strings.UserProfilePageAccountCardNoAccountDescription, "Set up AWS account and user in the AWS Management Console. Then return here to complete your profile."},
            { Strings.UserProfilePageAccountCardNoAccountButtonLabel, "Go to AWS Management Console"},
            { Strings.UserProfilePageAccountCardHasAccountTitle, "I have an AWS account and user"},
            { Strings.UserProfilePageAccountCardHasAccountDescription, "Create a profile with an AWS account and user."},
            { Strings.UserProfilePageAccountCardHasAccountButton, "Add new profile"},
            { Strings.UserProfilePageAccountNewProfileTitle, "Profile details"},
            { Strings.UserProfilePageAccountNewProfileDescription, "Provide the requested information to create an AWS profile for use with Amazon GameLift."},
            { Strings.UserProfilePageAccountNewProfileName, "Profile name"},
            { Strings.UserProfilePageAccountNewProfileAccessKeyInput, "Access key ID"},
            { Strings.UserProfilePageAccountNewProfileSecretKeyLabel, "Secret access key"},
            { Strings.UserProfilePageAccountNewProfileRegionLabel, "Default AWS Region"},
            { Strings.UserProfilePageAccountNewProfileCreateButton, "Create new profile"},
            { Strings.UserProfilePageAccountNewProfileCancelButton, "Cancel"},
            { Strings.UserProfilePageAccountNewProfileHelpLink, "Go to IAM console"},
            { Strings.UserProfilePageTableTitle, "User profiles"},
            { Strings.UserProfilePageTableDescription, "These user profiles are configured for use with the plugin on this device. Select an existing profile to use during your current plugin session or create a new profile."},
            { Strings.UserProfilePageSetProfileButton, "Select profile"},
            { Strings.UserProfilePageBootstrapButton, "Bootstrap profile"},
            { Strings.UserProfilePageAwsConfigurationFileLabel, "AWS credentials (local file)"},
            { Strings.UserProfilePageAwsConfigurationFileTooltip, "Open file to view or update the selected profile's settings"},
            { Strings.UserProfilePageCompletedBootstrapHelpLink, "What is bootstrapping?"},
            { Strings.UserProfilePageBootstrapErrorText, "An error occurred when trying to bootstrap your S3 bucket."},
            
            { Strings.UserProfilePageBootstrapPopupWindowTitle, "Bootstrap {{PROFILE_NAME}}"},
            { Strings.UserProfilePageBootstrapPopupNoticeStatusBox, "This action creates AWS resources for your account. You might incur charges for resources and " +
                $"data storage unless your AWS account is eligible for <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits."},
            { Strings.UserProfilePageBootstrapPopupDescription, "Bootstrap your profile to prepare AWS resources for use with Amazon GameLift." +
                " This includes an Amazon S3 bucket to store project configurations, build artifacts, and other dependencies.\nProfiles do not share S3 buckets."},
            { Strings.UserProfilePageBootstrapPopupBucketText, "S3 bucket name"},
            { Strings.UserProfilePageBootstrapPopupCancelButton, "Cancel"},
            { Strings.UserProfilePageBootstrapPopupContinueButton, "Bootstrap profile"},
            
            { Strings.UserProfilePageStatusBoxSuccessText, "Profile configuration and bootstrapping complete."},
            { Strings.UserProfilePageStatusBoxWarningText, "Profile configuration is incomplete, as bootstrapping is not completed."},
            { Strings.UserProfilePageStatusBoxErrorText, "An error occurred when trying to create your profile."},
            
            { Strings.HelpfulResourceSetupAccountTitle, "Set up an AWS account" },
            { Strings.HelpfulResourceSetupAccountDescription, "Learn more about setting up an AWS account and user for use with Amazon GameLift." },
            { Strings.HelpfulResourcePluginGuideTitle, "Plugin guide: Create a user profile" },
            { Strings.HelpfulResourcePluginGuideDescription, "Get help with setting up a user profile for use with the Amazon GameLift plugin." },
            { Strings.HelpfulResourceGettingStartedTitle, "Getting started with an AWS account" },
            { Strings.HelpfulResourceOrganizingEnvTitle, "Organizing Your AWS Environment Using Multiple Accounts" },
            { Strings.HelpfulResourceGetAccessKeysTitle, "Get your access keys" },
            { Strings.HelpfulResourceGetAccessKeysDescription, "Learn how to generate a new set of security credentials for your AWS user." },
            { Strings.HelpfulResourceServiceLocationsTitle, "Amazon GameLift service locations" },
            { Strings.HelpfulResourceServiceLocationsDescription, "Choose an AWS Region for your GameLift fleet type" },
            { Strings.HelpfulResourceManageAccessKeysTitle, "Manage access keys for IAM users" },
            { Strings.HelpfulResourceManageAccessKeysDescription, "Learn how to manage and secure your access keys." },
            { Strings.HelpfulResourceHostingSolutionsTitle, "Amazon GameLift hosting solutions" },
            { Strings.HelpfulResourceHostingSolutionsDescription, "Learn more about the different ways you can host your game servers. You can use the plugin to create managed EC2 fleets, managed container fleets, and Anywhere fleets." },
            { Strings.HelpfulResourceHostingFleetTitle, "Setting up a hosting fleet with Amazon GameLift" },
            { Strings.HelpfulResourceHostingFleetDescription, "Learn about the role of a hosting fleet and how each fleet type is structured." },

            { Strings.AnywherePageTitle, "Host with Anywhere"},
            { Strings.AnywherePageDescription, "Set up an Amazon GameLift Anywhere fleet to host game servers using your own hardware. With an Anywhere fleet, Amazon GameLift manages game sessions and placement (including matchmaking), while you control your own server hosting infrastructure under a single managed solution.\nCreate an Anywhere fleet for your on-premises or other compute resources. During game development, turn your local workstation into an Anywhere fleet to continuously deploy, test, and iterate your game builds."},
            { Strings.AnywherePageIntegrateTitle, "Set up your game with Amazon GameLift"},
            { Strings.AnywherePageIntegrateDescription,
                "Identify the game server build that you want to deploy to an Anywhere fleet. Your client and server" +
                " components must be integrated and packaged with the Amazon GameLift SDKs. For guidance on" +
                " integration and packaging, see the Plugin Guide for Unreal Engine."},
            { Strings.AnywherePageIntegrateServerLink, "Integrate game server functionality with the Amazon GameLift server SDK"},
            { Strings.AnywherePageIntegrateClientLink, "Integrate game client functionality with the AWS SDK"},
            { Strings.AnywherePageCreateFleetNameHint, "The fleet name must have 1–1024 characters. Valid characters are A-Z, a-z, 0-9, _ and - (hyphen)"},
            { Strings.AnywherePageCreateFleetButton, "Create new Anywhere fleet"},
            { Strings.AnywherePageCreateFleetCancelButton, "Cancel"},
            { Strings.AnywherePageConnectFleetTitle, "Connect to an Anywhere fleet"},
            { Strings.AnywherePageConnectFleetDescription,
                "Choose an existing Anywhere fleet or create a new fleet for your deployment. An Anywhere fleet is a" +
                " collection of hosting resources that you managed (such as your local workstation). This action" +
                " might create AWS resources and incur charges to your AWS account unless the account is eligible" +
                $" for <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits."},
            { Strings.AnywherePageCreateFleetNameLabel, "Fleet name"},
            { Strings.AnywherePageConnectFleetDefault, "Choose an existing Anywhere fleet"},
            { Strings.AnywherePageConnectFleetNameLabel, "Fleet name"},
            { Strings.AnywherePageConnectFleetIDLabel, "Fleet ID"},
            { Strings.AnywherePageConnectFleetStatusLabel, "Fleet status"},
            { Strings.AnywherePageConnectFleetStatusError, "Error"},
            { Strings.AnywherePageConnectFleetStatusActive, "Active"},
            { Strings.AnywherePageModifyFleetButton, "Change fleet selection"},
            { Strings.AnywherePageConnectFleetViewOnConsoleButton, "View in AWS Management Console"},
            { Strings.AnywherePageComputeTitle, "Register your workstation"},
            { Strings.AnywherePageComputeDescription,
                "Register your local workstation as a compute resource in the Anywhere fleet. Amazon GameLift" +
                " establishes communication with game server processes that are running on the compute. It sends" +
                " prompts to start game sessions and other actions.\nAfter you've registered your workstation," +
                " you're ready to launch and play your multi-player game with Amazon GameLift! Start the server and" +
                " then launch your project's game client to join a game session and start playing."},
            { Strings.AnywherePageComputeNameLabel, "Compute name"},
            { Strings.AnywherePageComputeIPLabel, "IP address"},
            { Strings.AnywherePageComputeIPDescription,
                "Defaults to the local IP address. You can use localhost (123.0.0.1) as the IP address. If your" +
                " machine is accessible via a public IP address, change this value as appropriate."},
            { Strings.AnywherePageComputeStatusLabel, "Status"},
            { Strings.AnywherePageComputeStatusRegistered, "Registered"},
            { Strings.AnywherePageComputeRegisterButton, "Register compute"},
            { Strings.AnywherePageComputeReplaceComputeButton, "Register new compute"},
            { Strings.AnywherePageComputeCancelReplaceButton, "Cancel"},
            { Strings.AnywherePageLaunchTitle, "Launch your game"},
            { Strings.AnywherePageLaunchDescription,
                "You're now ready to launch and play your multiplayer game using Amazon GameLift! Start the server" +
                " to launch a new server process and connect it to the Amazon GameLift service. Then start your game" +
                " client to request a new game session and join to play the game."},
            { Strings.AnywherePageLaunchServerLabel, "Start game server"},
            { Strings.AnywherePageLaunchServerButton, "Launch server in editor"},
            { Strings.AnywherePageConfigureClientLabel, "Configure client"},
            { Strings.AnywherePageConfigureClientButton, "Apply Anywhere settings"},
            { Strings.AnywherePageLaunchClientLabel, "Start game client"},
            { Strings.AnywherePageLaunchClientDescription, "Build and launch your configured client."},
            { Strings.AnywherePageStatusBoxDefaultComputeErrorText, "An error occurred when trying to register a compute."},
            { Strings.AnywherePageStatusBoxDefaultFleetErrorText, "An error occurred when trying to create a fleet."},
            { Strings.AnywherePageStatusBoxNotBootstrappedWarning, "Profile configuration is incomplete, as bootstrapping is not completed, navigate to AWS User Profiles for next steps."},
            
            { Strings.ManagedEC2Title, "Host with Managed EC2"},
            { Strings.ManagedEC2Description, "Managed EC2 fleets use Amazon EC2 instances to host your game servers.\nAmazon GameLift manages the instances and removes the burden of hardware and software management from hosting your games."},
            { Strings.ManagedEC2IntegrateTitle, "Set up your game with Amazon GameLift"},
            { Strings.ManagedEC2IntegrateDescription,
                "Integrate and package your game components with the Amazon GameLift SDKs. For guidance, see the " +
                $"<a href=\"{Urls.ManagedEC2IntegrateLink}\"><color=#D2D2D2><u>Plugin Guide for Unity Engine</u></color></a>."},
            { Strings.ManagedEC2IntegrateServerLink, "Integrate game server functionality with the Amazon GameLift server SDK"},
            { Strings.ManagedEC2IntegrateClientLink, "Integrate game client functionality with the AWS SDK"},
            { Strings.DeploymentScenarioTitle, "Select deployment scenario"},
            { Strings.DeploymentScenarioDescription,
                "Choose the type of hosting solution you want to create. Each scenario generates a different" +
                " collection of AWS resources. The charges to your AWS account, incurred when you deploy a fleet," +
                " vary depending on the scenario you select, unless the account is eligible for" +
                $" <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits."},
            { Strings.DeploymentScenarioHelpLinkScenarios, "Learn about deployment scenarios"},
            { Strings.DeploymentScenarioHelpLinkLocations, "Choose a location for your fleet"},
            { Strings.DeploymentScenarioHelpLinkPricing, "Amazon GameLift pricing"},
            { Strings.DeploymentScenarioSingleFleetLabelEc2,
                "Use this scenario to deploy a minimal managed EC2 fleet for your game server. This scenario also" +
                " sets up player authentication for your game and creates a backend service for game clients to start" +
                " and join game sessions."},
            { Strings.DeploymentScenarioSingleFleetLabelContainers,
                "Deploy to one On-Demand fleet, with a game backend to put players into games."},
            { Strings.DeploymentScenarioSingleFleetLink, "Learn more"},
            { Strings.DeploymentScenarioFlexMatchLabelEc2,
                "Use this scenario to deploy a fully configured set of managed EC2 fleets with matchmaking. This" +
                " scenario also sets up a queue to manage game session placement across Spot and On-Demand fleets," +
                " handles player authentication and creates a backend service for game clients to request matchmaking" +
                " and join matches."},
            { Strings.DeploymentScenarioFlexMatchLabelContainers,
                "Deploy to one multi-location On-Demand fleet, with a game backend, matchmaking, and placement queue" +
                " to put players into games."},
            { Strings.DeploymentScenarioFlexMatchLink, "Learn more"},
            { Strings.DeploymentScenarioShowMoreButton, "Show more options"},
            { Strings.ManagedEC2ParametersTitle, "Set game parameters"},
            { Strings.ManagedEC2ParametersDescription,
                "Tell us about the game server build you want to deploy to this fleet. Use a server build that's" +
                " been integrated and packaged with the Amazon GameLift SDKs. For guidance on integration and" +
                $" packaging, see the <a href=\"{Urls.ManagedEC2IntegrateLink}\"><color=#D2D2D2><u>Plugin Guide for Unity Engine</u></color></a>."},
            { Strings.ManagedEC2ParametersGameNameLabel, "Game name"},
            { Strings.ManagedEC2ParametersFleetNameLabel, "Fleet name"},
            { Strings.ManagedEC2ParametersBuildNameLabel, "Server build name"},
            { Strings.ManagedEC2ParametersLaunchParametersLabel, "Launch parameters"},
            { Strings.ManagedEC2ParametersOperatingSystemLabel, "Server build OS"},
            { Strings.ManagedEC2ParametersGameServerFolderLabel, "Server build folder"},
            { Strings.ManagedEC2ParametersGameServerFolderPath, "Server build folder path"},
            { Strings.ManagedEC2ParametersGameServerFileLabel, "Server build executable"},
            { Strings.ManagedEC2ParametersGameServerFilePath, "Server build executable path"},
            { Strings.ManagedEC2DeployTitle, "Deploy [ScenarioType]"},
            { Strings.ManagedEC2DeployDescription,
                "When you've selected a deployment scenario and set your game parameters, you're ready to deploy your" +
                " complete hosting solution with managed EC2 fleets. This action creates AWS resources and incurs" +
                " charges to your AWS account unless the account is eligible for" +
                $" <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits." +
                "\n<b>Deployment can take 30 to 40 minutes</b>. When deployment is complete, game servers are running" +
                " and waiting for players. You can launch game clients from any supported device and use it to join a" +
                " game session and start playing. Use the Start client button to launch a game client from your local" +
                " workstation. If you deployed the FlexMatch scenario, you need at least two game clients requesting" +
                " a match to start a game session."},
            { Strings.ManagedEC2DeployStatusLabel, "Status"},
            { Strings.ManagedEC2DeployStatusNotDeployed, "Not Deployed"},
            { Strings.ManagedEC2DeployStatusDeploying, "Deploying"},
            { Strings.ManagedEC2DeployStatusDeployed, "Deployed"},
            { Strings.ManagedEC2DeployStatusDeleting, "Deleting"},
            { Strings.ManagedEC2DeployStatusFailed, "Failed"},
            { Strings.ManagedEC2DeployStatusRolledBack, "Rolled back"},
            { Strings.ManagedEC2DeployStatusRollingBack, "Rolling back"},
            { Strings.ManagedEC2DeployStatusLink, "View in CloudFormation Console"},
            { Strings.ManagedEC2DeployActionsLabel, "AWS resource actions"},
            { Strings.ManagedEC2CreateStackButton, "Create"},
            { Strings.ManagedEC2DeleteStackButton, "Delete"},
            { Strings.ManagedEC2LaunchClientTitle, "Launch your game client"},
            { Strings.ManagedEC2LaunchClientLabel, "Start game client"},
            { Strings.ManagedEC2LaunchClientButton, "Launch client in editor"},
            { Strings.ManagedEC2LaunchClientDescription, "Build client executable and launch 2 to 4 client instances to start a game session."},
            { Strings.ManagedEC2ConfigureClientLabel, "Configure client"},
            { Strings.ManagedEC2ConfigureClientButton, "Apply Managed EC2 settings"},
            { Strings.ManagedEC2StatusBoxNotBootstrappedWarning, "Profile configuration is incomplete, as bootstrapping is not completed, navigate to AWS User Profiles for next steps."},
            
            { Strings.HelpPageTitle, "Learning Resources" },
            { Strings.HelpPageDescription, "Learn how to integrate and deploy games for hosting on GameLift. The GameLift service manages game server deployment, operation, and scaling. You can deploy custom-built game servers or use GameLift Realtime Servers to configure lightweight game servers for your game." },
            { Strings.HelpPageReportIssueLink, "Report Issues" },
            { Strings.HelpPageDocumentationLink, "Documentation" },
            { Strings.HelpPageForumLink, "AWS Forum" },
            { Strings.HelpPageEstimatingPriceTitle, "Estimating Price" },
            { Strings.HelpPageEstimatingPriceDescription, "With AWS Pricing Calculator, you can create a pricing estimate for Amazon GameLift. You don't need an AWS account or in-depth knowledge of AWS to use the calculator." },
            { Strings.HelpPageEstimatingPriceLink, "Learn More" },
            { Strings.HelpPageFleetIQTitle, "Amazon GameLift FleetIQ" },
            { Strings.HelpPageFleetIQDescription, "Amazon GameLift FleetIQ optimizes the use of low-cost Amazon EC2 Spot Instances for cloud-based game hosting. With Amazon GameLift FleetIQ, you can work directly with your hosting resources in Amazon EC2 and Amazon EC2 Auto Scaling while taking advantage of Amazon GameLift optimizations to deliver inexpensive, resilient game hosting for your players." },
            { Strings.HelpPageFleetIQLink, "Learn More" },
            { Strings.HelpPageFlexMatchTitle, "Amazon GameLift FlexMatch" },
            { Strings.HelpPageFlexMatchDescription, "Amazon GameLift FlexMatch is a customizable matchmaking service for multiplayer games. With FlexMatch, you can build a custom set of rules that defines what a multiplayer match looks like for your game, and determines how to evaluate and select compatible players for each match." },
            { Strings.HelpPageFlexMatchLink, "Learn More" },
            
            { Strings.InfoLinkDocumentationLink, "Documentation" },
            { Strings.InfoLinkForumLink, "AWS GameTech Forum" },
            { Strings.InfoLinkReportIssuesLink, "Report Issues" },
            
            { Strings.ProfileSelectorDropdownLabel, "AWS Account Profile" },
            { Strings.ProfileSelectorBucketNameLabel, "S3 Bucket Name" },
            { Strings.ProfileSelectorRegionLabel, "AWS Region" },
            { Strings.ProfileSelectorStatusLabel, "Bootstrap Status" },
            { Strings.BootstrapStatusActive, "Active" },
            { Strings.BootstrapStatusInactive, "Inactive" },
            { Strings.BootstrapStatusNoBucketCreated, "No bucket created" },
            
            { Strings.ViewLogsStatusBoxUrlTextButton, "View Logs"},
            { Strings.ViewS3LogsStatusBoxUrlTextButton, "View S3 Console"},

            { Strings.WhatIsContainerImage, "What is a container image?"},
            { Strings.DockerDocumentation, "Docker Documentation"},
            { Strings.ContainerQuestionnaireDoesContainerImageExist, "Do you have an existing Container image? If not, we'll create one for you."},
            { Strings.ContainerQuestionnaireUseExistingRepo, "Do you want to store the new container image in an existing Amazon ECR repository? If not, we'll create a private repository for the image."},
            { Strings.ContainerQuestionnaireWhereItLive, "Where does your container image live?"},
            { Strings.ContainerQuestionnaireYes, "Yes"},
            { Strings.ContainerQuestionnaireNo, "No"},

            { Strings.ContainerGameServerBuildLabel, "Server build folder" },
            { Strings.ContainerGameServerExecutableLabel, "Server executable"},
            { Strings.SelectECRRepositoryLabel, "Select ECR repository"},
            { Strings.ContainerSelectImageLabel, "Select ECR image"},
            { Strings.DockerImageIDLabel, "Docker image ID"},
            { Strings.DefaultSettings, "Container deployment settings - <i>optional</i>"},
            { Strings.DefaultSettingsDescription,
                "Keep or modify these core settings for your container deployment. You can create a fully customized" +
                " container definition by using the" +
                $" <a href=\"{Urls.AwsGameLiftDocs}\"><color=#D2D2D2><u>AWS Management Console</u></color></a>" +
                $" or <a href=\"{Urls.AwsGameLiftDocs}\"><color=#D2D2D2><u>AWS CLI</u></color></a>."},
            { Strings.ContainerConnectionPortRangeLabel, "Connection port range"},
            { Strings.ContainerTotalMemoryLabel, "Game server memory limit"},
            { Strings.ContainerTotalVcpuLabel, "Game server vCPU limit"},
            { Strings.ContainerImageTagLabel, "Container image tag"},
            { Strings.ContainerConnectionPortRangeInvalidMessage, "The port range is not valid" },
            { Strings.ContainerTotalMemoryInvalidMessage, "The memory limit is not valid" },
            { Strings.ContainerTotalVcpuInvalidMessage, "The total vCPU limit is not valid" },

            { Strings.ContainerLinksDockerDocumentationLabel, "What is Docker?" },
            { Strings.ContainerLinksDockerInstallLabel, "Docker Installation Guide" },
            { Strings.ContainerLinksEcrUserGuideLabel, "Amazon ECR User Guide" },

            { Strings.ContainerConfigureDciStepTitle, "Configuring container image" },
            { Strings.ContainerConfigureDciStepDescription,
                "We're creating a Dockerfile and building your Docker container image based on the information you" +
                " provided. The Dockerfile is a blueprint for how to construct your container image. You can view the" +
                " default Dockerfile template." },
            { Strings.ContainerPushImageAutoStepTitle, "Pushing to Amazon ECR" },
            { Strings.ContainerPushImageAutoStepDescription,
                "We're pushing your Docker container image to your ECR repository. When you create a container fleet," +
                " Amazon GameLift takes a snapshot of the stored image and deploys it across all instances in the" +
                " fleet. You can view and update your ECR repositories in the AWS Console." },
            { Strings.ContainerPushImageManualStepTitle, "Build image and push to Amazon ECR" },
            { Strings.ContainerPushImageManualStepDescription,
                "We couldn't find WSL or Docker on your system. Install these required tools to complete this step on" +
                " a Windows device. Alternatively, you can manually run the CLI commands on a Linux Docker platform" +
                " with your container image folder.\nTo manually push a build image to your ECR repository complete" +
                " the following steps. For alternative registry authentication methods, including using Amazon ECR" +
                $" credential helper, see <a href=\"{Urls.HowToAuthAPrivateEcrRepo}\"><color=#D2D2D2><u>Registry authentication</u></color></a>."},
            { Strings.ContainerPushImageManualStepCallToActionLabel,
                "When you've successfully pushed your container image to the ECR repo, continue. In the next step," +
                " Amazon GameLift will try to detect the new image."},
            { Strings.ContainerPushImageManualLoginCommandLabel,
                "1. Retrieve an authentication token and authenticate your Docker client to your registry. Use the" +
                " AWS CLI or CloudShell in the AWS Console:"},
            { Strings.ContainerPushImageManualBuildCommandLabel,
                "2. Build your Docker image using the following command. For information on building a Docker file" +
                $" from scratch, see the instructions <a href=\"{Urls.HowToBuildADockerFile}\"><color=#D2D2D2><u>here</u></color></a>." +
                " You can skip this step if your image has already been built:"},
            { Strings.ContainerPushImageManualTagCommandLabel, "3. After the build is completed, tag your image so you can push the image to this repository:"},
            { Strings.ContainerPushImageManualPushCommandLabel, "4. Run the following command to push this image to your newly created AWS repository:"},
            { Strings.ContainerConfigureCGDStepTitle, "Creating container group definition" },
            { Strings.ContainerConfigureCGDStepDescription, "We're creating a container group definition for you, based on the information you provided. A container group definition describes how to deploy your" +
                " container on each compute instance in a container fleet. Amazon GameLift uses container groups to manage sets of containers and distribute computing" +
                " resources among them." },
            { Strings.ContainerCreateContainerFleetStepTitle, "Creating a container fleet" },
            { Strings.ContainerCreateContainerFleetDescription, "We're deploying your container image to fleets of EC2 computing instances, based on the deployment information you provided. Initially, Amazon GameLift deploys one EC2 instance in each fleet, generating events " +
                "and updating fleet status in real time.\nUse the Amazon GameLift console to monitor status and adjust the fleet's hosting capacity as needed." },
            { Strings.ContainerFailStepViewInConsole, "View in AWS Management Console"},

            { Strings.ContainersFleetUpdatePopupWindowTitle, "Update Fleet Deployment" },
            { Strings.ContainersFleetUpdatePopupDescription,
                "You can modify how your game server build is deployed to existing fleet instances. When you choose" +
                " 'Update', Amazon GameLift begins redeploying all active fleet instances with your changes. This" +
                " action might create AWS resources and incur charges to your AWS account unless the account is" +
                $" eligible for <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits." },
            { Strings.ContainersFleetUpdateDeploymentDetailsTitle, "Deployment Details" },
            { Strings.ContainersFleetUpdateStatusBoxText, "During fleet update deployments, active game sessions are shut down." },
            { Strings.ContainersFleetUpdateStatusBoxButtonText, "Learn more" },
            { Strings.ContainersFleetUpdatePopupVisitConsoleButtonLabel, "Visit AWS Console"},
            { Strings.ContainersFleetUpdatePopupCancelButton, "Cancel" },
            { Strings.ContainersFleetUpdatePopupUpdateButton, "Update" },

            { Strings.ContainersPageMissingWslDockerStatusBoxText,
                "<b>Missing Docker</b>\nThis required tool is not detected on your system. To continue without" +
                " Docker, you must manually build your image and push it to the repository." },
            { Strings.ContainersPageDeploymentNoticeStatusBoxText,
                "Note: Fleet deployment can take 10-20 minutes to complete. Fleet status must be \"Active\" before" +
                " you can start hosting game sessions. This action creates AWS resources and incurs charges to your" +
                " AWS account unless the account is eligible for" +
                $" <a href=\"{Urls.AwsFreeTier}\"><color=#D2D2D2><u>AWS Free Tier</u></color></a> benefits." },
            { Strings.ContainersPageRegionUnsupportedStatusBoxTemplate,
                "Region {0} does not support container fleets. Select a different profile or create a new one with a" +
                " region that supports container fleets." }
        };

        public string GetError(string errorCode = null)
        {
            if (errorCode is null)
            {
                return "Unknown error";
            }

            if (_errorMessagesByCode.TryGetValue(errorCode, out string message))
            {
                return message;
            }

            return errorCode;
        }

        /// <exception cref="ArgumentNullException"></exception>
        public string Get(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_textsByKey.TryGetValue(key, out string text))
            {
                return text;
            }

            return key;
        }
    }
}
