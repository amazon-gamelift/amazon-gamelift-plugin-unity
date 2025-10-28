# Amazon GameLift Servers SDK for Unity — Metrics API

The Amazon GameLift Servers SDK Plugin for Unity provides a lightweight metrics system to record server health and gameplay metrics and publish them to AWS GameFleet Servers. These metrics can be visualized in Amazon Managed Grafana or Amazon CloudWatch.

## Integrating GameLift Server Metrics

This section walks you through adding metrics to the same minimal server flow you stand up when you follow the "Testing with Server Build" instructions in the core [Server SDK README](../GameLiftServerSDK/README.md#testing-with-server-build).

### Prerequisites

Before adding metrics you should already have:

1. Installed the Amazon GameLift Servers SDK Unity package (see [README Installation section](../GameLiftServerSDK/README.md#installation)).
2. Imported the log4net sample if you want file logging (helps when validating metrics init).
3. A scene with the test script or your own bootstrap that successfully reaches `ProcessReady` (even if it cannot yet connect to a real fleet).

### 1. Create metrics settings

1. In Unity: Assets > Create > GameLift > Metrics Settings.
2. Select the newly created `GameLiftMetricsSettings` asset.

- We suggest you `Enable Debug Logging` here to assist with log verification.
- Optionally set custom Global Tags (e.g., `map:arena`, `ruleset:deathmatch`) to provide enhanced metadata about your metrics. See the [Global tags](default_tags_and_metrics.md#global-tags) section for the list of built-in tags and additional details.

### 2. Add the processor MonoBehaviour

In the same bootstrap scene you already use for server builds:

1. Add the `GameLiftMetricsProcessor` component to the GameObject containing your GameLift bootstrap script.
2. Assign the `GameLiftMetricsSettings` asset you created to the `GameLiftMetricsProcessor` component.

At runtime this automatically:

- Initializes the metrics system once.
- Registers default/global tags (build, fleet/process IDs when available, etc.).
- Starts time / memory (Linux) stats collection.

### 3. Wire the GameLiftMetricsProcessor into the GameLift bootstap script

1. Import the `Aws.GameLift.Unity.Metrics` namespace.

    `using Aws.GameLift.Unity.Metrics;`

2. Pass the previously created `GameLiftMetricsProcessor` reference to the bootstrap script.

Via a monobehavior:

``` c#
[SerializeField]
private GameLiftMetricsProcessor metricsProcessor;
```

Then assign `GameLiftMetricsProcessor` component to the SerializeField via the Unity Editor.

Hook these where you already handle GameLift Server SDK callbacks so player/session metrics populate:

- On Start Game Session callback: `metricsProcessor.OnGameSessionStarted(gameSession);`
- When accepting a player session: `metricsProcessor.OnPlayerSessionAccepted();`
- When removing a player session (disconnect / leave): `metricsProcessor.OnPlayerSessionRemoved();`
- When your session ends: `metricsProcessor.OnGameSessionEnded();`

Below is an example implementation for `AcceptPlayerSession`:

```csharp
    ...
    [SerializeField]
    private GameLiftMetricsProcessor metricsProcessor;
    ...
    public bool AcceptPlayerSession(string playerSessionId)
    {
        try
        {
            GenericOutcome outcome = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);

            if (outcome.Success)
            {
                _logger.Write(":) Accepted Player Session: " + playerSessionId);
                // Record player session accepted metric
                metricsProcessor?.OnPlayerSessionAccepted();
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
```

### 4. Instrument network statistics

If you have a transport (Unity NGO, Mirror, custom UDP, etc.) update connection counts and traffic.

```csharp
// On new client connection
GameLiftMetrics.Network.IncrementServerConnections();

// On client disconnect
GameLiftMetrics.Network.DecrementServerConnections();

// When you send N bytes (aggregate per tick or per packet as suits your volume)
GameLiftMetrics.Network.IncrementBytesOut(bytesSent);

// When you receive N bytes
GameLiftMetrics.Network.IncrementBytesIn(bytesReceived);

// Packet counts / loss
GameLiftMetrics.Network.IncrementPacketsOut(packetsSentDelta);
GameLiftMetrics.Network.IncrementPacketsIn(packetsReceivedDelta);
GameLiftMetrics.Network.IncrementPacketsInLost(packetsInLostDelta);
GameLiftMetrics.Network.IncrementPacketsOutLost(packetsOutLostDelta);
```

See [network_stats.md](network_stats.md) for an example wrapper and advanced patterns.

### 5. Build & run your server

Follow the same build flow described in the [Server SDK README](../GameLiftServerSDK/README.md#testing-with-server-build). No extra steps are required beyond adding the `GameLiftMetricsProcessor` object to the scene.

### 6. Validate metrics locally

Look for log lines (enable "Enable Debug Logging" in `GameLiftMetricsSettings` for more detail):

- `GameLiftMetrics: Initialize` — metrics pipeline started.
- Periodic flush logs

### 7. Customise metrics and tags

Once validated:

- Add any custom counters/gauges/timers early during bootstrap (see Custom metrics section below).
- Add global tags for build map/variant before creating those metrics (settings asset or `GameLiftMetrics.Manager.AddGlobalTag`).

### 8. Deploy to GameLift & visualize metrics

Most deployment steps are identical to the flow in the [Server SDK README "Testing with Server Build"](../GameLiftServerSDK/README.md#testing-with-server-build) and broader deployment docs. Follow those instructions to package, upload, and run your server build; metrics are already embedded when the scene contains `GameLiftMetricsProcessor` (or you initialized manually in code).

Checklist:

1. Build & upload: Follow README. Ensure the `GameLiftMetricsSettings` asset referenced by the processor is included and the setting `Enable Metrics` is checked.
2. Runtime: Default StatsD target is `localhost:8125` - this will integrate with the Gamelift Servers metric receivers. Override host/port in settings or env if your infrastructure differs.
3. Grafana: Use explore to query metrics like `server_tick_time` and `server_up` which are exported by default from the `GameLiftMetricsProcessor`.
4. Global tags: Confirm the tags you've assigned appear as labels (e.g., `map="arena"`, `fleet_id="..."`).
5. Dashboards: Explore the `Fleet Overview`, `Instance Overview`, `Instance Performance` and `Server Performance` dashboards available in Amazon Managed Grafana.

---

The remainder of this document covers deeper API usage, configuration, and advanced patterns.

## Unity API overview

### GameLiftMetrics (static)

- `Initialize(GameLiftMetricsSettings settings)` / `Shutdown()` — lifecycle.
- `Manager` — access to the underlying `MetricsManager` for custom counters/gauges/timers.
- `Network` — singleton `NetworkStats` for bytes/packets/connection metrics.

### GameLiftMetricsProcessor (MonoBehaviour)

- Initializes metrics, adds global tags, and manages out-of-the-box metrics modules.
- Hooks you should call from your server logic:
  - `OnGameSessionStarted(GameSession)` — sets `session_id` tag and `server_max_players`, resets `server_players`.
  - `OnPlayerSessionAccepted()` — increments `server_players`.
  - `OnPlayerSessionRemoved()` — decrements `server_players`.
  - `OnGameSessionEnded()` — resets players to 0 and flushes.

### Alternative: Manual initialization (without `GameLiftMetricsProcessor`)

If you prefer not to use the Unity `MonoBehaviour` helper (for example, you have a pure headless bootstrap or want tighter control over lifecycle), you can initialize and use the metrics system directly.

Minimal pattern:

```csharp
using Aws.GameLift.Unity.Metrics;
using Aws.GameLift.Server.Model.Metrics;

public static class ServerMetricsBootstrap
{
  public static void Init(GameLiftMetricsSettings settings)
  {
    // Initialize once early (e.g., before creating custom metrics)
    GameLiftMetrics.Initialize(settings);
  }

  public static void Shutdown()
  {
    if (GameLiftMetrics.IsInitialized)
    {
      GameLiftMetrics.Shutdown();
    }
  }
}
```

Usage example (e.g., in a headless bootstrap class):

```csharp
// Load settings (ScriptableObject) or create one in code
var settings = Resources.Load<GameLiftMetricsSettings>("GameLiftMetricsSettings");
ServerMetricsBootstrap.Init(settings);

// On shutdown / process exit / OnDestroy
ServerMetricsBootstrap.Shutdown();
```

Most users should keep using `GameLiftMetricsProcessor` for simplicity; switch to manual initialization only if you need custom lifecycle control.

### Out-of-the-box modules

- TimeStats (always-on): `tick_time`, `delta_time`, `fixed_update_time` (P50/P90/P95) and `up`.
- MemoryStats (Linux only, optional): physical/virtual/commit and managed GC memory.
- NetworkStats (user-instrumented): bytes/packets/loss and `server_connections`.

See the full metric list in [default_tags_and_metrics.md](default_tags_and_metrics.md).

## Custom metrics with GameLiftMetrics.Manager

The Unity plugin initializes and owns the underlying MetricsManager for you. Use `GameLiftMetrics.Manager` to create custom counters, gauges, and timers.

Create metrics once, then reuse

```csharp
using Aws.GameLift.Unity.Metrics;

// e.g., in your server bootstrap after GameLiftMetrics.Initialize(settings)
static class Metrics
{
  public static readonly ICounter ItemsPickedUp =
    GameLiftMetrics.Manager.NewCounter("items_picked_up").Build();

  public static readonly IGauge ActiveQuests =
    GameLiftMetrics.Manager.NewGauge("active_quests").Build();

  public static readonly ITimer PlayerLoadTime =
    GameLiftMetrics.Manager.NewTimer("player_load_time").Build();
}

// Usage anywhere in your code
Metrics.ItemsPickedUp.Increment();
Metrics.ActiveQuests.Increment();
Metrics.PlayerLoadTime.Set(235.0); // milliseconds
```

### Metric types at a glance

- Counters: ever-increasing totals. Good for rates and totals (e.g., `player_joins`).
- Gauges: current value snapshots that go up/down (e.g., `players`).
- Timers: record durations in milliseconds (e.g., `match_duration_ms`).

Counters

```csharp
var deaths = GameLiftMetrics.Manager.NewCounter("player_deaths").Build();
deaths.Increment();
deaths.Add(5);
```

Gauges

```csharp
var serverLoad = GameLiftMetrics.Manager.NewGauge("server_load").Build();
serverLoad.Set(75.5);
serverLoad.Increment();
serverLoad.Add(10);
serverLoad.Reset();
```

Timers (with derived metrics)

```csharp
using Aws.GameLift.Server.Model.Metrics; // for derived metric types

var loopTimer = GameLiftMetrics.Manager
  .NewTimer("game_loop_duration_ms")
  .AddDerivedMetric(new Min())
  .AddDerivedMetric(new Max())
  .AddDerivedMetric(new Count())
  .Build();

// record a measurement
loopTimer.Set(8.7);
```

### Sampling high-frequency metrics

Use sample rates to reduce volume for very high-frequency events.

```csharp
using Aws.GameLift.Server.Model.Metrics; // SampleRate types

var bullets = GameLiftMetrics.Manager
  .NewCounter("bullet_fired")
  .SetSampleRate(new SampleFractional(0.1)) // 10%
  .Build();
```

### Tagging

- Per-metric tags: add dimensions on creation or later:

```csharp
var actions = GameLiftMetrics.Manager
  .NewCounter("player_actions")
  .AddTag("action_type:jump")
  .Build();

actions.AddTag("map:arena");
```

- Global tags: configure once via `GameLiftMetricsSettings.GlobalTags` or add at runtime using `GameLiftMetrics.Manager.AddGlobalTag(s)` before creating metrics. See [default_tags_and_metrics.md](default_tags_and_metrics.md#global-tags).

Notes

- Prefer creating metrics once during initialization and reuse them; avoid creating metrics per event.
- The StatsD host, port and flush interval are driven by `GameLiftMetricsSettings`. See Configuration above.
- All metric operations return a success/failure outcome internally; check logs if `EnableDebugLogging` is on.

## Configuration

### Manage flush interval

- `FlushIntervalMs` controls how often the SDK flushes aggregated metrics to the StatsD endpoint. It’s applied during initialization and used by the underlying MetricsManager.
- Typical values range from 2000–10000 ms. Lower values improve sample granularity at the cost of reduced performance and increased cost. Higher values reduce granularity and delay reporting times.

### Turn metrics off completely

- Set `EnableMetrics = false` in `GameLiftMetricsSettings` to disable all metrics. When disabled, `GameLiftMetricsProcessor` won’t initialize the metrics pipeline and no metrics will be emitted.
- Alternatively, don’t add `GameLiftMetricsProcessor` to your scene and don’t call `GameLiftMetrics.Initialize()` in code.
- To stop metrics at runtime, call `GameLiftMetrics.Shutdown()`. This is handled automaticaly by the `GameLiftMetricsProcessor`.
- Memory metrics can be disabled independently via `EnableMemoryMetrics = false`.

### Full settings reference

All fields available in `GameLiftMetricsSettings`:

- Enablement
  - `EnableMetrics` (bool): Master switch for the entire metrics system. Default: false.
  - `EnableMemoryMetrics` (bool): Enables Linux memory metrics (`/proc`-based). Default: true.
  - `MemoryMetricsIntervalSeconds` (float, 0.1–60): Sampling interval for memory metrics. Default: 1.0.
- StatsD configuration
  - `StatsDHost` (string): StatsD hostname. Env override: `GAMELIFT_STATSD_HOST`. Default: `localhost`.
  - `StatsDPort` (int): StatsD port. Env override: `GAMELIFT_STATSD_PORT`. Default: 8125.
  - `FlushIntervalMs` (int, 1000–60000): Flush cadence for StatsD packets. No env override. Default: 5000.
- Tagging
  - `GlobalTags` (string[]): Custom global tags applied to all emitted metrics (e.g., `map:arena`, `ruleset:ranked`).
- Identifiers (with environment variable overrides)
  - `BuildIDOverride` (string): Build ID when `GAMELIFT_BUILD_ID` is not set; otherwise ignored. Fallback: `Application.version`.
  - `ServerID` (string): Env override `GAMELIFT_SERVER_ID`.
  - `FleetID` (string): Env override `GAMELIFT_SDK_FLEET_ID`. Default serialized value: `UnknownFleetID`.
  - `ProcessID` (string): Env override `GAMELIFT_SDK_PROCESS_ID`. Default serialized value: `UnknownProcessID`.
- Diagnostics
  - `EnableDebugLogging` (bool): Extra logs for metrics lifecycle and errors. Default: false.
- Advanced (code-only)
  - `CustomStatsDClient` (IStatsDClient, non-serialized): Optional injection for a custom client. If set, the metrics manager will use it instead of creating a default client.

## Default tags & Metrics

For the complete list of default tags and out-of-the-box metrics, see the following sections in default_tags_and_metrics:

- [Global tags](default_tags_and_metrics.md#global-tags)
- [Connection Metrics](default_tags_and_metrics.md#connection-metrics)
- [Network](default_tags_and_metrics.md#network)
- [Performance](default_tags_and_metrics.md#performance)
- [Memory (Linux only)](default_tags_and_metrics.md#memory-linux-only)
- [Players and session](default_tags_and_metrics.md#players-and-session)
