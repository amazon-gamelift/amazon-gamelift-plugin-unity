// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;

namespace AmazonGameLift.Editor
{
    internal class ScenarioParametersUpdater
    {
        private readonly CoreApi _coreApi;
        private readonly Func<ScenarioParametersEditor> _editorFactory;

        public ScenarioParametersUpdater(CoreApi coreApi, Func<ScenarioParametersEditor> editorFactory)
        {
            _coreApi = coreApi ?? throw new ArgumentNullException(nameof(coreApi));
            _editorFactory = editorFactory ?? throw new ArgumentNullException(nameof(editorFactory));
        }

        /// <summary>
        /// Possible errors: <see cref="ErrorCode.InvalidParameters"/> if <see cref="parameters"/> is null or empty,
        /// and the errors from <see cref="ScenarioParametersEditor"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">For any parameter.</exception>
        public virtual Response Update(string parametersFilePath, IReadOnlyDictionary<string, string> parameters)
        {
            if (parametersFilePath is null)
            {
                throw new ArgumentNullException(nameof(parametersFilePath));
            }

            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            FileReadAllTextResponse fileReadResponse = _coreApi.FileReadAllText(parametersFilePath);

            if (!fileReadResponse.Success)
            {
                return fileReadResponse;
            }

            ScenarioParametersEditor parameterEditor = _editorFactory.Invoke();
            Response readResponse = parameterEditor.ReadParameters(fileReadResponse.Text);

            if (!readResponse.Success)
            {
                return readResponse;
            }

            foreach (KeyValuePair<string, string> pair in parameters)
            {
                Response setParameterResponse = parameterEditor.SetParameter(pair.Key, pair.Value);

                if (!setParameterResponse.Success)
                {
                    return setParameterResponse;
                }
            }

            SaveParametersResponse saveResponse = parameterEditor.SaveParameters();

            if (!saveResponse.Success)
            {
                return saveResponse;
            }

            Response fileWriteResponse = _coreApi.FileWriteAllText(parametersFilePath, saveResponse.SerializedParameters);

            if (!fileWriteResponse.Success)
            {
                return fileWriteResponse;
            }

            return Response.Ok(new Response());
        }
    }
}
