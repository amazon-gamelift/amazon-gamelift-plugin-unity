#!/bin/bash

# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

set -e

# Change to the script's directory to ensure relative paths work
cd "$(dirname "$0")"

# Function to prepare files needed for EC2 deployment
prepare_ec2_files() {

  if [[ "$OS_TYPE" == "linux" ]]; then
    # Generate scripts from templates
    echo "Generating scripts using the .env variables..."
    cp otel-collector/ec2/templates/gamelift/install.sh.tpl out/install.sh
    perl -i -pe "s|\{\{AMP_REMOTE_WRITE_ENDPOINT_VAR\}\}|$AMP_REMOTE_WRITE_ENDPOINT|g" out/install.sh
    perl -i -pe "s|\{\{REGION_VAR\}\}|$REGION|g" out/install.sh

    # Make generated scripts executable (Linux scripts only)
    chmod +x out/install.sh

    # Copy OTel Collector
    echo "Copying the GameLift Servers OTel Collector RPM..."
    cp otel-collector/common/gl-otel-collector.$LINUX_ARCH.rpm out/gl-otel-collector.rpm
  elif [[ "$OS_TYPE" == "windows" ]]; then

    echo "Generating scripts using the .env variables..."
    # Generate scripts from templates
    cp otel-collector/ec2/templates/gamelift/install.bat out/install.bat
    cp otel-collector/ec2/templates/gamelift/install.ps1.tpl out/install.ps1
    perl -i -pe "s|\{\{AMP_REMOTE_WRITE_ENDPOINT_VAR\}\}|$AMP_REMOTE_WRITE_ENDPOINT|g" out/install.ps1
    perl -i -pe "s|\{\{REGION_VAR\}\}|$REGION|g" out/install.ps1

    # Copy OTel Collector
    echo "Copying the GameLift Servers OTel Collector msi..."
    cp otel-collector/common/gl-otel-collector.msi out/


  else
    echo "Unsupported OS_TYPE: $OS_TYPE. Must be 'linux' or 'windows'."
    exit 1
  fi

  # Copy conf folder contents
  echo "Copying necessary configuration files..."
  mkdir -p out/conf/gl-otel-collector
  cp otel-collector/common/conf/gl-otel-collector/gamelift-base.yaml out/conf/gl-otel-collector/
  cp otel-collector/common/conf/gl-otel-collector/gamelift-ec2.yaml out/conf/gl-otel-collector/

}

# Function to prepare files needed for container deployment
prepare_container_files() {
  echo "Copying the GameLift Servers OTel Collector packages..."
  cp otel-collector/common/gl-otel-collector.$LINUX_ARCH.rpm out/gl-otel-collector.rpm
  cp otel-collector/common/gl-otel-collector.$LINUX_ARCH.deb out/gl-otel-collector.deb

  echo "Copying necessary configuration files..."
  cp otel-collector/common/conf/gl-otel-collector/gamelift-base.yaml out/
  cp otel-collector/common/conf/gl-otel-collector/gamelift-container.yaml out/

  echo "Copying container deployment files..."
  cp otel-collector/container/.env out/
  cp otel-collector/container/* out/
}

pick_deployment_mode()
{
  echo "Select the deployment mode:"
  echo "1) EC2"
  echo "2) Container"
  read -p "Enter the number corresponding to your deployment mode [1]: " mode_choice
  mode_choice=${mode_choice:-1}

  case $mode_choice in
    1)
      export DEPLOYMENT_MODE="ec2"
      ;;
    2)
      export DEPLOYMENT_MODE="container"
      ;;
    *)
      echo "Invalid choice. Please select 1 for EC2 or 2 for Container."
      exit 1
      ;;
  esac
  echo "Using deployment mode: $DEPLOYMENT_MODE"
}

pick_os()
{
  echo "Select the servers target OS:"
  echo "1) Linux"
  echo "2) Windows"
  read -p "Enter the number corresponding to your OS [1]: " os_choice
  os_choice=${os_choice:-1}

  case $os_choice in
    1)
      export OS_TYPE="linux"
      ;;
    2)
      export OS_TYPE="windows"
      ;;
    *)
      echo "Invalid choice. Please select 1 for Linux or 2 for Windows."
      exit 1
      ;;
  esac
  echo "Using OS: $OS_TYPE"
}

pick_linux_arch()
{
  echo "Select the Linux architecture:"
  echo "1) amd64 (x86_64)"
  echo "2) arm64 (aarch64)"
  read -p "Enter the number corresponding to your architecture [1]: " arch_choice
  arch_choice=${arch_choice:-1}

  case $arch_choice in
    1)
      export LINUX_ARCH="amd64"
      ;;
    2)
      export LINUX_ARCH="arm64"
      ;;
    *)
      echo "Invalid choice. Please select 1 for amd64 (x86_64) or 2 for arm64 (aarch64)."
      exit 1
      ;;
  esac
  echo "Using Linux architecture: $LINUX_ARCH"
}

pick_deployment_mode

# Check if .env file exists
if [ ! -f "otel-collector/$DEPLOYMENT_MODE/.env" ]; then
  echo "otel-collector/$DEPLOYMENT_MODE/.env file not found! Make sure you create a .env file with your values for AMP_REMOTE_WRITE_ENDPOINT and REGION. See METRICS.md for more details."
  exit 1
fi

# Warn if placeholder UUID still present (yellow if terminal supports color)
if grep -q 'UUID_HERE' otel-collector/$DEPLOYMENT_MODE/.env; then
  if [ -t 1 ]; then YELLOW='\033[33m'; RESET='\033[0m'; else YELLOW=''; RESET=''; fi
  echo -e "${YELLOW}WARNING: otel-collector/$DEPLOYMENT_MODE/.env file contains UUID_HERE. Please set a valid UUID for metrics${RESET}"
fi

# Load .env file
set -a
source otel-collector/$DEPLOYMENT_MODE/.env
set +a

# Clean up old out folder
echo "Preparing the out folder..."
rm -rf out
mkdir -p out

# Run EC2-specific preparation when requested
if [[ "$DEPLOYMENT_MODE" == "ec2" ]]; then
  pick_os

  # GameLift Servers EC2 fleets support multiple architectures on Linux.
  if [[ "$OS_TYPE" == "linux" ]]; then
    pick_linux_arch
  fi

  prepare_ec2_files
elif [[ "$DEPLOYMENT_MODE" == "container" ]]; then
  # Containers are Linux only, but we need to know the architecture.
  pick_linux_arch
  prepare_container_files
  mkdir -p out/game
fi

echo "Done! All files required for your game server build are in the 'out/' directory."
