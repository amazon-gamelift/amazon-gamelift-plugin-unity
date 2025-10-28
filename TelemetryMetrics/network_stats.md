# Network Statistics Instrumentation - A General Guide

## Overview

`NetworkStats` is a lightweight, transport‑agnostic helper that lets any Unity networking stack (NGO, Mirror, FishNet, Photon Fusion, custom UDP/TCP, etc.) report network counters to the Amazon GameLift Metrics pipeline in server builds.

- Local counters you report: bytes in/out, packets in/out, packet loss
- Server reporting: values flow through `GameLiftMetrics` ➜ Amazon Managed Grafana & Cloudwatch
- Usage model: call small increment helpers where you already send/receive data

Related runtime types:

- `GameLiftMetrics` : primary accessor for metrics‑related functionality; exposes `MetricsManager` and the `NetworkStats` singleton
- `NetworkStats` : the increment API you call
- `GameLiftMetricsProcessor` : Unity component that initializes metrics, adds tags, and manages lifecycle

## Documentation

For Amazon GameLift Unity plugin docs, see:
[Amazon GameLift Servers SDK for Unity](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html)

## Supported Versions

The metrics helpers are authored for Unity server builds (`UNITY_SERVER`). They are independent of a specific networking library. The example usage below assumes modern Unity (2021 LTS or later). Adjust to your project conventions.

## Prerequisites

1. Unity project (server build) with the Amazon GameLift Unity plugin installed.
2. Build with the `UNITY_SERVER` define.
3. Ensure metrics are initialized at runtime:
    - Preferred: add a `GameLiftMetricsProcessor` component to a scene that runs on your server build.
    - Or programmatically call `GameLiftMetrics.Initialize(settings)` during server startup.

## How It Works

- `GameLiftMetricsProcessor` (or your bootstrap) initializes `GameLiftMetrics` and the underlying `MetricsManager`.
- `GameLiftMetrics.Network` provides a singleton `NetworkStats` instance once initialized.
- You call call increment methods at the points you transmit or deliver payloads:
  - On send: `IncrementPacketsOut(1)` and `IncrementBytesOut(payloadByteCount)`
  - On receive: `IncrementPacketsIn(1)` and `IncrementBytesIn(payloadByteCount)`
  - Optionally: `IncrementPacketsInLost(delta)` / `IncrementPacketsOutLost(delta)` if your library exposes loss counts
- Counters only accept positive deltas; non‑positive values are ignored.

Data flow (server build):

YourNetworking ➜ NetworkStats increments ➜ GameLiftMetrics ➜ StatsD ➜ Backend/Collector

## Quick Start (Minimal)

1. Ensure metrics are initialized (via `GameLiftMetricsProcessor` is simplest).

2. In your networking code, wire send/receive hooks. Example:

```csharp
#if UNITY_SERVER
using Aws.GameLift.Unity.Metrics;
#endif

// When you SEND a packet or reliable message
void OnSend(byte[] payload /* or NativeArray<byte>, Span<byte>, etc. */)
{
#if UNITY_SERVER
    GameLiftMetrics.Network.IncrementPacketsOut(1);
    GameLiftMetrics.Network.IncrementBytesOut(payload?.Length ?? 0);
#endif
}

// When you DELIVER a received payload to game code
void OnReceive(ReadOnlySpan<byte> payload)
{
#if UNITY_SERVER
    GameLiftMetrics.Network.IncrementBytesIn(payload.Length);
    GameLiftMetrics.Network.IncrementPacketsIn(1);
#endif
}
```

3. If your stack exposes packet loss deltas (via events or stats polling), report them as positive deltas:

```csharp
#if UNITY_SERVER
    // Example deltas computed since last sample
    if (deltaInLost > 0)  GameLiftMetrics.Network.IncrementPacketsInLost(deltaInLost);
    if (deltaOutLost > 0) GameLiftMetrics.Network.IncrementPacketsOutLost(deltaOutLost);
#endif
```

## Troubleshooting

- Metrics not emitted
  - Ensure your server build includes `UNITY_SERVER` and initializes metrics (`GameLiftMetricsProcessor` present in scene or `GameLiftMetrics.Initialize` called).
  - Verify logs for “NetworkStats initialized”.

- Duplicated counts
  - Ensure you only increment once per payload. Don’t count both at raw socket and at higher‑level message dispatch if they represent the same data.

- Loss values ignored
  - Deltas must be positive. Zero/negative values are ignored by design.

## Extending

If your networking stack exposes richer telemetry (queues, resend rates, RTT, channels), you can add new counters or gauges via `GameLiftMetrics.Manager` alongside `NetworkStats`.
