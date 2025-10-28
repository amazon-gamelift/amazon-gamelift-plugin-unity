- [Amazon GameLift Servers Metrics Deployment](#amazon-gamelift-servers-metrics-deployment)
  - [Prerequisites](#prerequisites)
- [Setup & Deployment Workflow](#setup--deployment-workflow)
  - [Step 1: Integrate Metrics Feature into Your Game Project](#step-1-integrate-metrics-feature-into-your-game-project)
    - [Prerequisites](#prerequisites-1)
    - [1. Create metrics settings](#1-create-metrics-settings)
    - [2. Add the processor MonoBehaviour](#2-add-the-processor-monobehaviour)
    - [3. Wire the GameLiftMetricsProcessor into the GameLift bootstap script](#3-wire-the-gameliftmetricsprocessor-into-the-gamelift-bootstap-script)
    - [4. Emit custom telemetry metrics (optional)](#4-emit-custom-telemetry-metrics-optional)
  - [Step 2: Enable IAM Identity Center and Deploy CloudFormation Stack](#step-2-enable-iam-identity-center-and-deploy-cloudformation-stack)
    - [Step 2.1: Enable IAM Identity Center](#step-21-enable-iam-identity-center)
    - [Step 2.2: Deploy CloudFormation Stack](#step-22-deploy-cloudformation-stack)
      - [Option 1: CloudFormation Console Deployment](#option-1-cloudformation-console-deployment)
      - [Option 2: AWS CLI Deployment](#option-2-aws-cli-deployment)
  - [Step 3: Retrieve Configuration Information](#step-3-retrieve-configuration-information)
    - [Step 3.1: Amazon Prometheus Remote Write URL](#step-31-amazon-prometheus-remote-write-url)
    - [Step 3.2: Fleet Role ARN](#step-32-fleet-role-arn)
  - [Step 4: Prepare the Amazon GameLift Servers Build](#step-4-prepare-the-amazon-gamelift-servers-build)
    - [Step 4.1: Configure Environment Variables](#step-41-configure-environment-variables)
    - [Step 4.2: Generate Deployment Files](#step-42-generate-deployment-files)
  - [Step 5: Build and Package Your Game Server](#step-5-build-and-package-your-game-server)
    - [Step 5.1: Managed EC2 Fleets](#step-51-managed-ec2-fleets)
    - [Step 5.2: Managed Container Fleets](#step-52-managed-container-fleets)
  - [Step 6 - Create an AWS Identity Center User](#step-6---create-an-aws-identity-center-user)
  - [Step 7 - Configure Amazon Grafana](#step-7---configure-amazon-grafana)
    - [Step 7.1 - Link Amazon Grafana Users](#step-71---link-amazon-grafana-users)
    - [Step 7.2 - Configure Amazon Grafana Data Sources](#step-72---configure-amazon-grafana-data-sources)
    - [Step 7.3 - Import Amazon Grafana Dashboards](#step-73---import-amazon-grafana-dashboards)
  - [What's Next](#whats-next)
- [Appendix](#appendix)
  - [Container Deployment Files Explanation](#container-deployment-files-explanation)
    - [File Descriptions](#file-descriptions)
    - [How the Container Runs](#how-the-container-runs)
    - [Customization](#customization)
  - [Amazon Grafana Dashboards](#amazon-grafana-dashboards)
    - [EC2 Fleet Overview](#ec2-fleet-overview)
      - [EC2 Fleet Overview Metrics](#ec2-fleet-overview-metrics)
    - [Instances Overview](#instances-overview)
      - [Instances Overview Metrics](#instances-overview-metrics)
    - [Instance Performance](#instance-performance)
      - [Instance Performance Metrics](#instance-performance-metrics)
    - [Container Fleet Overview](#container-fleet-overview)
      - [Container Fleet Overview Metrics](#container-fleet-overview-metrics)
    - [Container Performance](#container-performance)
      - [Container Performance Metrics](#container-performance-metrics)
    - [Server Performance](#server-performance)
      - [Note on SDKs & Plugins](#note-on-sdks--plugins)
      - [Server Performance Metrics](#server-performance-metrics)
  - [IAM Policy for CloudFormation Service Role](#iam-policy-for-cloudformation-service-role)

# Amazon GameLift Servers Metrics Deployment

This telemetry metrics solution provides comprehensive observability for dedicated game servers deployed on Amazon GameLift Servers.
The solution supports both [Amazon GameLift Servers managed EC2 fleets](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/fleets-intro-managed.html)
and [Amazon GameLift Servers managed container fleets](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/fleets-intro-containers.html).

The system collects server-side metrics via StatsD and delivers them to Amazon Managed Prometheus and Amazon CloudWatch using a customized
OpenTelemetry (OTEL) Collector. These metrics can then be visualized in Amazon Managed Grafana for comprehensive monitoring and analysis.

This guide provides step-by-step instructions for infrastructure configuration, feature integration, game server packaging, and telemetry data visualization.

## Prerequisites

Ensure you have the following requirements before proceeding:

1. **AWS Account**: An AWS account configured for Amazon GameLift Servers. Follow the [setup documentation](https://docs.aws.amazon.com/gamelift/latest/developerguide/setting-up-aws-login.html) to configure your account properly.
2. **AWS CLI**: The [AWS Command Line Interface](https://aws.amazon.com/cli/) installed and configured.
3. **Target Region**: Identify the AWS region where you plan to deploy your resources.


# Setup & Deployment Workflow
If you are using the `GameLiftPlugin`:
- Make sure to follow the [README.md](../GameLiftPlugin/README.md) for initial plugin set up. 
- Add game hosting functionality to your server code by following this guide: [Integrate your game server code](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/unity-plug-in-anywhere.html#unity-plug-in-anywhere-integrate)

Otherwise if you are using `GameLiftServerSDK`:
- Make sure to follow the [README.md](../GameLiftServerSDK/README.md) for initial plugin set up. 
- Add game hosting functionality to your server code by following this guide: [Integrate your game server code](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/unity-plug-in-anywhere.html#unity-plug-in-anywhere-integrate)

## Step 1: Integrate Metrics Feature into Your Game Project

### Prerequisites

Before adding metrics you should already have:

1. Installed the Amazon GameLift Servers SDK Unity package (see [README Installation section](../GameLiftServerSDK/README.md#installation)).
2. Imported the log4net sample if you want file logging (helps when validating metrics init).
3. A scene with the test script or your own bootstrap that successfully reaches `ProcessReady` (even if it cannot yet connect to a real fleet).

### 1. Create metrics settings

1. In Unity: Assets > Create > GameLift > Metrics Settings.
2. Select the newly created `GameLiftMetricsSettings` asset.

- We suggest you `Enable Debug Logging` here to assist with log verification.
- Optionally set custom Global Tags (e.g., `map:arena`, `ruleset:deathmatch`) to provide enhanced metadata about your metrics. See the [Global tags](default_tags_and_metrics.md#global-tags) section for the list of built-in tags and additional details.

### 2. Add the processor MonoBehaviour

In the same bootstrap scene you already use for server builds:

1. Add the `GameLiftMetricsProcessor` component to the GameObject containing your GameLift bootstrap script.
2. Assign the `GameLiftMetricsSettings` asset you created to the `GameLiftMetricsProcessor` component.

At runtime this automatically:

- Initializes the metrics system once.
- Registers default/global tags (build, fleet/process IDs when available, etc.).
- Starts time / memory (Linux) stats collection.

### 3. Wire the GameLiftMetricsProcessor into the GameLift bootstap script

1. Import the `Aws.GameLift.Unity.Metrics` namespace.

    `using Aws.GameLift.Unity.Metrics;`

2. Pass the previously created `GameLiftMetricsProcessor` reference to the bootstrap script.

Via a monobehavior:

``` c#
[SerializeField]
private GameLiftMetricsProcessor metricsProcessor;
```

Then assign `GameLiftMetricsProcessor` component to the SerializeField via the Unity Editor.

Hook these where you already handle GameLift Server SDK callbacks so player/session metrics populate:

- On Start Game Session callback: `metricsProcessor.OnGameSessionStarted(gameSession);`
- When your session ends: `metricsProcessor.OnGameSessionEnded();`

### 4. Emit custom telemetry metrics (optional)
Please follow steps in [CUSTOM_METRICS.md](CUSTOM_METRICS.md) guide to add custom telemetry metrics.

## Step 2: Enable IAM Identity Center and Deploy CloudFormation Stack

### Step 2.1: Enable IAM Identity Center

Before deploying the CloudFormation stack, enable AWS IAM Identity Center:

1. In the AWS Console, go to **IAM Identity Center** and click **Enable**.
2. Confirm your Account ID and Region, then click **Enable** again.

> **Important:** Enable IAM Identity Center in the same region where you plan to deploy the CloudFormation stack.

### Step 2.2: Deploy CloudFormation Stack

Deploy the [provided CloudFormation template](otel-collector/common/templates/cf/observability.yaml) to create the necessary AWS infrastructure. This stack provisions:

- Amazon Managed Prometheus workspace
- Amazon Managed Grafana workspace
- Required IAM roles and policies

You can deploy the stack using either the [CloudFormation console](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/cfn-console-create-stack.html) or the [AWS CLI](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/service_code_examples.html#create-stack-sdk).

#### Template Parameters

The CloudFormation template accepts the following configurable parameters:

- **WorkspaceName**: Name for the Amazon Grafana and Amazon Prometheus workspaces. Must be unique within the stack.
- **EnvironmentTag**: Environment identifier tag for created resources (e.g., `dev`, `staging`, `production`).
- **AuthenticationProviders**: [Authentication method](https://docs.aws.amazon.com/grafana/latest/userguide/getting-started-with-AMG.html#AMG-getting-started-workspace-authentication) for Amazon Grafana. Choose `SAML` if you have an existing SAML 2.0 identity provider, or `AWS_SSO` (default) to manage users through [AWS Identity Center](https://docs.aws.amazon.com/singlesignon/latest/userguide/what-is.html).

#### Option 1: CloudFormation Console Deployment

1. Navigate to **CloudFormation** in the AWS Management Console
2. Select your target AWS Region from the top navigation bar
3. Choose **Create stack** > **With new resources (standard)**
4. Select **Choose an existing template** > **Upload a template file**
5. Click **Choose file** and upload the [`observability.yaml`](otel-collector/common/templates/cf/observability.yaml) file
6. Click **Next** to validate the template
7. Provide a stack name and configure the template parameters
8. Click **Next** to proceed to stack options
9. Under **Capabilities**, acknowledge that the template creates IAM resources
10. Click **Next**, review the configuration, and click **Submit** to deploy

#### Option 2: AWS CLI Deployment

1. Open a terminal and navigate to the repository root directory
2. Execute the following command, replacing the placeholder values:

```bash
aws cloudformation create-stack \
    --stack-name [YOUR_STACK_NAME] \
    --template-body file://otel-collector/common/templates/cf/observability.yaml \
    --region [DESIRED_AWS_REGION] \
    --parameters ParameterKey="WorkspaceName",ParameterValue="[YOUR_WORKSPACE_NAME]" \
        ParameterKey="EnvironmentTag",ParameterValue="[YOUR_ENVIRONMENT_TAG]" \
        ParameterKey="AuthenticationProviders",ParameterValue="[YOUR_AUTHENTICATION_PROVIDER]" \
    --capabilities CAPABILITY_NAMED_IAM
```

> **Note:** If deploying the stack using a [CloudFormation service role](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/using-iam-servicerole.html),
> refer to the [Appendix](#iam-policy-for-cloudformation-service-role) for required policy configurations.

> **Important:** The CloudFormation template includes predefined Amazon CloudWatch dashboards. Since CloudWatch dashboard names must be globally unique
> within an AWS account, this stack can only be deployed **once per AWS account**.


## Step 3: Retrieve Configuration Information

After successful stack deployment, collect the following required configuration values:

### Step 3.1: Amazon Prometheus Remote Write URL

The OTEL Collector requires the Remote Write URL to connect to Amazon Managed Prometheus:

1. Navigate to **Amazon Prometheus** in the AWS Management Console
2. Select the region where your workspace was created
3. Click **All workspaces** in the left navigation panel
4. Select the **Workspace ID** of your newly created workspace
5. Copy the **Endpoint - remote write URL** for later use

### Step 3.2: Fleet Role ARN

If you are using the `GameLiftPlugin` to deploy your game server build, you can skip this step. In step 5, the plugin will automatically handle the following when the `Enable telemetry metrics` checkbox is selected:
- Create a fleet role with write access permissions to Prometheus and CloudWatch
- Enable generating a shared credentials file on fleet (for Managed EC2 fleets)

Otherwise if you are using the `GameLiftServerSDK`, retrieve the IAM role ARN required for fleet creation:

1. Navigate to **IAM** in the AWS Management Console
2. Click **Roles** in the left navigation menu
3. Search for `GameLiftFleetRole` (created by the CloudFormation stack)
4. Remember the role name and ARN - this will be needed when creating fleets

## Step 4: Prepare the Amazon GameLift Servers Build

### Step 4.1: Configure Environment Variables

Locate the `.env` file in either the [`otel-collector/container`](otel-collector/container/.env) or [`otel-collector/ec2`](otel-collector/ec2/.env) directory, depending on your chosen fleet type.
Note that this file may be hidden by your operating system - use `ls -al` on Mac/Linux or enable hidden file viewing on Windows.

Open the appropriate `.env` file and configure the following values with the information collected in Step 3:
- `AWS_REGION`: Your target AWS region
- `AMP_REMOTE_WRITE_ENDPOINT`: The Amazon Prometheus Remote Write URL

> **Note:** For container deployments, the `.env` file also includes `AWS_PROFILE` and `AWS_SHARED_CREDENTIALS_FILE`. These specify the AWS profile name
> and the location for storing fleet credentials. These values are predefined and it's not necessary to change them unless you have specific requirements.

### Step 4.2: Generate Deployment Files

The release bundle includes configuration files, templates, and packages required for metrics collection. The solution uses:
- [Amazon GameLift Servers build install scripts](https://docs.aws.amazon.com/gamelift/latest/developerguide/gamelift-build-cli-uploading-install.html) for managed EC2 fleets
- Pre-configured Docker files for managed container fleets

Generate all required files by running the appropriate preparation script from the repository root directory:

**Linux/macOS:**
```bash
chmod +x ./telemetry-metrics/prepare_files_to_include.sh
./telemetry-metrics/prepare_files_to_include.sh
```

**Windows:**
```powershell
powershell -file .\telemetry-metrics\prepare_files_to_include.ps1
```

When prompted, select your desired hosting solution and target operating system and architecture.

The script generates the following directory structure in the `telemetry-metrics/out/` folder depending on the chosen hosting solution and OS:

**Managed EC2 Fleet - Linux:**
```
telemetry-metrics/
└── out/
    ├── conf/
    │   └── gl-otel-collector/
    │       ├── gamelift-base.yaml
    │       └── gamelift-ec2.yaml
    ├── gl-otel-collector.rpm
    └── install.sh
```

**Managed EC2 Fleet - Windows:**
```
telemetry-metrics/
└── out/
    ├── conf/
    │   └── gl-otel-collector/
    │       ├── gamelift-base.yaml
    │       └── gamelift-ec2.yaml
    ├── gl-otel-collector.msi
    ├── install.bat
    └── install.ps1
```

**Managed Container Fleet:**
```
telemetry-metrics/
└── out/
    ├── game/ # Place your game server files here
    ├── .env
    ├── container-credentials.sh
    ├── Dockerfile.al23
    ├── Dockerfile.deb
    ├── entrypoint-game.sh
    ├── gamelift-base.yaml
    ├── gamelift-container.yaml
    ├── gl-otel-collector.deb
    ├── gl-otel-collector.rpm
    ├── supervisor-watcher.py
    └── supervisord.conf
```

## Step 5: Build and Package Your Game Server

Navigate to the generated `telemetry-metrics/out/` directory and follow the instructions specific to your chosen fleet type.

### Step 5.1: Managed EC2 Fleets

For managed EC2 fleets, package your game server and supporting files within the `out/` folder and upload to Amazon GameLift Servers:

1. Copy the generated files from the `telemetry-metrics/out/` folder to the root directory of your game server.
2. Modify the generated `install.sh` or `install.bat` file with any additional dependency setup. 

If you are using the `GameLiftPlugin` to deploy your game server build:
- Follow the Managed EC2 flow to upload your build
- Select the `Enable telemetry metrics` checkbox in the build parameters section
  - This automatically:
      - Configures a fleet role with write access permissions to Prometheus and CloudWatch
      - Enables generating a shared credentials file on fleet (for Managed EC2 fleets)
- Click deploy to start the fleet creation with the correct telemetry configuration

Otherwise if you are using `GameLiftServerSDK`:

3. Follow the [Amazon GameLift Servers documentation](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/gamelift-build-cli-uploading-builds.html) to upload your build
4. After uploading, follow the [fleet creation guide](https://docs.aws.amazon.com/gamelift/latest/developerguide/fleets-creating.html) to configure your fleet

    > **Important:** When configuring the fleet, you must enable **Generate a shared credentials file** and assign the GameLiftFleetRole created in Step 2 as the Instance role.

### Step 5.2: Managed Container Fleets

Refer to the [Appendix](#container-deployment-files-explanation) for detailed information on how container fleets integrate with the OTEL collector and the file structure included in the `telemetry-metrics/out/` folder.


#### 1. Add Game Server Files

The Dockerfile expects your game server files to follow the required directory structure. Copy your game server executable and all dependencies into the
`telemetry-metrics/out/game/` folder before building the container image. For example:

```sh
cp -r <path-to-your-game-server> telemetry-metrics/out/game
```

> **Important:**
> Make sure to update the `entrypoint-game.sh` script to point to the correct game server executable.

> **NOTE:**
> If your game requires dependencies setup:
> - For runtime environment dependencies (environment variables, runtime configurations), add them to the `entrypoint-game.sh` script
> - For installed dependencies (packages, libraries), modify the `game` image section in the Dockerfile to install the required dependencies using the appropriate package manager (e.g., `yum`, `apt-get`)

#### 2. Build the Docker Image
You can build the container image using either Amazon Linux 2023 or Debian:

**Amazon Linux 2023**

To build the Amazon Linux 2023-based image:

```sh
cd telemetry-metrics/out
# Build the image (replace <your-tag> with your desired image tag)
docker build -f Dockerfile.al23 -t <your-tag> .
```

**Debian**

To build the Debian-based image:

```sh
cd telemetry-metrics/out
# Build the image (replace <your-tag> with your desired image tag)
docker build -f Dockerfile.deb -t <your-tag> .
```

**Optional: Build and Run Locally**

```sh
# Prepare your game server files
cd telemetry-metrics/out
cp -r <path-to-your-game-server> ./game

# Build the image
docker build -f Dockerfile.al23 -t <your-tag> .

# Run the container
# (Set environment variables as needed)
docker run -e GAMELIFT_COMPUTE_TYPE=CONTAINER -e GAMELIFT_FLEET_ID=DOCKER <your-tag>
```

#### 3. Upload the Image to Amazon ECR

To deploy your container image with Amazon GameLift Servers, upload it to Amazon Elastic Container Registry (ECR).
Replace `<your-image>`, `<your-repo-name>`, `<your-region>`, and `<account-id>` with your actual values.

**Create an ECR repository (if not already created):**
   ```sh
   aws ecr create-repository --repository-name <your-repo-name> --region <your-region>
   ```

**Authenticate Docker with ECR:**
   ```sh
   aws ecr get-login-password --region <your-region> | docker login --username AWS --password-stdin <account-id>.dkr.ecr.<your-region>.amazonaws.com
   ```

**Tag the image for ECR:**
   ```sh
   docker tag <your-image>:latest <account-id>.dkr.ecr.<your-region>.amazonaws.com/<your-repo-name>:latest
   ```

**Push the image to ECR:**
   ```sh
   docker push <account-id>.dkr.ecr.<your-region>.amazonaws.com/<your-repo-name>:latest
   ```



#### 4. Create a Container Group Definition

If you are using the `GameLiftPlugin` to deploy your game server build on Managed Container fleets:
- Select the ECR image URI from the dropdown to allow the plugin to create a container group definition and container fleet automatically with your image.
- Select the `Enable telemetry metrics` checkbox to automatically configure a fleet role with write access permissions to Prometheus and CloudWatch
- Click deploy to start the container group definition and fleet creation with the correct telemetry configuration

Otherwise if you are using `GameLiftServerSDK`:
- Create a container group definition with the ECR image URI you just created to specify how Amazon GameLift Servers
should deploy your containerized game server. See the [official documentation](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/containers-create-groups.html) for detailed instructions. 

#### 5. Create a Container Fleet

If you are using the `GameLiftPlugin` to deploy your game server build:
- The plugin will automatically create the managed container fleet for you when you complete the deployment process from [step 4](#4-create-a-container-group-definition) above.

Otherwise if you are using `GameLiftServerSDK`:
- Create a managed container fleet to deploy your container group. See the [offical documentation](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/containers-build-fleet.html) for details.
    > **Important:** When creating the fleet, under the Access and logging panel, use the IAM role drop-down to select the role you created in Step 2.


## Step 6 - Create an AWS Identity Center User

This project uses AWS IAM Identity Center (AWS SSO) for identity and access management. Follow these steps to [create a user](https://docs.aws.amazon.com/singlesignon/latest/userguide/addusers.html) on your AWS account:

> **Important:** The user must be created in the same region where your CloudFormation Stack was deployed in Step 2.

1. Navigate to the **Users** tab from the left sidebar and click **Add User**.
2. Provide user details such as Username, Email, First Name, and Last Name.
3. Check your email to confirm the user’s password and complete registration.
4. In the IAM Identity Center console, go to **Permission sets** and click **Create permission set**.
5. Select a managed policy to define the user’s access level. You may optionally configure advanced settings, then create the permission set.
6. In the IAM Identity Center console, navigate to the **AWS accounts** section.
7. Choose the AWS Account you want to grant access to, then click **Assign users or groups**.
8. Select the user you just created and click next.
9. Choose the permission set you created earlier and click next.
10. Click Submit and wait for configuration to complete.

## Step 7 - Configure Amazon Grafana

After deploying your monitoring infrastructure and launching the fleet on Amazon GameLift Servers,
you can configure Amazon Managed Grafana to visualize your metrics.

### Step 7.1 - Link Amazon Grafana Users

Once the CloudFormation template has completed successfully, a new Amazon Managed Grafana workspace will be available.
To assign admin access to your Amazon Grafana workspace:

1. Use the IAM Identity Center user you created in Step 6.
2. In the AWS Console, navigate to **Amazon Grafana**.
3. Click on your workspace name (not the URL).
4. Under the **Authentication** tab, click **Assign new user or group** under the **AWS IAM Identity Center** section.
5. Follow the [official documentation](https://docs.aws.amazon.com/grafana/latest/userguide/AMG-manage-users-and-groups-AMG.html) to add the user and **assign Admin permissions to the user**.
> **Important:** Before proceeding, double check that you have granted **Admin permissions** to the user,
> otherwise you will have trouble configuring and viewing Grafana dashboards.

### Step 7.2 - Configure Amazon Grafana Data Sources

To enable metrics visualization, you must configure CloudWatch and Amazon Managed Prometheus as data sources in your Grafana workspace.

1. Open **Amazon Grafana** and click on your workspace URL.
2. Log into Amazon Grafana using the admin user from Step 7.1.
3. From the left-hand menu, go to **Apps > AWS Data Sources**.
4. Click the **Data sources** tab.
5. Under **Service**, choose **CloudWatch**.
6. Select the AWS region where you deployed the CloudFormation stack from the **Region(s)** drop down.
7. Tick the checkbox next to that region.
8. Click **Add 1 data source**.
9. Repeat steps 5 to 8, this time selecting **Amazon Managed Service for Prometheus**.
10. Once both are added, go to **Connections > Data sources** from the left sidebar, and verify that both Amazon CloudWatch and Amazon Prometheus are present.
11. Click on the Amazon CloudWatch, scroll to the bottom, and click **Save & test**. You should see a success message that indicates Amazon Grafana can query Amazon CloudWatch.
12. Repeat step 11 for the Amazon Prometheus data source.

### Step 7.3 - Import Amazon Grafana Dashboards

Template dashboards are available in the [templates/grafana](otel-collector/common/templates/grafana) directory.
See the [Appendix](#amazon-grafana-dashboards) for more details about these dashboards.
You can import them into your Amazon Grafana workspace using the following steps:

1. Go to **Amazon Grafana** in the AWS Console.
2. Click your workspace URL to open Amazon Grafana.
3. Log in using your IAM Identity Center credentials.
4. From the left-hand menu, go to **Dashboards > New > Import**.
5. Copy and paste the contents of a dashboard JSON file into the text box.
6. Click load.
7. When prompted, map the dashboard’s data sources.

> **Note:** If your Amazon Grafana workspace has not been configured with authentication,
> follow this [documentation](https://docs.aws.amazon.com/grafana/latest/userguide/AMG-data-sources-builtin.html)

Once the dashboard is imported and data sources are mapped, you can view real-time server metrics collected
from your Amazon GameLift Servers fleet.

## What's Next
After following the step-by-step workflow, you should be able to monitor your fleet with comprehensive metrics visibility.
For advanced configuration and custom metrics implementation, refer to [CUSTOM_METRICS.md](CUSTOM_METRICS.md) for detailed instructions and customization options.

# Appendix

## Container Deployment Files Explanation
### File Descriptions

- **Dockerfile.al23 / Dockerfile.deb**: Define the base image, install dependencies, copy configuration and scripts, install the OpenTelemetry Collector, and set the entrypoint.
- **container-credentials.sh**: Provides secure access to AWS credentials from within the container.
- **entrypoint-game.sh**: Entrypoint script that starts the game server process.
- **gamelift-base.yaml**: Base OpenTelemetry Collector configuration with shared components for both deployments.
- **gamelift-container.yaml**: Container-specific OpenTelemetry collector configuration.
- **supervisord.conf**: Defines which processes to run (for example, the game server and the OpenTelemetry Collector) and how they should be managed.
- **supervisor-watcher.py**: Monitors processes managed by supervisord and responds to signals as needed.
    - NOTE: Currently, this script only monitors the game process. If the game process exits, it initiates container shutdown.
- **gl-otel-collector.{arch}.rpm / gl-otel-collector.{arch}.deb**: Pre-built OpenTelemetry Collector packages for installation.
- **.env**: Example environment variables file. You can customize this file for your deployment.

### How the Container Runs

1. The container starts with `supervisord` using the `supervisord.conf` configuration file.
2. `supervisord` launches the game server (via `entrypoint-game.sh`) and the OpenTelemetry Collector as separate processes.
3. `supervisor-watcher.py` monitors these processes and triggers a container shutdown if the game process exits.
4. The OpenTelemetry Collector exports metrics to your configured destination (for example, Amazon Managed Prometheus).

### Customization

- Update the Dockerfile to copy your game server files into the image.
- Modify `gamelift-base.yaml` to adjust shared metrics collection and export settings.
- Modify `gamelift-ec2.yaml` to adjust EC2-specific metrics collection settings.
- Modify `gamelift-container.yaml` to change container-specific metrics collection for container fleets.
- Set environment variables in the `.env` or through your container orchestration platform.


## Amazon Grafana Dashboards

Template dashboards for visualizing metrics are available in the [templates/grafana](otel-collector/common/templates/grafana) directory
for Amazon Grafana and in the [CloudFormation template](otel-collector/common/templates/cf/observability.yaml) for Amazon CloudWatch.
These templates are designed as a starting point for monitoring the performance and activity of your server hosted on
Amazon GameLift Servers.

If you deployed your observability stack using the provided CloudFormation templates, these dashboards can be easily
imported into your Amazon Managed Grafana workspace. Otherwise, you will need to ensure that you have Amazon Prometheus
and Amazon CloudWatch data sources enabled.

The following dashboards are provided:

| Dashboards                     | Fleet Type        | Description                                                                                                         |
|--------------------------------|-------------------|---------------------------------------------------------------------------------------------------------------------|
| EC2 Fleet Overview             | EC2 Fleet         | Displays information on concurrent players (CCU), instances and player capacity                                     |
| Instances Overview             | EC2 Fleet         | Displays average CPU, memory, and network utilization across all fleet instances                                    |
| Instance Performance           | EC2 Fleet         | Displays detailed metrics (CPU, memory, disk, network) for an individual instance                                   |
| Server Performance (EC2)       | EC2 Fleet         | Displays the network, memory and runtime performance of a specified game server process on managed EC2 fleets       |
| Container Fleet Overview       | Container Fleet   | Displays average resource utilization of all containers in a managed container fleet                                |
| Container Performance          | Container Fleet   | Displays detailed metrics of individual containers within a specific ECS task                                       |
| Server Performance (Container) | Container Fleet   | Displays the network, memory and runtime performance of a specified game server process on managed container fleets |

**Managed EC2 Fleets:**

- EC2 Fleet Overview provides high-level fleet capacity and scaling insights
- Use Instances Overview and Instance Performance dashboards for host-level monitoring
- Metrics collected via hostmetrics receiver for system-level visibility
- Focus on EC2 instance resource utilization and performance
- Server Performance (EC2) monitors game server application metrics independent of underlying infrastructure

**Managed Container Fleets:**

- Use Container Fleet Overview and Container Performance dashboards for ECS task and container-level monitoring
- Metrics collected via ECS Container Receiver for containerized workload visibility
- Focus on task-level aggregation and container resource isolation
- Server Performance (Container) monitors game server application metrics independent of underlying infrastructure

See below for a detailed breakdown of each dashboard.

### EC2 Fleet Overview

The Fleet Overview dashboard provides a high-level view of global and regional fleet utilization and capacity. It also
includes graphs showing game server starts, stops, and crashes, along with the percentage of healthy game servers.
This dashboard supports FleetID and Location filters.


The Fleet Overview dashboard is useful for the following:

- *Fleet Capacity Monitoring Globally and by Location*
    - Capacity indicates the number of active available processes that can host game sessions. Capacity Usage is the
      percentage of available processes that are ready to host game sessions compared to the total number of available processes.
      Global CCU shows the number of concurrent players in all game servers across all instances in all locations worldwide.
      Together, the Global CCU and Capacity Usage metrics provide an overview of the total concurrent player load on the entire
      game server fleet and how that compares to available capacity.
    - When you break this down by location, you can see player load and capacity utilization by geographic region,
      and identify hotspots that may require more server resources in those locations.
- *Game Servers Health Monitoring*
    - Server Process Activations, Terminations and Crashes, and Percent Healthy Server Processes metrics provide insights
      into the operational health and stability of running game server processes.
    - Use the Crashed Game Servers table to quickly identify server issues and remediate them.
- *Fleet Scaling*
    - Monitoring Desired Instances, along with Active Instances and Idle Instance % metrics, allows you to understand current
      scaling behavior and identify opportunities to optimize fleet size, called "right sizing." This helps ensure you have
      the right amount of server capacity provisioned to handle game sessions and player loads, without over-provisioning which
      wastes resources during low-demand periods.

#### EC2 Fleet Overview Metrics

| Section                 | Panel Name            | Definition                                                                                                                                        |
|-------------------------|-----------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| Global CCU and Capacity | CCU                   | Shows the number of concurrent users in all the game servers over all the instances in all locations.                                             |
| Global CCU and Capacity | Global CCU            | Shows the number of concurrent users in all the game servers over all the instances globally.                                                     |
| Global CCU and Capacity | Active Instances      | Shows the total number of instances in the fleet that are active.                                                                                 |
| Global CCU and Capacity | Active Processes      | Shows the number of active game server processes that are ready to host a game session.                                                           |
| Global CCU and Capacity | Game Server Starts    | Shows the number of game sessions that started across the fleet.                                                                                  |
| Global CCU and Capacity | Healthy Game Servers  | Shows the average percentage of game servers that report healthy to Amazon GameLift Servers across the fleet.                                     |
| Global CCU and Capacity | Crashed Game Sessions | Shows the game session IDs of the crashed game sessions. Click the link of game session ARN to navigate to the Server Performance dashboard.      |
| Location CCU            | Location Players      | Shows the number of concurrent players in a location, including all the game servers over all the instances in the location.                      |
| Location CCU            | Location Capacity     | Shows capacity utilization (%) in a location, and the percentage of game servers in use in the location.                                          |

### Instances Overview

The Instances Overview dashboard provides insights into the performance and health of fleet instances. You can filter by
FleetID, Instance Type, or Location.

This dashboard includes graphs and tables listing the top instances by CPU, memory, network, and disk usage. Metrics
can be filtered by statistical types such as mean, max, min, p99, p95, and p90.

The Instances Overview dashboard helps with the following:

- *Fleet CPU, Network and Disk Usage Monitoring*
    - Look for CPU utilization that exceeds normal thresholds or shows consistently high usage over time.
    - Identify unexpected spikes or sustained high network usage.
    - Monitor for sudden drops in the percentage of healthy servers and investigate any declines in server health.
- *Fleet Scaling and Capacity Planning*
    - Analyze resource usage patterns to plan for future scaling needs.
- *Performance Issues and Bottlenecks Troubleshooting*
    - Identify performance bottlenecks and investigate root causes.
    - Look for unusual increases in disk read or write operations.

#### Instances Overview Metrics

| Section          | Panel Name       | Definition                                                                                      |
|------------------|------------------|-------------------------------------------------------------------------------------------------|
| Current Averages | CPU Usage        | Real-time gauges show average processor utilization across all instances.                       |
| Current Averages | Network In       | Shows the average rate of incoming network traffic across all instances.                        |
| Current Averages | Network Out      | Shows the average rate of outgoing network traffic across all instances.                        |
| Current Averages | Disk Read Ops    | Shows the average rate of disk read operations across all instances.                            |
| Current Averages | Disk Read Bytes  | Shows the average rate of disk read bytes across all instances.                                 |
| Current Averages | Disk Write Bytes | Shows the average rate of disk write bytes across all instances.                                |
| Instance Summary | CPU Usage        | Shows the average processor utilization across all instances.                                   |
| Instance Summary | Memory Usage     | Shows average memory utilization across all instances.                                          |
| Network Summary  | Network In       | Shows the average rate of incoming network traffic across all instances.                        |
| Network Summary  | Network Out      | Shows the average rate of outgoing network traffic across all instances.                        |
| Disk Summary     | Disk Write Bytes | Shows the bytes written to disk per second.                                                     |
| Disk Summary     | Disk Write Ops   | Shows the disk write operations per second.                                                     |
| Disk Summary     | Disk Read Bytes  | Shows the bytes read from disk per second.                                                      |
| Disk Summary     | Disk Read Ops    | Shows the disk read operations per second.                                                      |

### Instance Performance

The Instance Performance dashboard provides detailed performance data for a single instance. It includes tables that
highlight the top game sessions by CPU, memory, filesystem, and network usage.

The Instance Performance dashboard helps with the following:

- *Instance Monitoring*
    - Monitor for anomalies in CPU, memory, filesystem usage, and network performance for each game session on an instance. The associated tables show the top sessions in each category.
- *Network Analysis*
    - Monitor connections, errors, and bytes and packets I/O at a glance to see the network state on an instance.

#### Instance Performance Metrics

| Section             | Panel Name           | Definition                                                                                                    |
|---------------------|----------------------|---------------------------------------------------------------------------------------------------------------|
| Current Averages    | CPU Usage            | Real-time gauges show overall processor utilization across all cores.                                         |
| Current Averages    | Filesystem Usage     | Real-time gauges showing root partition storage utilization.                                                  |
| Load Summary        | Memory Usage         | Detailed breakdown of memory usage by type.                                                                   |
| Load Summary        | CPU Usage            | Shows CPU time broken down by different usage types.                                                          |
| Load Summary        | CPU Load Averages    | Shows average CPU load times.                                                                                 |
| Network Bytes I/O   | Network Bytes I/O    | Comprehensive network performance metrics.                                                                    |
| Network Bytes I/O   | Data Transfer Volume | Transmit and receive data for each of the network interfaces on the instance.                                 |
| Disk Operations I/O | Disk Operations I/O  | Disk performance metrics for Operations, Bytes, Operation Time, I/O Time, and Pending Operations.             |


### Container Fleet Overview

The Container Overview dashboard provides a task-level overview of GameLift Container Fleet performance and resource utilization.
You can filter by Fleet ID and Location to monitor ECS tasks across different regions. It aggregates container metrics to the task level,
providing fleet-wide visibility into container workload performance. It features gauges showing current CPU and memory utilization,
and tables listing the top 20 resource-consuming tasks for detailed analysis.

The Container Overview dashboard helps with the following:

- *Fleet Capacity Monitoring Globally and by Location*
  - Capacity indicates the number of active available processes that can host game sessions. Capacity Usage is the percentage of available processes that are ready to host game sessions compared to the total number of available processes.
  - Global CCU shows the number of concurrent players in all the game servers over all the instances in all locations across the world. This gives an overview of the total concurrent player load on the entire game server fleet and how that compares to the available capacity.
  - You can filter by "location" to see the player load and capacity utilization by geographic region, and identify hotspots that may require more server resources in those locations.
- *Fleet Game Servers Health Monitoring*
  - "Server Process Activations, Terminations and Crashes," and "Percent Healthy Server Processes" metrics provide insights into the operational health and stability of the running game server processes.
  - Use the Crashed Game Servers table to quickly identify server issues and remediate.
- *Fleet Game Servers Scaling*
  - Track "Desired Instances," "Active Instances," and "Idle Instance %" metrics to understand how your fleet scales and optimize your server capacity.
  - This right sizing process ensures you have enough servers to handle player demand without wasting money on unused resources during quiet periods.
- *Task-Level Fleet Resource Monitoring*
  - Monitor CPU and memory utilization across all ECS tasks in the Amazon GameLift Servers managed container fleet using rate-based calculations.
  - Identify resource hotspots and capacity planning opportunities at the task level rather than individual containers.
  - Track resource consumption patterns across different geographic regions.
- *Container Fleet Capacity Planning*
  - Analyze task-level resource usage patterns to optimize fleet sizing and resource allocation.
  - Use top resource consumer tables to identify tasks requiring attention or optimization.
- *Regional Load Distribution Analysis*
  - Monitor task performance across different AWS regions using location filtering.
  - Identify regional capacity imbalances and scaling opportunities.
- *Performance Issue Identification*
  - Identify the highest resource-consuming tasks for detailed investigation.
  - Use drill-down links to navigate from overview to detailed container performance analysis.

#### Container Fleet Overview Metrics

| Section                 | Panel Name            | Definition                                                                                                                               |
|-------------------------|-----------------------|------------------------------------------------------------------------------------------------------------------------------------------|
| Global CCU and Capacity | CCU                   | Shows the number of concurrent users in all the game servers over all the instances in all locations.                                    |
| Global CCU and Capacity | Global CCU            | Shows the number of concurrent users in all the game servers over all the instances globally.                                            |
| Global CCU and Capacity | Game Server Starts    | Shows the number of game sessions that started across the fleet.                                                                         |
| Global CCU and Capacity | Healthy Game Servers  | Shows the average percentage of game servers that report healthy to Amazon GameLift Servers across the fleet.                            |
| Global CCU and Capacity | Crashed Game Sessions | Shows the game session IDs of the crashed game sessions. Click the link of game session to navigate to the Server Performance dashboard. |
| Current Averages        | CPU Usage             | Real-time gauge showing task-level CPU utilization using rate-based calculations.                                                        |
| Current Averages        | Memory Usage          | Real-time gauge showing task-level memory utilization as percentage of reserved memory.                                                  |
| Task Summary            | Top 20 CPU            | Table showing the 20 highest CPU-consuming tasks with drill-down links to Container Performance.                                         |
| Task Summary            | Top 20 Memory         | Table showing the 20 highest memory-consuming tasks with drill-down links to Container Performance.                                      |


### Container Performance

The Container Performance dashboard provides detailed monitoring of individual containers within a specific ECS task.
You can filter by Fleet ID, Task ID, and Container Name to focus on specific containers or view all containers in a task.
It displays container-level resource utilization, network activity, and detailed performance metrics. It supports
multi-container task analysis and provides granular visibility into container resource consumption patterns.

The Container Performance dashboard helps with the following:

- *Individual Container Monitoring*
  - Monitor CPU and memory usage for specific containers within ECS tasks using rate-based calculations.
  - Analyze container resource allocation efficiency and identify optimization opportunities.
  - Track container performance trends over time for capacity planning.
- *Multi-Container Task Analysis*
  - Compare resource usage across multiple containers within the same task when "All" containers are selected.
  - Identify resource imbalances or conflicts between containers sharing task resources.
- *Container Network Performance*
  - Monitor network I/O patterns at the container level to identify connectivity issues or bandwidth bottlenecks.
  - Analyze network usage patterns for game server containers and supporting services.
- *Container Resource Optimization*
  - Use detailed CPU usage breakdown to understand kernel vs user mode processing patterns.
  - Optimize container resource reservations based on actual utilization patterns.

#### Container Performance Metrics

| Section             | Panel Name           | Definition                                                                                    |
|---------------------|----------------------|-----------------------------------------------------------------------------------------------|
| Current Averages    | CPU Usage            | Real-time gauge showing container CPU utilization using rate-based calculations.              |
| Current Averages    | Memory Usage         | Real-time gauge showing container memory utilization as percentage of reserved memory.        |
| Container Details   | CPU Usage Details    | Detailed breakdown of CPU usage by type: total, kernel mode, user mode, and system time.      |
| Container Details   | Memory Usage         | Container memory utilization over time showing usage patterns and trends.                     |
| Network Summary     | Network I/O          | Container-level network bytes in/out showing data transfer patterns.                          |


### Server Performance

> **Important:** There are two versions of this dashboard, intended for different fleet types (EC2 or Container).
> Ensure you import the correct dashboard template for your fleet type.

The Server Performance dashboard provides detailed metrics on server timings, network activity, memory, and CPU usage.
You can filter results using the Game Session ID.

The Server Performance dashboard helps with the following:

- *Server Monitoring*
    - Look for spikes or trends in server delta time, tick rate, tick time, world tick time, or CPU usage metrics that may indicate performance issues.
- *Network Analysis*
    - Monitor connection counts, network I/O, and packet loss metrics for signs of network congestion or reliability problems.
    - Analyze physical and virtual memory usage, looking for high utilization that could cause performance degradation.
- *Performance Troubleshooting*
    - Analyze server metrics and CPU resource usage to identify and diagnose performance issues.
- *Benchmarking*
    - Use server metrics to establish a performance baseline and track changes over time, enabling you to evaluate the impact of updates and configuration changes.

#### Note on SDKs & Plugins

The Server Performance dashboard is primarily populated with metrics exported directly from your dedicated game server.
- **Plugins:** If you are using the Amazon GameLift Servers Plugin for Unity or the Amazon GameLift Servers Plugin for Unreal Engine, this dashboard works out of the box.
These plugins integrate with Unity and Unreal Engine APIs to provide performance metrics automatically.

- **SDKs:** If you are using the C#, C++, or Go Amazon GameLift Servers SDKs, some metrics will not appear by default.
These SDKs are engine-agnostic and do not automatically capture engine-level runtime metrics. However, you can use the
SDK APIs to emit the required metrics from your game server code.

The following table lists the expected metric names and their types:

| Metric Name              | Type    | Description                                                                                                                                             |
|--------------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------|
| server_delta_time        | Timer   | The time elapsed between server "ticks" or updates (measured in milliseconds). This represents the interval between each server-side game state update. |
| server_tick_time         | Timer   | The actual time spent processing each server update cycle.                                                                                              |
| server_world_tick_time   | Timer   | The time spent updating the game world state and all its components.                                                                                    |
| server_connections       | Gauge   | The number of active client connections currently maintained by the game server.                                                                        |
| server_bytes_in          | Counter | The number of bytes received by the game server netcode.                                                                                                |
| server_bytes_out         | Counter | The number of bytes sent by the game server netcode.                                                                                                    |
| server_packets_in        | Counter | The number of packets received by the game server netcode.                                                                                              |
| server_packets_out       | Counter | The number of packets sent by the game server netcode.                                                                                                  |
| server_packets_in_lost   | Counter | The number of packets lost when expected to be received by the game server netcode.                                                                     |
| server_packets_out_lost  | Counter | The number of packets lost when expected to be sent by the game server netcode.                                                                         |

Refer to the [CUSTOM_METRICS.md](CUSTOM_METRICS.md) file in this package for details on creating timer, gauge and counter metrics.

#### Server Performance Metrics

| Section | Panel Name             | Definition                                                                                                                                              |
|---------|------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------|
| Timings | Server Delta Time      | The time elapsed between server "ticks" or updates (measured in milliseconds). This represents the interval between each server-side game state update. |
| Timings | Server Tick Time       | The actual time spent processing each server update cycle.                                                                                              |
| Timings | Server Tick Rate       | The number of game state updates the server performs per second (measured in Hz).                                                                       |
| Timings | Server World Tick Time | The time spent updating the game world state and all its components.                                                                                    |
| Network | Connections            | The number of active client connections currently maintained by the game server.                                                                        |
| Network | Network I/O (Packets)  | The rate and volume of network packets being sent and received.                                                                                         |
| Network | Network I/O (Bytes)    | The volume and rate of data being transferred between the game server and clients.                                                                      |
| Network | Packet Loss            | The percentage of network packets that fail to reach their destination.                                                                                 |
| Memory  | Memory Usage           | The amount of system RAM currently being used by the game server.                                                                                       |
| CPU     | CPU Usage              | The percentage of available CPU resources being utilized by this game server.                                                                           |


## IAM Policy for CloudFormation Service Role
To create a CloudFormation stack with the service role, make sure you have the required IAM policy attached:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "CloudFormationPermissions",
            "Effect": "Allow",
            "Action": [
                "cloudformation:CreateStack",
                "cloudformation:UpdateStack",
                "cloudformation:DeleteStack",
                "cloudformation:DescribeStacks"
            ],
            "Resource": "*"
        },
        {
            "Sid": "APSPermissions",
            "Effect": "Allow",
            "Action": [
                "aps:CreateWorkspace",
                "aps:DescribeWorkspace",
                "aps:DeleteWorkspace",
                "aps:ListWorkspaces",
                "aps:TagResource"
            ],
            "Resource": "*"
        },
        {
            "Sid": "GrafanaPermissions",
            "Effect": "Allow",
            "Action": [
                "grafana:CreateWorkspace",
                "grafana:DescribeWorkspace",
                "grafana:DeleteWorkspace",
                "grafana:UpdateWorkspace"
            ],
            "Resource": "*"
        },
        {
            "Sid": "IAMPermissions",
            "Effect": "Allow",
            "Action": [
                "iam:CreateRole",
                "iam:DeleteRole",
                "iam:PutRolePolicy",
                "iam:AttachRolePolicy",
                "iam:DetachRolePolicy",
                "iam:PassRole",
                "iam:GetRole"
            ],
            "Resource": "*"
        },
        {
            "Sid": "CloudWatchPermissions",
            "Effect": "Allow",
            "Action": [
                "cloudwatch:PutDashboard",
                "cloudwatch:GetDashboard",
                "cloudwatch:DeleteDashboards"
            ],
            "Resource": "*"
        }
    ]
}
```
