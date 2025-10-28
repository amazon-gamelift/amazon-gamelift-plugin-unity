# StatsTransport for Netcode for GameObjects

## Overview

The StatsTransport sample is a lightweight “decorator” transport for Netcode for GameObjects (NGO) that wraps your real transport (for example, Unity Transport) to collect basic network statistics and, on server builds, report those statistics to the Amazon GameLift Metrics plugin.

- Local tracking: total bytes in/out, total packets in/out
- Server reporting: forwards those counts to GameLift Metrics (when available)
- Placement: sits between NetworkManager and your real transport

## Documentation

For Amazon GameLift Unity plugin documentation, see the Amazon GameLift Servers documentation:
[Amazon GameLift Servers SDK for Unity](https://docs.aws.amazon.com/gamelift/latest/developerguide/unity-plug-in.html)

## Supported Versions

This sample was built targeting Unity 6.0 and Netcode for GameObjects (NGO) 2.11. You may need small modifications for other versions or setups; this serves purely as an example of how one could integrate with NGO and GameLift Metrics.

## Prerequisites

1. A Unity project using Netcode for GameObjects (NGO) and a transport (for example, Unity Transport).
2. To report server metrics, the Amazon GameLift Unity plugin must be installed and initialized in server builds.
3. Server builds should be compiled with the `UNITY_SERVER` define.

## How It Works

- `StatsTransport` derives from NGO `NetworkTransport` and holds an inner `NetworkTransport` (usually `UnityTransport`).
- All required `NetworkTransport` calls are proxied to the inner transport.
- On send, it increments bytes/packet counters before forwarding to the inner transport.
- On receive, it increments counters when the inner transport surfaces data using `OnTransportEvent` (event-based counting).
- When compiled with `UNITY_SERVER` and the GameLift Metrics plugin is initialized, the sample also forwards byte/packet totals to `GameLiftMetrics.Network`.

Data flow:

NetworkManager ⇄ StatsTransport ⇄ UnityTransport ⇄ Socket

## Installation

1. Add the script

   - Place `StatsTransport.cs` on the same GameObject as your `NetworkManager` and your real transport (for example, `UnityTransport`).

2. Assign the inner transport

   - In the Inspector, set `InnerTransport` to your real transport component.
   - If left unset, the sample attempts to auto-detect an appropriate transport on the same GameObject.

3. Set StatsTransport as the active transport

   - In `NetworkManager`, set `NetworkConfig ➜ Network Transport` to the `StatsTransport` component.

## Extending Metrics (Optional)

`NetworkStats` in the Amazon GameLift Unity plugin exposes additional counters for packet loss:

- `packets_in_lost`
- `packets_out_lost`

If your transport exposes loss information (statistics or callbacks), you can report packet loss to GameLift Metrics when running a server build.

```csharp
#if UNITY_SERVER
if (GameLiftMetrics.IsInitialized && GameLiftMetrics.Network != null)
{
  // Report positive deltas only; each lost packet should be counted once
  GameLiftMetrics.Network.IncrementPacketsInLost(deltaInLost);
  GameLiftMetrics.Network.IncrementPacketsOutLost(deltaOutLost);
}
#endif
```

Notes:

1. Only report positive deltas. Non‑positive values are ignored by `NetworkStats`.
2. Avoid double‑counting if your transport reports loss via both events and polling.
3. If your transport does not expose loss metrics, you may approximate loss using sequence numbers and gap detection (outside the scope of this sample).

## Troubleshooting

- InnerTransport is null
  - Ensure a real transport (for example, `UnityTransport`) exists on the same GameObject and is assigned. The sample attempts to auto-detect, but explicit assignment is recommended.

- Metrics are not visible
  - Confirm you built with `UNITY_SERVER`.
  - Ensure `GameLiftMetricsProcessor` is active and initialized with valid settings.
  - Check logs for initialization messages and exceptions.

- Duplicate byte counts
  - This sample only counts via `OnTransportEvent`. If you add counting to `PollEvent`, remove the event-based counting to avoid double-counting.

---

This sample is intentionally small and opinionated for clarity. Adapt it to your production needs (error handling, telemetry cardinality, and log policy).
