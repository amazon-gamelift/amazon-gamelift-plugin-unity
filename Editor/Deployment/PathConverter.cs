// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;

namespace AmazonGameLift.Editor
{
    internal class PathConverter
    {
        private readonly CoreApi _coreApi;

        public static PathConverter SharedInstance { get; } = new PathConverter(CoreApi.SharedInstance);

        public PathConverter(CoreApi coreApi) =>
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));

        /// <summary>Returns the absolute containers folder path, if the plugin is located under 'Assets' or 'Packages'.</summary>=
        public virtual string GetContainersAbsolutePath()
        {
            string containersInternalPath = $"{Paths.PackageName}/{Paths.ContainersRootInPackage}/";
            string containersAssetPath = $"Assets/{containersInternalPath}";
            string containersPath = Path.GetFullPath(containersAssetPath);

            if (!_coreApi.FileExists(containersPath))
            {
                string containersPackagePath = $"Packages/{containersInternalPath}";
                containersPath = Path.GetFullPath(containersPackagePath);
            }

            return Path.GetDirectoryName(containersPath);
        }

        /// <summary>Returns the absolute scenario folder path, if the plugin is located under 'Assets' or 'Packages'.</summary>
        /// <exception cref="ArgumentException">If <paramref name="scenarioFolderName"/> is null or empty.</exception>
        public virtual string GetScenarioAbsolutePath(string scenarioFolderName)
        {
            if (string.IsNullOrEmpty(scenarioFolderName))
            {
                throw new ArgumentException(DevStrings.StringNullOrEmpty, nameof(scenarioFolderName));
            }

            if (scenarioFolderName.StartsWith("Assets/"))
            {
                return Path.GetFullPath(scenarioFolderName);
            }

            string parametersInternalPath = $"{Paths.PackageName}/{Paths.ScenariosRootInPackage}/{scenarioFolderName}/{Paths.ParametersFileName}";
            string parametersAssetPath = $"Assets/{parametersInternalPath}";
            string parametersPath = Path.GetFullPath(parametersAssetPath);

            if (!_coreApi.FileExists(parametersPath))
            {
                string parametersPackagePath = $"Packages/{parametersInternalPath}";
                parametersPath = Path.GetFullPath(parametersPackagePath);
            }

            return Path.GetDirectoryName(parametersPath);
        }

        /// <exception cref="ArgumentException">If <paramref name="scenarioAssetFolderPath"/> is null or empty.</exception>
        public virtual string GetCustomScenarioAbsolutePath(string scenarioAssetFolderPath)
        {
            if (string.IsNullOrEmpty(scenarioAssetFolderPath))
            {
                throw new ArgumentException(DevStrings.StringNullOrEmpty, nameof(scenarioAssetFolderPath));
            }

            string parametersAssetPath = $"{scenarioAssetFolderPath}/{Paths.ParametersFileName}";
            string parametersPath = Path.GetFullPath(parametersAssetPath);
            return Path.GetDirectoryName(parametersPath);
        }

        /// <exception cref="ArgumentException">If <paramref name="scenarioFolderPath"/> is null or empty.</exception>
        public virtual string GetParametersFilePath(string scenarioFolderPath)
        {
            if (string.IsNullOrEmpty(scenarioFolderPath))
            {
                throw new ArgumentException(DevStrings.StringNullOrEmpty, nameof(scenarioFolderPath));
            }

            return Path.Combine(scenarioFolderPath, Paths.ParametersFileName);
        }
    }
}
