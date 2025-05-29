// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AmazonGameLift.Runtime;
#if !UNITY_SERVER
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.UserIdentityManagement.Models;
#endif
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using AmazonGameLift.Editor;
#endif

public class GameLift : MonoBehaviour
{
    [Serializable]
    public sealed class GameLiftConnectionChangedEvent : UnityEvent<bool> { }

    public GameLiftConnectionChangedEvent ConnectionChangedEvent;
    public static readonly string ClientConfigFilePath = "GameLiftAnywhereClientSettings.yaml";

    [SerializeField]
    private GameLiftClientSettings _gameLiftSettings;

    private readonly Logger _logger = Logger.SharedInstance;
#if UNITY_SERVER
    private GameLiftServer _server;
#else
    private GameLiftClient _client;
#endif

    public int ServerPort { get; private set; }

    public bool IsConnected { get; set;}

    private bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;

        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);

                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }

        return isOk;
    }

    private void Awake()
    {
        _logger.Write(":) GAMELIFT AWAKE");
        // Allow Unity to validate HTTPS SSL certificates; http://stackoverflow.com/questions/4926676
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        ConnectionChangedEvent.AddListener(value => IsConnected = value);
#if UNITY_SERVER
        _logger.Write(":) I AM SERVER");
        _server = new GameLiftServer(this, _logger);
#else
        _logger.Write(":) I AM CLIENT");
        var config = _gameLiftSettings.GetConfiguration();
        if (config.IsGameLiftAnywhere)
        {
#if UNITY_EDITOR
            var stateManager = new StateManager(new CoreApi());
            config.FleetId = stateManager.AnywhereFleetId;
            config.FleetLocation = stateManager.AnywhereFleetLocation;
            config.AwsRegion = stateManager.Region;
            config.ProfileName = stateManager.ProfileName;
#else
            var settings = new Settings<ClientSettingsKeys>(ClientConfigFilePath);
            config.FleetId = settings.GetSetting(ClientSettingsKeys.FleetId).Value;
            config.FleetLocation = settings.GetSetting(ClientSettingsKeys.FleetLocation).Value;
            config.AwsRegion = settings.GetSetting(ClientSettingsKeys.CurrentRegion).Value;
            config.ProfileName = settings.GetSetting(ClientSettingsKeys.CurrentProfileName).Value;
#endif
        }
        var coreApi = new LoggingGameLiftCoreApi(config);
        _client = new GameLiftClient(coreApi, new Delay(), _logger);
#endif
    }

    private void OnDestroy()
    {
#if !UNITY_SERVER
        _client.Dispose();
#endif
    }

#if UNITY_SERVER
    public void StartServer(int port, string authToken = null, string logFilePath = null)
    {
        _logger.Write($":) GAMELIFT StartServer at port {port}.");
        ServerPort = port;
        _server.Start(port, authToken, logFilePath);
    }

    public void TerminateGameSession()
    {
        _server.ProcessEnding();
    }

    // we received a force terminate request. Notify clients and gracefully exit.
    public void TerminateServer()
    {
        Application.Quit();
    }

    public bool AcceptPlayerSession(string playerSessionId)
    {
        return _server.AcceptPlayerSession(playerSessionId);
    }

    public bool RemovePlayerSession(string playerSessionId)
    {
        return _server.RemovePlayerSession(playerSessionId);
    }

#else
#if UNITY_EDITOR
    public void OverrideClient(GameLiftClient value)
    {
        _client = value ?? throw new ArgumentNullException(nameof(value));
    }
#endif

    public SignUpResponse SignUp(string email, string password)
    {
        return _client.Core.SignUp(email, password);
    }

    public ConfirmSignUpResponse ConfirmSignUp(string email, string confirmationCode)
    {
        return _client.Core.ConfirmSignUp(email, confirmationCode);
    }

    public SignInResponse SignIn(string email, string password)
    {
        SignInResponse response = _client.Core.SignIn(email, password);

        if (!response.Success)
        {
            return response;
        }

        _client.ClientCredentials = new ClientCredentials
        {
            AccessToken = response.AccessToken,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken
        };
        return response;
    }

    public SignOutResponse SignOut()
    {
        return _client.SignOut();
    }

    public void SetCredentials(ClientCredentials credentials)
    {
        _client.ClientCredentials = credentials;
    }

    public async Task<(bool success, ConnectionInfo connection)> GetConnectionInfo(CancellationToken cancellationToken = default)
    {
        _logger.Write("CLIENT GetConnectionInfo()");
        (bool success, ConnectionInfo connectionInfo) response = await _client.GetConnectionInfo(cancellationToken);
        _logger.Write($"CLIENT CONNECT INFO: {response.connectionInfo}");
        ConnectionChangedEvent?.Invoke(response.success);
        return response;
    }
#endif
}
