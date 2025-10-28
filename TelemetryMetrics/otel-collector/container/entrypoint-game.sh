#!/bin/bash

# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

# entrypoint-game.sh
# This script acts as the entrypoint for your game server process.
# It is intended to be run by supervisor and can be customized for your own game.

# --- CONFIGURATION SECTION ---
# Set the path to your game executable below.
GAME_EXECUTABLE="/local/game/<PATH_TO_GAME_EXECUTABLE>"
GAME_ARGUMENTS=""

# You can set any environment variables needed for your game here.
# export GAME_ENV_VAR=value

# --- LAUNCH SECTION ---

echo "Starting game server: $GAME_EXECUTABLE $GAME_ARGUMENTS"
if [ -n "$GAME_ARGUMENTS" ]; then
    exec "$GAME_EXECUTABLE" $GAME_ARGUMENTS
else
    exec "$GAME_EXECUTABLE"
fi

# If you need to run additional setup before launching your game,
# add those commands above the exec line.

# For example:
# echo "Preparing game assets..."
# ./prepare-assets.sh

# The 'exec' command replaces the shell with your game process.
# This ensures proper signal handling in containers.

