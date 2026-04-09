// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Central MonoBehaviour that owns every other Neon Blitz subsystem.
///
/// SERVER build:   drives simulation tick, owns NeonBlitzNetworkServer.
/// CLIENT build:   drives UI state machine, owns NeonBlitzNetworkClient,
///                 receives state snapshots from the server, feeds them to
///                 renderer + HUD.
///
/// Both builds reference NeonBlitzGameLiftServer / NeonBlitzGameLiftClient
/// respectively (the unused half is compiled away by #if guards).
/// </summary>
public class NeonBlitzManager : MonoBehaviour
{
    // ── Inspector — shared ────────────────────────────────────────────────────
    [Header("Scene references")]
    [SerializeField] private Canvas _mainCanvas;

    // ── Inspector — client only ───────────────────────────────────────────────
#if !UNITY_SERVER
    [SerializeField] private NeonBlitzRenderer  _renderer;
    [SerializeField] private NeonBlitzHUD       _hud;
    [SerializeField] private LobbyScreen        _lobbyScreen;
    [SerializeField] private GameOverScreen     _gameOverScreen;
    [SerializeField] private TouchSwipeInput    _swipeInput;
    [SerializeField] private NeonBlitzAudio     _audio;
    [SerializeField] private NeonBlitzGameLiftClient _gameLiftClient;
#endif

    // ── Inspector — server only ───────────────────────────────────────────────
#if UNITY_SERVER
    [SerializeField] private NeonBlitzGameLiftServer _gameLiftServer;
#endif

    // ── Runtime — server ──────────────────────────────────────────────────────
#if UNITY_SERVER
    private NeonBlitzSimulation    _sim;
    private NeonBlitzNetworkServer _server;
#endif

    // ── Runtime — client ──────────────────────────────────────────────────────
#if !UNITY_SERVER
    private NeonBlitzNetworkClient         _client;
    private NeonBlitzGameState             _latestState;
    private int                            _localPlayerId = -1;
    private CancellationTokenSource        _cts;
    private NeonGamePhase                  _prevPhase = NeonGamePhase.Lobby;
#endif

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
#if UNITY_SERVER
        StartServer();
#else
        StartClient();
#endif
    }

    private void Update()
    {
#if UNITY_SERVER
        if (_server != null)
            _server.Update(Time.deltaTime);
#else
        _client?.Update();
        if (_latestState != null)
            RenderFrame();
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_SERVER
        _server?.Shutdown();
#else
        _cts?.Cancel();
        _client?.Disconnect();
#endif
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  SERVER SIDE
    // ═════════════════════════════════════════════════════════════════════════

#if UNITY_SERVER

    private void StartServer()
    {
        int port = NeonBlitzConfig.DefaultPort;

        // Initialise GameLift (optional — gracefully no-ops when not available)
        if (_gameLiftServer != null && _gameLiftServer.Initialise())
            port = _gameLiftServer.ServerPort;

        _sim    = new NeonBlitzSimulation();
        _server = new NeonBlitzNetworkServer(this, _sim, port);

        Debug.Log("[NeonBlitz Manager] Server started");
    }

    // Called by NeonBlitzNetworkServer
    public bool AcceptPlayerSession(string sessionId) =>
        _gameLiftServer != null ? _gameLiftServer.AcceptPlayerSession(sessionId) : true;

    public void RemovePlayerSession(string sessionId) =>
        _gameLiftServer?.RemovePlayerSession(sessionId);

    public void TerminateSession() =>
        _gameLiftServer?.TerminateGameSession();

#endif // UNITY_SERVER

    // ═════════════════════════════════════════════════════════════════════════
    //  CLIENT SIDE
    // ═════════════════════════════════════════════════════════════════════════

#if !UNITY_SERVER

    private async void StartClient()
    {
        _cts    = new CancellationTokenSource();
        _client = new NeonBlitzNetworkClient(this);

        SetupLobbyScreen();

        await ConnectLoop(_cts.Token);
    }

    // ── Connection flow ───────────────────────────────────────────────────────

    private void SetupLobbyScreen()
    {
        if (_lobbyScreen == null) return;
        _lobbyScreen.OnReadyPressed += OnLobbyReady;
        _lobbyScreen.Show();
        _lobbyScreen.SetStatusText("Connecting to GameLift…");
    }

    private async Task ConnectLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            _lobbyScreen?.SetReadyInteractable(false);
            _lobbyScreen?.SetStatusText("Finding a match…");

            string playerName = "Player";
            var (ok, info) = await _gameLiftClient.GetConnectionInfo(playerName, ct);

            if (!ok)
            {
                _lobbyScreen?.SetStatusText("Could not find a server. Retrying…", Color.red);
                await Task.Delay(3000, ct);
                continue;
            }

            bool connected = _client.TryConnect(info);
            if (!connected)
            {
                _lobbyScreen?.SetStatusText("Connection failed. Retrying…", Color.red);
                await Task.Delay(2000, ct);
                continue;
            }

            _localPlayerId = -1;   // assigned once first STATE arrives
            _lobbyScreen?.SetStatusText("Connected! Waiting for players…");
            _lobbyScreen?.SetReadyInteractable(true);
            break;
        }
    }

    private void OnLobbyReady(string playerName)
    {
        _client?.SendReady();
    }

    // ── State from server ─────────────────────────────────────────────────────

    /// <summary>Called by NeonBlitzNetworkClient when a STATE message arrives.</summary>
    public void ApplyServerState(string json)
    {
        var state = JsonUtility.FromJson<NeonBlitzGameState>(json);
        if (state == null) return;

        // First state tells us our player ID
        if (_localPlayerId < 0)
            _localPlayerId = state.localPlayerId;

        _latestState = state;
        HandlePhaseTransition(state);
    }

    private void HandlePhaseTransition(NeonBlitzGameState state)
    {
        if (state.phase == _prevPhase) return;
        var oldPhase = _prevPhase;
        _prevPhase   = state.phase;

        switch (state.phase)
        {
            case NeonGamePhase.Countdown:
                _lobbyScreen?.Hide();
                _audio?.PlayCountdown();
                break;

            case NeonGamePhase.Playing:
                _audio?.PlayMatchStart();
                SetupSwipeInput();
                break;

            case NeonGamePhase.GameOver:
                _swipeInput?.gameObject.SetActive(false);
                _audio?.PlayMatchEnd();
                ShowGameOver(state);
                break;
        }
    }

    // ── Swipe input ───────────────────────────────────────────────────────────

    private void SetupSwipeInput()
    {
        if (_swipeInput == null) return;
        _swipeInput.gameObject.SetActive(true);
        _swipeInput.OnSwipe -= OnSwipe;   // guard against double-subscribe
        _swipeInput.OnSwipe += OnSwipe;
    }

    private void OnSwipe(NeonDirection dir)
    {
        if (_latestState?.phase != NeonGamePhase.Playing) return;
        if (_localPlayerId < 0) return;

        var input = new NeonInputMessage
        {
            playerId  = _localPlayerId,
            direction = dir,
        };
        _client?.SendInput(input);
        _audio?.PlayMove();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    private void RenderFrame()
    {
        _renderer?.Render(_latestState);
        _hud?.Refresh(_latestState);
    }

    // ── Game-over screen ──────────────────────────────────────────────────────

    private void ShowGameOver(NeonBlitzGameState state)
    {
        if (_gameOverScreen == null) return;
        _gameOverScreen.Populate(state);
        _gameOverScreen.OnPlayAgain -= PlayAgain;
        _gameOverScreen.OnPlayAgain += PlayAgain;
        _gameOverScreen.OnMainMenu  -= GoMainMenu;
        _gameOverScreen.OnMainMenu  += GoMainMenu;
        _gameOverScreen.Show();
    }

    private void PlayAgain()
    {
        _gameOverScreen?.Hide();
        _client?.SendEnd();
        _client?.Disconnect();
        // Reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void GoMainMenu()
    {
        _client?.SendEnd();
        _client?.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ── Connection lost callbacks ─────────────────────────────────────────────

    public void OnConnectionRejected(string reason)
    {
        Debug.LogWarning($"[NeonBlitz Manager] Rejected: {reason}");
        _lobbyScreen?.Show();
        _lobbyScreen?.SetStatusText($"Rejected: {reason}", Color.red);
        _lobbyScreen?.SetReadyInteractable(true);
    }

    public void OnServerDisconnected()
    {
        if (_latestState?.phase == NeonGamePhase.Playing)
        {
            // Unexpected disconnect mid-game — show lobby again
            _lobbyScreen?.Show();
            _lobbyScreen?.SetStatusText("Disconnected from server.", Color.red);
        }
    }

#endif // !UNITY_SERVER
}
