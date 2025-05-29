# GameLift Plugin Deployment Scenarios Development Guide

## Pre-requisites

### Install python 3

* Go to https://www.python.org/downloads/
* Download and install

### Install cfn-format

* Go to https://github.com/awslabs/aws-cloudformation-template-formatter/releases
* Download cfn-format
* Unzip it to a bin folder of your PATH

## Adding a new deployment scenario

1. Create a new directories under `CloudFormation` directory named `scenario<##>_<name>`
1. Create the following files:
    * `<scenario_name>/cloudformation.yml` -- This will contain the CFN template of the deployment scenario
    * `<scenario_name>/parameters.json` -- This will contain the parameters to the CFN template
    * `<scenario_name>/lambda/*.py` -- These are the lambda function source code that will be uploaded to s3 and used to
      create the lambda functions
    * `<scenario_name>/Deployer.cs` -- This will contain the necessary data for the deployment scenario to be displayed
      in the plugin UI
    * `<scenario_name>/tests/test.py` -- This will run integ tests against a deployed scenario (the scenario is already
      deployed with `bin/deploy_cfn.py`)
1. Load the project in Unity, and .meta files will be generated for you. These .meta files will need to be committed
   eventually

## Update & test an existing deployment scenario

1. Use Amazon GameLift Plug-in for Unity to create a CloudFormation stack
    * If the deployment failed immediately, it means the template is mal-formatted. Check the error output to locate the
      error.
    * If the deployment started but eventually failed, go to CloudFormation console to find out why.
1. Modify `GAME_NAME` and `REGION` in `python3 <scenario_name>/tests/test.py` to match the stack you deployed
1. Run `aws configure` to set your default AWS credentials to match the account in which your stack was deployed
1. Run `python3 <scenario_name>/tests/test.py` to verify that the scenario is still working
1. Make any updates to the template and test, then repeat the last 3 steps, i.e. redeploy, wait, test
    * To speed up iteration, you can remove the `Fleet` resource in the template and replace all of its references to a
      hard-coded resource ID/ARN

## Run Load Test

* Reference: https://aws.amazon.com/blogs/compute/load-testing-a-web-applications-serverless-backend/

1. Install Artillery: https://artillery.io/docs/guides/getting-started/installing-artillery.html
1. `cd common/tests`
1. Get an id-token
    1. Run `aws configure` to set your default AWS credentials with permissions to your Cognito resources
    1. Sign-up for a temporary
       account: `aws cognito-idp sign-up --client-id <your-client-id> --username <temporary-user-name> --password <temporary-password>`
    1. Confirm the account
       sign-up: `aws cognito-idp admin-confirm-sign-up --user-pool-id <your-user-pool-id> --username <temporary-user-name>`
    1. Initiate
       auth: `aws cognito-idp initiate-auth --client-id <your-client-id> --auth-flow USER_PASSWORD_AUTH --auth-parameters USERNAME=<temporary-user-name>,PASSWORD=<temporary-password>`
    1. Copy the id-token from `.AuthenticationResult.IdToken`
1. Modify `loadtest.yml`
    1. `target` -- Your API gateway endpoint, e.g. `https://1234567xyz.execute-api.us-west-2.amazonaws.com/v1`
    1. `Authorization` -- id-token retrieved from the above steps
1. Run `artillery run loadtest.yml`
    * Artillery reports every 10 seconds, so you should see `10 * arrivalRate` number of results in each report. For
      most APIs, you should expect to see 2xx codes; however, due to the Web Application Firewall throttling rules (
      See `WebACL` resource in the CloudFormation templates), you will start seeing 403 errors if the requesting rate is
      too high. This is to be expected!

## Before you submit a pull request

1. Run `cfn-format --write <scenario_name>/cloudformation.yml`,
   example: `cfn-format --write scenario1_auth_only/cloudformation.yml`
    * NOTE: a caveat in cfn-format is that it does not preserve comments, we'll need to manually add copyrights back
      once the formatter completes.
   ```
   # Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
   # SPDX-License-Identifier: MIT-0
   ```
