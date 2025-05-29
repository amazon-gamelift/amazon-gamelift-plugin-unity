// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER
using System;
using System.Collections.Generic;
using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using AmazonGameLift.Runtime;
using AmazonGameLiftPlugin.Core.CredentialManagement;
using AmazonGameLiftPlugin.Core.CredentialManagement.Models;
using AmazonGameLiftPlugin.Core.Shared.FileSystem;
using Aws.GameLift;
using Aws.GameLift.Server;
using UnityEngine;

#if UNITY_EDITOR
using AmazonGameLift.Editor;
#endif

public class GameLiftServer
{
    public static readonly string ServerConfigFilePath = "GameLiftServerRuntimeSettings.yaml";
    private static readonly string ProcessIDPrefix = "Amazon-GameLift-SampleGame";
    private readonly GameLift _gl;
    private readonly Logger _logger;
    private readonly Settings<ServerSettingsKeys> _settings;
#if UNITY_EDITOR
    private readonly ICredentialsStore _credentialsStore;
#endif

    private int _port;
    private bool _isConnected;
    private string _logFilePath;

    public GameLiftServer(GameLift gl, Logger logger)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = new Settings<ServerSettingsKeys>(ServerConfigFilePath);
#if UNITY_EDITOR
        _credentialsStore = new CredentialsStore(new FileWrapper());
#endif
    }

    // The port must be in the range of open ports for the fleet
    public void Start(int port, string authToken = null, string logFilePath = null)
    {
        _logFilePath = logFilePath;
        _port = port;

        string sdkVersion = GameLiftServerAPI.GetSdkVersion().Result;
        _logger.Write(":) SDK VERSION: " + sdkVersion);

        try
        {
            string websocketUrl;
            string fleetID;
            string computeName;
#if UNITY_EDITOR
            var stateManager = new StateManager(new CoreApi());
            websocketUrl = stateManager.WebSocketUrl;
            fleetID = stateManager.AnywhereFleetId;
            computeName = stateManager.ComputeName;
            var profileName = stateManager.ProfileName;
            var region = stateManager.Region;
            var credentialsResponse =
                _credentialsStore.RetriveAwsCredentials(
                    new RetriveAwsCredentialsRequest() { ProfileName = profileName });
            var gameLiftClient = new AmazonGameLiftClient(credentialsResponse.AccessKey, credentialsResponse.SecretKey,
                RegionEndpoint.GetBySystemName(region));
            authToken ??= gameLiftClient
                .GetComputeAuthToken(new GetComputeAuthTokenRequest { ComputeName = computeName, FleetId = fleetID })
                .AuthToken;
#else
            websocketUrl = _settings.GetSetting(ServerSettingsKeys.WebSocketUrl).Value;
            fleetID = _settings.GetSetting(ServerSettingsKeys.FleetId).Value;
            computeName = _settings.GetSetting(ServerSettingsKeys.ComputeName).Value;
#endif
            var serverParams =
                new ServerParameters(websocketUrl, $"{ProcessIDPrefix}-{Guid.NewGuid()}", computeName, fleetID, authToken);

            GenericOutcome initOutcome = GameLiftServerAPI.InitSDK(serverParams);

            if (initOutcome.Success)
            {
                _logger.Write(":) SERVER IS IN A GAMELIFT FLEET");
                ProcessReady();
            }
            else
            {
                SetConnected(false);

                _logger.Write(":( SERVER NOT IN A FLEET. GameLiftServerAPI.InitSDK() returned " + Environment.NewLine +
                              initOutcome.Error.ErrorMessage);
            }
        }
        catch (Exception e)
        {
            _logger.Write(":( SERVER NOT IN A FLEET. GameLiftServerAPI.InitSDK() exception " + Environment.NewLine +
                          e.Message);
        }
    }

    public bool AcceptPlayerSession(string playerSessionId)
    {
        try
        {
            GenericOutcome outcome = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);

            if (outcome.Success)
            {
                _logger.Write(":) Accepted Player Session: " + playerSessionId);
                return true;
            }
            else
            {
                _logger.Write(":( ACCEPT PLAYER SESSION FAILED. AcceptPlayerSession() returned " +
                              outcome.Error.ToString());
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.Write(":( ACCEPT PLAYER SESSION FAILED. AcceptPlayerSession() exception " + Environment.NewLine +
                          e.Message);
            return false;
        }
    }

    public bool RemovePlayerSession(string playerSessionId)
    {
        try
        {
            GenericOutcome outcome = GameLiftServerAPI.RemovePlayerSession(playerSessionId);

            if (outcome.Success)
            {
                _logger.Write(":) Removed Player Session: " + playerSessionId);
                return true;
            }
            else
            {
                _logger.Write(":( REMOVE PLAYER SESSION FAILED. RemovePlayerSession() returned " +
                              outcome.Error.ToString());
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.Write(":( REMOVE PLAYER SESSION FAILED. RemovePlayerSession() exception " + Environment.NewLine +
                          e.Message);
            return false;
        }
    }

    private void ProcessReady()
    {
        try
        {
            ProcessParameters processParams = CreateProcessParameters();
            GenericOutcome processReadyOutcome = GameLiftServerAPI.ProcessReady(processParams);
            SetConnected(processReadyOutcome.Success);

            if (processReadyOutcome.Success)
            {
                _logger.Write(":) PROCESSREADY SUCCESS.");
            }
            else
            {
                _logger.Write(":( PROCESSREADY FAILED. ProcessReady() returned " +
                              processReadyOutcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            _logger.Write(":( PROCESSREADY FAILED. ProcessReady() exception " + Environment.NewLine + e.Message);
        }
    }

    private ProcessParameters CreateProcessParameters()
    {
        var logParameters = new LogParameters();

        if (_logFilePath != null)
        {
            logParameters.LogPaths = new List<string> { _logFilePath };
        }

        return new ProcessParameters(
            onStartGameSession: gameSession =>
            {
                _logger.Write(":) GAMELIFT SESSION REQUESTED"); //And then do stuff with it maybe.
                try
                {
                    GenericOutcome outcome = GameLiftServerAPI.ActivateGameSession();

                    if (outcome.Success)
                    {
                        _logger.Write(":) GAME SESSION ACTIVATED");
                    }
                    else
                    {
                        _logger.Write(":( GAME SESSION ACTIVATION FAILED. ActivateGameSession() returned " +
                                      outcome.Error.ToString());
                    }
                }
                catch (Exception e)
                {
                    _logger.Write(":( GAME SESSION ACTIVATION FAILED. ActivateGameSession() exception " +
                                  Environment.NewLine + e.Message);
                }
            },
            onUpdateGameSession: session =>
            {
                _logger.Write(":) GAME SESSION UPDATED");
                _logger.Write(":) UPDATE REASON - " + session.UpdateReason);
                _logger.Write(":) GameSession: " + JsonUtility.ToJson(session.GameSession));
            },
            onProcessTerminate: () =>
            {
                _logger.Write(":| GAMELIFT PROCESS TERMINATION REQUESTED (OK BYE)");
                _gl.TerminateServer();
            },
            onHealthCheck: () =>
            {
                _logger.Write(":) GAMELIFT HEALTH CHECK REQUESTED (HEALTHY)");
                return true;
            },
            // tell the GameLift service which port to connect to this process on.
            // unless we manage this there can only be one process per server.
            _port,
            logParameters);
    }

    public void ProcessEnding()
    {
        try
        {
            GenericOutcome outcome = GameLiftServerAPI.ProcessEnding();

            if (outcome.Success)
            {
                _logger.Write(":) PROCESSENDING");
            }
            else
            {
                _logger.Write(":( PROCESSENDING FAILED. ProcessEnding() returned " + outcome.Error.ToString());
            }
        }
        catch (Exception e)
        {
            _logger.Write(":( PROCESSENDING FAILED. ProcessEnding() exception " + Environment.NewLine + e.Message);
        }
        finally
        {
            GameLiftServerAPI.Destroy();
        }
    }

    private void SetConnected(bool value)
    {
        if (_isConnected == value)
        {
            return;
        }

        _isConnected = value;
        _gl.ConnectionChangedEvent?.Invoke(value);
    }
}

#endif
