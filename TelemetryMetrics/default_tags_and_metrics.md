# Unity Out of the Box Metrics

## Contents

- [Summary](#summary)
- [How it works](#how-it-works)
- [Strings](#strings)
  - [Global tags](#global-tags)
- [Connection Metrics](#connection-metrics)
- [Metrics](#metrics)
  - [Network](#network)
  - [Performance](#performance)
  - [Memory (Linux only)](#memory-linux-only)
  - [Players and session](#players-and-session)

## Summary

This document lists the metrics and tags emitted by the GameLift Unity SDK Plugin.

## How it works

- Performance metrics are measured every frame via `TimeStats`.
- Memory metrics are optional and sampled on a configurable interval (`MemoryMetricsIntervalSeconds`).
- Network metrics are transport-agnostic and require manual calls into `NetworkStats` from your networking layer.
- All metrics are separately aggregated and flushed every `FlushIntervalMs`.

## Strings

Global tags are assigned to all metrics for the lifetime of the server. You can add custom tags via `GameLiftMetrics.Manager.AddGlobalTag[s]` or via `GameLiftMetricsSettings`.

### Global tags

These are attached to all metrics during initialization.

| Tag key | Description | Source |
| :-- | :-- | :-- |
| `platform` | Constant `unity`. | Static. |
| `build_configuration` | `Debug` or `Release`. | `Debug.isDebugBuild`. |
| `fleet_id` | GameLift Fleet ID. | User specified ID via `GameLiftMetricsSettings` or `GAMELIFT_SDK_FLEET_ID`. |
| `gamelift_process_id` | GameLift Process ID. | User specified ID via `GameLiftMetricsSettings` or `GAMELIFT_SDK_PROCESS_ID`. |
| `build_id` | Build identifier. | `GAMELIFT_BUILD_ID` → `BuildIDOverride` → `Application.version`. |
| `server_id` | Server instance identifier. | User specified ID via `GameLiftMetricsSettings` or `GAMELIFT_SERVER_ID`. |
| `unity_version` | Unity engine version. | `Application.unityVersion`. |
| `process_pid` | OS process ID. | `Process.GetCurrentProcess().Id`. |
| custom | User-defined global tags. | `GameLiftMetricsSettings.GlobalTags`. |

Tip: Add deployment-, map-, or ruleset-specific tags via `GlobalTags` to improve filtering in your dashboards.

## Metrics

### Connection Metrics

| Name | Unit | Description | Source |
| :-- | :-- | :-- | :-- |
| `server_connections` | count | Current number of active network connections. | Call `GameLiftMetrics.Network.SetServerConnections()` or `IncrementServerConnections()` / `DecrementServerConnections()`. |

Notes

- This is not auto-populated. Wire it to your transport (e.g., NGO `OnClientConnectedCallback`/`OnClientDisconnectCallback`).
- Player counts are tracked separately; see [Players and session](#players-and-session).

Available APIs

- `GameLiftMetrics.Network.SetServerConnections(int value)` — set gauge explicitly
- `GameLiftMetrics.Network.IncrementServerConnections()` — increment by 1
- `GameLiftMetrics.Network.DecrementServerConnections()` — decrement by 1

### Network

Manual instrumentation required via `GameLiftMetrics.Network`:

| Name | Unit | Description | Source |
| :-- | :-- | :-- | :-- |
| `bytes_in` | bytes | Bytes received; increment counter by delta. | `GameLiftMetrics.Network.IncrementBytesIn(deltaBytes)` |
| `bytes_out` | bytes | Bytes sent; increment counter by delta. | `GameLiftMetrics.Network.IncrementBytesOut(deltaBytes)` |
| `packets_in` | count | Packets received; increment counter by delta. | `GameLiftMetrics.Network.IncrementPacketsIn(delta)` |
| `packets_out` | count | Packets sent; increment counter by delta. | `GameLiftMetrics.Network.IncrementPacketsOut(delta)` |
| `packets_in_lost` | count | Incoming packets lost; increment by delta. | `GameLiftMetrics.Network.IncrementPacketsInLost(delta)` |
| `packets_out_lost` | count | Outgoing packets lost; increment by delta. | `GameLiftMetrics.Network.IncrementPacketsOutLost(delta)` |

All network metrics are user-driven; no OS interface scraping is performed.

### Performance

Emitted by `TimeStats` every frame with derived percentiles P50, P90, P95 on timers:

| Name | Unit | Description | Source |
| :-- | :-- | :-- | :-- |
| `tick_time` | ms | Processing time per server update (frame). | `TimeStats` |
| `delta_time` | ms | Unity `Time.deltaTime` per frame. | `TimeStats` |
| `fixed_update_time` | ms | Duration of each `FixedUpdate`. | `TimeStats` |
| `up` | state | 1 while running; 0 on shutdown. | `TimeStats` |

Tips

- `tick_time` reports the per-frame server workload on the main thread; watch P90/P95 to catch spikes.

### Memory (Linux only)

Emitted by `MemoryStats` when `EnableMemoryMetrics` is true (Linux builds: `UNITY_STANDALONE_LINUX` or Editor on Linux):

| Name | Unit | Description | Source |
| :-- | :-- | :-- | :-- |
| `mem_physical_total` | bytes | Total system RAM. | `MemoryStats` (`/proc/meminfo`) |
| `mem_physical_available` | bytes | Available system RAM. | `MemoryStats` (`/proc/meminfo`) |
| `mem_physical_used` | bytes | Process RSS (resident set). | `MemoryStats` (`/proc/self/statm`) |
| `mem_virtual_total` | bytes | MemTotal + SwapTotal. | `MemoryStats` (`/proc/meminfo`) |
| `mem_virtual_available` | bytes | Commit limit minus committed. | `MemoryStats` (`/proc/meminfo`) |
| `mem_virtual_used` | bytes | Process virtual size (VmSize). | `MemoryStats` (`/proc/self/statm`) |
| `mem_commit_limit` | bytes | System commit limit. | `MemoryStats` (`/proc/meminfo`) |
| `mem_committed_as` | bytes | Total committed memory. | `MemoryStats` (`/proc/meminfo`) |
| `mem_commit_available` | bytes | Commit limit - committed. | `MemoryStats` (`/proc/meminfo`) |
| `managed_gc_allocated_bytes` | bytes | Managed GC allocated bytes. | `MemoryStats` (`GC.GetTotalMemory(false)`) |

On non-Linux platforms, `MemoryStats` is a no-op stub and emits nothing.

Notes

- `mem_physical_used` is process RSS (resident set), a good proxy for actual RAM footprint.
- `mem_virtual_used` is process address space (VmSize) and can be much larger than RSS.

### Players and session

Managed by `GameLiftMetricsProcessor` when you call the hooks:

| Name | Unit | Description | Source |
| :-- | :-- | :-- | :-- |
| `server_players` | count | Active players; increment/decrement on session accept/remove. | `GameLiftMetricsProcessor.OnPlayerSessionAccepted/Removed()` |
| `server_max_players` | count | Max players for session. | `GameLiftMetricsProcessor.OnGameSessionStarted(GameSession)` |
