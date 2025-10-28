#!/bin/bash

# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

# Periodically queries IMDS proxied to the gamelift container
# for credentials and writes them to $AWS_SHARED_CREDENTIALS_FILE.

: "${AWS_PROFILE:=FleetRoleCredentials}"
: "${AWS_SHARED_CREDENTIALS_FILE:=/local/credentials/credentials}"
CRED_DIR="$(dirname "$AWS_SHARED_CREDENTIALS_FILE")"
CRED_FILE="$AWS_SHARED_CREDENTIALS_FILE"

if [ ! -d "$CRED_DIR" ]; then
    echo "Creating AWS credentials directory"
    mkdir -p "$CRED_DIR"
fi

while true; do
    echo "Fetching AWS credentials from IMDS"
    CREDS_JSON=$(curl -s http://169.254.170.2${AWS_CONTAINER_CREDENTIALS_RELATIVE_URI})
    ACCESS_KEY=$(echo "$CREDS_JSON" | grep -oP '"AccessKeyId"\s*:\s*"\K[^"]+')
    SECRET_KEY=$(echo "$CREDS_JSON" | grep -oP '"SecretAccessKey"\s*:\s*"\K[^"]+')
    SESSION_TOKEN=$(echo "$CREDS_JSON" | grep -oP '"Token"\s*:\s*"\K[^"]+')
    if [ -n "$ACCESS_KEY" ] && [ -n "$SECRET_KEY" ] && [ -n "$SESSION_TOKEN" ]; then
        cat > "$CRED_FILE" <<EOF
[$AWS_PROFILE]
aws_access_key_id = $ACCESS_KEY
aws_secret_access_key = $SECRET_KEY
aws_session_token = $SESSION_TOKEN
EOF
        echo "Wrote credentials to $CRED_FILE, will refresh in 5 minutes"
        sleep 300
    else
        echo "Failed to parse credentials from IMDS response. Retrying in 5 seconds..."
        sleep 5
    fi
done
