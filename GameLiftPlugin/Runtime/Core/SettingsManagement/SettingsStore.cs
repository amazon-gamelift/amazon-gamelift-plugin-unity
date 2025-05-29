// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using AmazonGameLiftPlugin.Core.Shared.SettingsStore;
using YamlDotNet.RepresentationModel;

namespace AmazonGameLiftPlugin.Core.SettingsManagement
{
    public class SettingsStore : ISettingsStore
    {
        private readonly string _settingsFilePath;
        private readonly IFileWrapper _fileWrapper;
        private readonly IStreamWrapper _yamlStreamWrapper;

        public SettingsStore(IFileWrapper fileWrapper, IStreamWrapper yamlStreamWrapper = default, string settingsFilePath = default)
        {
            _fileWrapper = fileWrapper;
            _yamlStreamWrapper = yamlStreamWrapper ?? new YamlStreamWrapper();
            _settingsFilePath = settingsFilePath ?? $"{Directory.GetCurrentDirectory()}/settings.yaml";
        }

        public PutSettingResponse PutSetting(PutSettingRequest request)
        {
            if (string.IsNullOrEmpty(request.Key) || string.IsNullOrEmpty(request.Value))
            {
                return Response.Fail(new PutSettingResponse()
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            if (!_fileWrapper.FileExists(_settingsFilePath))
            {
                InitSettingsFile();
            }

            var input = new StringReader(_fileWrapper.ReadAllText(_settingsFilePath));

            _yamlStreamWrapper.Load(input);

            YamlMappingNode rootMappingNode = (_yamlStreamWrapper.GetDocuments().Count > 0) ? (YamlMappingNode)_yamlStreamWrapper.GetDocuments()[0].RootNode : default;

            if (rootMappingNode == null)
            {
                return Response.Fail(new PutSettingResponse()
                {
                    ErrorCode = ErrorCode.InvalidSettingsFile
                });
            }

            if (rootMappingNode.Children.ContainsKey(request.Key))
            {
                rootMappingNode.Children.Remove(request.Key);
            }

            rootMappingNode.Add(request.Key, request.Value);

            using (TextWriter writer = _fileWrapper.CreateText(_settingsFilePath))
            {
                _yamlStreamWrapper.Save(writer, false);
            }

            return Response.Ok(new PutSettingResponse());
        }

        public GetSettingResponse GetSetting(GetSettingRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
            {
                return Response.Fail(new GetSettingResponse
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            if (_fileWrapper.FileExists(_settingsFilePath))
            {
                var input = new StringReader(_fileWrapper.ReadAllText(_settingsFilePath));

                _yamlStreamWrapper.Load(input);

                YamlMappingNode rootMappingNode = (_yamlStreamWrapper.GetDocuments().Count > 0) ? (YamlMappingNode)_yamlStreamWrapper.GetDocuments()[0].RootNode : default;

                if (rootMappingNode == null)
                {
                    return Response.Fail(new GetSettingResponse()
                    {
                        ErrorCode = ErrorCode.InvalidSettingsFile
                    });
                }

                return rootMappingNode.Children.ContainsKey(request.Key)
                    ? Response.Ok(new GetSettingResponse()
                    {
                        Value = rootMappingNode.Children.First(x => x.Key.ToString() == request.Key).Value.ToString()
                    })
                    : Response.Fail(new GetSettingResponse()
                    {
                        ErrorCode = ErrorCode.NoSettingsKeyFound
                    });
            }

            return Response.Fail(new GetSettingResponse
            {
                ErrorCode = ErrorCode.NoSettingsFileFound
            });
        }

        public ClearSettingResponse ClearSetting(ClearSettingRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
            {
                return Response.Fail(new ClearSettingResponse()
                {
                    ErrorCode = ErrorCode.InvalidParameters
                });
            }

            if (!_fileWrapper.FileExists(_settingsFilePath))
            {
                return Response.Fail(new ClearSettingResponse
                {
                    ErrorCode = ErrorCode.NoSettingsFileFound
                });
            }

            var input = new StringReader(_fileWrapper.ReadAllText(_settingsFilePath));

            _yamlStreamWrapper.Load(input);

            YamlMappingNode rootMappingNode = (_yamlStreamWrapper.GetDocuments().Count > 0) ? (YamlMappingNode)_yamlStreamWrapper.GetDocuments()[0].RootNode : default;

            if (rootMappingNode == null)
            {
                return Response.Fail(new ClearSettingResponse()
                {
                    ErrorCode = ErrorCode.InvalidSettingsFile
                });
            }

            if (rootMappingNode.Children.ContainsKey(request.Key))
            {
                rootMappingNode.Children.Remove(request.Key);

                using (TextWriter writer = _fileWrapper.CreateText(_settingsFilePath))
                {
                    _yamlStreamWrapper.Save(writer, false);
                }
            }

            return Response.Ok(new ClearSettingResponse());
        }

        private void InitSettingsFile()
        {
            var input = new StringReader($"---{Environment.NewLine}version: 1{Environment.NewLine}...");

            _yamlStreamWrapper.Load(input);

            using (TextWriter writer = _fileWrapper.CreateText(_settingsFilePath))
            {
                _yamlStreamWrapper.Save(writer, false);
            }
        }
    }
}
