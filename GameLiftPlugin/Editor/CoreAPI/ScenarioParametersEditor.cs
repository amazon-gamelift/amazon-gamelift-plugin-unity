// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using AmazonGameLiftPlugin.Core.Shared;
using Newtonsoft.Json;
using CoreErrorCode = AmazonGameLiftPlugin.Core.Shared.ErrorCode;

namespace AmazonGameLift.Editor
{
    public class ScenarioParametersEditor
    {
        public static readonly string ErrorReadingFailed = "ReadingFailed";
        public static readonly string ErrorWritingFailed = "WritingFailed";
        public static readonly string ErrorEditingInProgress = "EditingInProgress";
        public static readonly string ErrorEditingNotInProgress = "EditingNotInProgress";

        private List<ScenarioParameter> _currentParameters;

        /// <summary>
        /// Possible errors: <see cref="ErrorCode.InvalidParameters"/> if <see cref="parameters"/> is null or empty,
        /// <see cref="ErrorReadingFailed"/>, <see cref="ErrorEditingInProgress"/>.
        /// </summary>
        public virtual Response ReadParameters(string serializedParameters)
        {
            if (string.IsNullOrEmpty(serializedParameters))
            {
                return Response.Fail(new Response() { ErrorCode = CoreErrorCode.InvalidParameters });
            }

            if (_currentParameters != null)
            {
                return Response.Fail(new Response() { ErrorCode = ErrorEditingInProgress });
            }

            try
            {
                _currentParameters = JsonConvert.DeserializeObject<List<ScenarioParameter>>(serializedParameters);
                return Response.Ok(new Response());
            }
            catch (JsonException ex)
            {
                var response = new Response()
                {
                    ErrorCode = ErrorReadingFailed,
                    ErrorMessage = ex.Message
                };
                return Response.Fail(response);
            }
        }

        /// <summary>
        /// Possible errors: <see cref="ErrorWritingFailed"/>, <see cref="ErrorEditingNotInProgress"/>.
        /// </summary>
        public virtual SaveParametersResponse SaveParameters()
        {
            if (_currentParameters == null)
            {
                return Response.Fail(new SaveParametersResponse() { ErrorCode = ErrorEditingNotInProgress });
            }

            try
            {
                string serialized = JsonConvert.SerializeObject(_currentParameters);
                _currentParameters = null;
                return Response.Ok(new SaveParametersResponse(serialized));
            }
            catch (JsonException ex)
            {
                var response = new SaveParametersResponse()
                {
                    ErrorCode = ErrorWritingFailed,
                    ErrorMessage = ex.Message
                };
                return Response.Fail(response);
            }
        }

        /// <summary>
        /// Possible errors: <see cref="ErrorCode.InvalidParameters"/> if <see cref="key"/>
        /// or <see cref="value"/> is null or empty, <see cref="ErrorEditingNotInProgress"/>.
        /// </summary>
        public virtual Response SetParameter(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return Response.Fail(new Response() { ErrorCode = CoreErrorCode.InvalidParameters });
            }

            if (_currentParameters == null)
            {
                return Response.Fail(new SaveParametersResponse() { ErrorCode = ErrorEditingNotInProgress });
            }

            ScenarioParameter parameter = _currentParameters.Find(item => item.ParameterKey == key);

            if (parameter == null)
            {
                parameter = new ScenarioParameter()
                {
                    ParameterKey = key,
                    ParameterValue = value
                };
                _currentParameters.Add(parameter);
            }

            parameter.ParameterValue = value;
            return Response.Ok(new Response());
        }
    }
}
