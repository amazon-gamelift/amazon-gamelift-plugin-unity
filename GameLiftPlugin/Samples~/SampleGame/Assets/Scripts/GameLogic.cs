// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Threading;
using System.Threading.Tasks;
#endif
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    [SerializeField]
    private Canvas _mainCanvas;

    [SerializeField]
    private StartGameScreen _startGameScreenPrefab;

    private readonly Logger _logger = Logger.SharedInstance;

    private Input _input;

    private Simulation _simulation;

#if UNITY_SERVER
    private NetworkServer _server;
#else
    private NetworkClient _client;
    private bool _startConnection;
    private bool _connected;
    private CancellationTokenSource _connectionCancellationTokenSource;
#endif

    public Render Render { get; private set; }

    public Status Status { get; private set; }

    public GameLog Log { get; private set; }

    public GameLift GameLift { get; private set; }

    public ulong Frame
    {
        set => _simulation.Frame = value;
        get => _simulation.Frame;
    }

    public int PlayerIdx => _simulation.PlayerIdx;

    public bool Playing => _simulation.Playing;

    public bool Authoritative =>
#if UNITY_SERVER
        true;
#else
        _client.Authoritative;
#endif

    public bool GameliftStatus
    {
        get => Status.GameliftStatus;
        private set => Status.GameliftStatus = value;
    }

#if !UNITY_SERVER
    public bool ClientConnected
    {
        get => _connected;
        set
        {
            _connected = value;
            Status.Connected = value;
        }
    }
#endif

    private void Awake()
    {
        _logger.Write(":) GAMELOGIC AWAKE");

        // Get pointers to scripts on other objects
        var gameliftObj = GameObject.Find("/GameLiftStatic");
        Debug.Assert(gameliftObj != null);
        GameLift = gameliftObj.GetComponent<GameLift>();

        if (GameLift == null)
        {
            _logger.Write(":| GAMELIFT CODE NOT AVAILABLE ON GAMELIFTSTATIC OBJECT", LogType.Error);
            return;
        }

        GameLift.ConnectionChangedEvent.AddListener(value => GameliftStatus = value);
    }

    private async void Start()
    {
        _logger.Write(":) GAMELOGIC START");
        // create owned objects
        Log = new GameLog(this, _logger);
        _input = new Input(this);
        Status = new Status(this);
        _simulation = new Simulation(this);
        Render = new Render();
#if UNITY_SERVER
        _server = new NetworkServer(this, GameLift.ServerPort);
        // if running server, GameLift is already initialized before GameLogic.Start is called
        GameliftStatus = GameLift.IsConnected;
        _logger.Write(":) LISTENING ON PORT " + GameLift.ServerPort);
#else
        _client = new NetworkClient(this);
        _connectionCancellationTokenSource = new CancellationTokenSource();

        if (GameLift == null)
        {
            return;
        }

        try
        {
            await Connect(_connectionCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            Log.WriteLine("Connection was cancelled.");
            return;
        }
#endif
        // Start Game
        _input.Start();
        Status.Start();
        _simulation.ResetBoard();
        _simulation.ResetScores();
#if UNITY_SERVER
        // if I am the server, I will send my state to all the clients so they have the same board and RNG state as I do
        _server.TransmitState();
#endif
        Log.WriteLine("HELLO WORLD!");
        Render.SetMessage("HELLO WORLD!");
        ResetMessage(60);
        RenderBoard();
    }

    private void Update()
    {
#if UNITY_SERVER
        _input.Update();
        Attract();
        Frame++;
        _server.Update();
#else
        if (!ClientConnected)
        {
            return;
        }

        _input.Update();
        Attract();
        Frame++;
        _client.Update();
#endif
    }

    private void OnApplicationQuit()
    {
        Log.WriteLine("Application received quit signal");
#if UNITY_SERVER
        _server.Disconnect();
#else
        _connectionCancellationTokenSource.Cancel();
        _connectionCancellationTokenSource.Dispose();
        _client.Disconnect();
#endif
    }

#if !UNITY_SERVER
    private async Task Connect(CancellationToken cancellationToken)
    {
        StartGameScreen startGameScreen = Instantiate(_startGameScreenPrefab, _mainCanvas.transform);
        startGameScreen.SetSubmitAction(StartConnection);
        startGameScreen.Show();

        while (!ClientConnected)
        {
            while (!_startConnection)
            {
                // similar to yield return null
                await Task.Yield();
            }

            _startConnection = false;
            startGameScreen.SetInteractable(false);
            startGameScreen.SetResultText(string.Empty);

            (bool success, ConnectionInfo connectionInfo) = await GameLift.GetConnectionInfo(cancellationToken);

            if (success)
            {
                GameliftStatus = connectionInfo.IpAddress != NetworkClient.LocalHost;
                ClientConnected = _client.TryConnect(connectionInfo);
            }

            if (!ClientConnected)
            {
                startGameScreen.SetInteractable(true);
                startGameScreen.SetResultText(Strings.ErrorStartingGame);
            }
        }

        startGameScreen.Hide();
    }

    private void StartConnection()
    {
        _startConnection = true;
    }
#endif

    private void Attract()
    {
        if (!Playing && Authoritative && Frame % 60 == 0)
        {
            // randomize the board
            _simulation.ResetBoard();
#if UNITY_SERVER
            // if I am the server, I will send my state to all the clients so they have the same board and RNG state as I do
            _server.TransmitState();
#endif
            RenderBoard();
        }
    }

    public void InputEvent(int playerIdx, Chord inputChord)
    {
#if !UNITY_SERVER
        _client.TransmitInput(playerIdx, inputChord);
#endif
        if (_simulation.SimulateOnInput(playerIdx, inputChord))
        {
            RenderBoard();
#if UNITY_SERVER
            _server.TransmitState();
#endif
        }
    }

    public void ShowHighlight(int keyIdx)
    {
        Render.ShowHighlight(keyIdx);
    }

    public void HideHighlight(int keyIdx)
    {
        Render.HideHighlight(keyIdx);
    }

    public void RenderBoard()
    {
        Render.RenderBoard(_simulation, Status);
    }

    public void ResetScore(int playerIdx)
    {
        _simulation.ResetScore(playerIdx);
    }

    public void ZeroScore(int playerIdx)
    {
        _simulation.ZeroScore(playerIdx);
    }

    // Is the specified player in the current game?
    public bool IsConnected(int playerIdx)
    {
#if UNITY_SERVER
        return _server.IsConnected(playerIdx);
#else
        if (playerIdx == 0)
        {
            return true;
        }

        return false;
#endif
    }

    // We pressed RETURN and we are ready to start the game
    public void Ready()
    {
#if !UNITY_SERVER
        if (Authoritative)
        {
            StartGame(); // single player start
            RenderBoard();
        }
        else
        {
            _client.Ready();
        }
#endif
    }

    public void StartGame()
    {
        _simulation.ResetBoard();
        _simulation.ResetScores();
        _simulation.Playing = true;
        Log.WriteLine("GO");
        Render.SetMessage("GO");
        FlashMessage(4, 4);
#if UNITY_SERVER
        _server.TransmitState();
#endif
        RenderBoard();
    }

    public void End()
    {
#if !UNITY_SERVER
        if (Authoritative)
        {
            EndGame(); // single player end
        }
        else
        {
            _client.End();
        }
#endif
    }

    public void EndGame()
    {
        _simulation.Playing = false;
        Log.WriteLine("GAME OVER");
        Render.SetMessage("GAME OVER");
        FlashMessage(10, 1);
#if UNITY_SERVER
        _server.TransmitState();
#endif
    }

    public string GetState(int playerIdx)
    {
        return _simulation.Serialize(playerIdx);
    }

    public void SetState(string state)
    {
        bool priorPlayingState = _simulation.Deserialize(state);

        if (priorPlayingState == false && _simulation.Playing == true)
        {
            StartGame();
        }

        if (priorPlayingState == true && _simulation.Playing == false)
        {
            EndGame();
        }

        RenderBoard();
    }

    public void TransmitLog()
    {
#if UNITY_SERVER
        _server.TransmitLog("THIS IS A TEST");
#endif
    }

    public void ResetMessage(int timeSeconds)
    {
        StartCoroutine(Render.ResetMessage(timeSeconds));
    }

    public void FlashMessage(int timeSeconds, int rate)
    {
        StartCoroutine(Render.FlashMessage(timeSeconds, rate));
    }
}
