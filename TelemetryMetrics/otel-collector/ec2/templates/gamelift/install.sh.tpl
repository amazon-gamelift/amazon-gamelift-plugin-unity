#!/bin/bash

# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

set -eo pipefail

# If we're not root then sudo up.
if [[ "$UID" != "0" ]]; then
    sudo $0 $@
    exit $?
fi

echo "Updating dependencies..."

yum update -y

# Ensure jq is installed for JSON parsing
if ! command -v jq &> /dev/null; then
    echo "jq not found, installing..."
    yum install -y jq
fi

echo "Installing OTEL collector..."

cp /local/game/gl-otel-collector.rpm /tmp/gl-otel-collector.rpm

# Install GameLift OTEL collector.
rpm -Uvh /tmp/gl-otel-collector.rpm

echo "Configuring OTEL collector..."

# Install GameLift OTEL collector config files.
ln -s /local/game/conf/gl-otel-collector/gamelift-base.yaml /opt/aws/gl-otel-collector/etc/gamelift-base.yaml
ln -s /local/game/conf/gl-otel-collector/gamelift-ec2.yaml /opt/aws/gl-otel-collector/etc/gamelift-ec2.yaml

systemctl daemon-reload

# Parse fleetId and ComputeType from gamelift-metadata.json
# Default compute type to EC" if it wasn't found in the metadata.
FLEET_ID=$(jq -r '.fleetId' /local/gamemetadata/gamelift-metadata.json)
COMPUTE_TYPE=$(jq -r '.ComputeType // empty' /local/gamemetadata/gamelift-metadata.json)
if [ -z "$COMPUTE_TYPE" ]; then
    COMPUTE_TYPE="EC2"
fi

# Set environment variables for AWS OTel collector.
echo "AWS_PROFILE=FleetRoleCredentials" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env
echo "AWS_SHARED_CREDENTIALS_FILE=/local/credentials/credentials" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env
echo "AMP_REMOTE_WRITE_ENDPOINT={{AMP_REMOTE_WRITE_ENDPOINT_VAR}}" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env
echo "REGION={{REGION_VAR}}" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env
echo "GAMELIFT_FLEET_ID=$FLEET_ID" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env
echo "GAMELIFT_COMPUTE_TYPE=$COMPUTE_TYPE" | sudo tee -a /opt/aws/gl-otel-collector/etc/.env

echo "Enabling OTEL collector service..."

# Enable the GameLift OTEL Collector.
systemctl enable gl-otel-collector.service
