# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

import os
import shutil
import subprocess
import tempfile
from pathlib import Path

PLUGINS_OUTPUT_DIRECTORY = 'Plugins'
EXPECTED_PLUGIN_COUNT = 8

# main
script_path = os.path.dirname(os.path.abspath(__file__))
plugins_output_path = Path(script_path, PLUGINS_OUTPUT_DIRECTORY)
config_path = Path(script_path, 'packages.config')

temp_directory = tempfile.TemporaryDirectory()

# Download the third party NuGet packages
nuget_return_code = subprocess.run(['nuget', 'restore', config_path, '-PackagesDirectory', temp_directory.name]).returncode
if nuget_return_code > 0:
    raise Exception("nuget restore did not exit successfully, please check logs to fix any errors.")

# Copy the .NET standard 2.0 version of the DLLs
if os.path.exists(plugins_output_path) and os.path.isdir(plugins_output_path):
    shutil.rmtree(plugins_output_path)
os.mkdir(plugins_output_path)

filepaths = list(Path(temp_directory.name).glob('*/lib/netstandard2.0/*.dll'))
if len(filepaths) < EXPECTED_PLUGIN_COUNT:
    raise Exception("failed to download the necessary NuGet packages, please check logs to fix any errors.")

for filepath in filepaths:
    shutil.move(filepath, plugins_output_path)

temp_directory.cleanup()
