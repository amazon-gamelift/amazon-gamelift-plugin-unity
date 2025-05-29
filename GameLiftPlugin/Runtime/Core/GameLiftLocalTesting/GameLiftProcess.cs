// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AmazonGameLiftPlugin.Core.GameLiftLocalTesting.Models;
using AmazonGameLiftPlugin.Core.Shared;
using AmazonGameLiftPlugin.Core.Shared.Logging;
using AmazonGameLiftPlugin.Core.Shared.ProcessManagement;

namespace AmazonGameLiftPlugin.Core.GameLiftLocalTesting
{
    public class GameLiftProcess : IGameLiftProcess
    {
        private readonly IProcessWrapper _processWrapper;

        public GameLiftProcess(IProcessWrapper processWrapper)
        {
            _processWrapper = processWrapper;
        }

        // This method starts GameLift Local
        public StartResponse Start(StartRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.GameLiftLocalFilePath))
                {
                    return Response.Fail(new StartResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                string arg = FormatCommand(request);

                int processId;

                if (request.LocalOperatingSystem == LocalOperatingSystem.MAC_OS)
                {
                    // Starts a bash process to run an Apple script, which activates a new Terminal App window,
                    // and runs the GameLift local jar.
                    string activateTerminalScript = $"tell application \\\"Terminal\\\" to activate";
                    string setGameLiftLocalFilePath = $"set GameLiftLocalFilePathEnvVar to \\\"{request.GameLiftLocalFilePath}\\\"";
                    string runGameLiftLocalJarScript = $"\\\"java -jar \\\" & quoted form of GameLiftLocalFilePathEnvVar & \\\" -p {request.Port.ToString()}\\\"";
                    string runGameLiftLocal = $"tell application \\\"Terminal\\\" to do script {runGameLiftLocalJarScript}";
                    string osaScript = $"osascript -e \'{activateTerminalScript}\' -e \'{setGameLiftLocalFilePath}\' -e \'{runGameLiftLocal}\'";
                    string bashCommand = $" -c \"{osaScript}\"";

                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = "/bin/bash",
                        CreateNoWindow = false,
                        Arguments = bashCommand,
                        WorkingDirectory = Path.GetDirectoryName(request.GameLiftLocalFilePath)
                    };

                    processId = ExecuteMacOsTerminalCommand(processStartInfo);
                }
                else
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = "java",
                        Arguments = arg,
                        WorkingDirectory = Path.GetDirectoryName(request.GameLiftLocalFilePath)
                    };

                    processId = _processWrapper.Start(processStartInfo);
                }

                return Response.Ok(new StartResponse
                {
                    ProcessId = processId
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StartResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        private void CloseTerminalWindow(int windowId)
        {
            // Starts a bash process to run an Apple script, which tells Terminal App to close a specified window
            string closeWindowScript = $"tell application \\\"Terminal\\\" to close window id {windowId}";
            string osaScript = $"osascript -e \'{closeWindowScript}\'";
            string bashCommand = $" -c \"{osaScript}\"";

            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "/bin/bash",
                CreateNoWindow = false,
                Arguments = bashCommand
            };

            _processWrapper.Start(processStartInfo);
        }

        private int ExecuteMacOsTerminalCommand(ProcessStartInfo processStartInfo)
        {
            (int bashId, string output) = _processWrapper.GetProcessIdAndStandardOutput(processStartInfo);
            // When running script in Mac OS Terminal App via oascript, Terminal returns the window id as a response,
            // e.g. "tab 1 of window id 62198". This will be used as the id when we terminate the window.
            var match = Regex.Match(output, "window id (\\d+)");
            if (match.Success)
            {
                // Return the Terminal app window id
                return Int32.Parse(match.Groups[1].Value);
            }
            else
            {
                // Pass back the /bin/bash execution process id as an identifier instead
                return bashId;
            }
        }

        private string FormatCommand(StartRequest request)
        {
            var arguments = new List<string>();

            arguments.Add("-jar");
            arguments.Add(EscapeArgument(request.GameLiftLocalFilePath));
            arguments.Add("-p");
            arguments.Add(EscapeArgument(request.Port.ToString()));

            return string.Join(" ", arguments);
        }

        private string EscapeArgument(string argument)
        {
            return "\"" + Regex.Replace(argument, @"(\\+)$", @"$1$1") + "\"";
        }

        public StopResponse Stop(StopRequest request)
        {
            try
            {
                if (request.LocalOperatingSystem == LocalOperatingSystem.MAC_OS) {
                    CloseTerminalWindow(request.ProcessId);
                } else {
                    _processWrapper.Kill(request.ProcessId);
                }

                return Response.Ok(new StopResponse());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new StopResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }

        public RunLocalServerResponse RunLocalServer(RunLocalServerRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.FilePath) || string.IsNullOrWhiteSpace(request.ApplicationProductName))
                {
                    return Response.Fail(new RunLocalServerResponse
                    {
                        ErrorCode = ErrorCode.InvalidParameters
                    });
                }

                int processId;

                if (request.LocalOperatingSystem == LocalOperatingSystem.MAC_OS)
                {
                    // Starts a bash process to run an Apple script, which activates a new Terminal App window,
                    // and runs the game server executable in the Unity compiled .app file
                    string activateTerminalScript = $"tell application \\\"Terminal\\\" to activate";
                    string setGameServerFilePath = $"set GameServerFilePathEnvVar to \\\"{request.FilePath}\\\"";
                    string runGameServerScript = $"GameServerFilePathEnvVar";
                    string runGameServer = $"tell application \\\"Terminal\\\" to do script {runGameServerScript}";
                    string osaScript = $"osascript -e \'{activateTerminalScript}\' -e \'{setGameServerFilePath}\' -e \'{runGameServer}\'";
                    string bashCommand = $" -c \"{osaScript}\"";

                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        FileName = "/bin/bash",
                        CreateNoWindow = false,
                        Arguments = bashCommand
                    };

                    processId = ExecuteMacOsTerminalCommand(processStartInfo);
                }
                else
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = request.ShowWindow,
                        FileName = request.FilePath
                    };

                    processId = _processWrapper.Start(processStartInfo);
                }

                return Response.Ok(new RunLocalServerResponse
                {
                    ProcessId = processId
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);

                return Response.Fail(new RunLocalServerResponse
                {
                    ErrorCode = ErrorCode.UnknownError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
