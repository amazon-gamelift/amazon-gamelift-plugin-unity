# NEON BLITZ — Multiplayer Tron Arena

> **The greatest smartphone game** — a fast-paced, real-time competitive arena
> game for 2–4 players, built on Amazon GameLift.

---

## Gameplay

Players each control a glowing **neon trail** on a dark grid.  
Every second you survive your trail grows longer behind you.  
**Crash into any trail (including your own) and you're eliminated.**  
The last player alive wins — or the player with the highest score when the
2-minute clock expires.

### Power-ups

Four pickups spawn on the grid during the match:

| Icon | Name   | Effect |
|------|--------|--------|
| ⚡   | Speed  | Move 65 % faster for 6 seconds |
| 🛡   | Shield | Survive one collision |
| ❄️   | Freeze | Halve every other player's speed for 6 seconds |
| 💣   | Bomb   | Instantly destroy all trails within a 5×5 area |

### Scoring

| Event | Points |
|-------|--------|
| Surviving | +1 / second |
| Eliminating another player | +50 (to every survivor) |
| Winning the match | +100 |
| Territory at end (per cell owned) | +2 |

---

## Architecture

```
NeonBlitz/
└── Assets/Scripts/
    ├── Core/
    │   ├── NeonBlitzConfig.cs      ← All tunable constants
    │   └── NeonBlitzState.cs       ← Serialisable game-state data structures
    ├── Simulation/
    │   └── NeonBlitzSimulation.cs  ← Authoritative game logic (SERVER only)
    ├── Network/
    │   ├── NeonBlitzProtocol.cs    ← Length-prefixed TCP framing
    │   ├── NeonBlitzNetworkServer.cs
    │   └── NeonBlitzNetworkClient.cs
    ├── GameLift/
    │   ├── NeonBlitzGameLiftServer.cs  ← GameLift Server SDK wrapper
    │   └── NeonBlitzGameLiftClient.cs  ← GameLift Core API wrapper
    ├── Input/
    │   └── TouchSwipeInput.cs      ← Swipe + WASD/arrow fallback
    ├── Rendering/
    │   └── NeonBlitzRenderer.cs    ← Procedural neon grid (no art assets needed)
    ├── UI/
    │   ├── NeonBlitzHUD.cs         ← In-game timer, scores, power-up bar
    │   ├── LobbyScreen.cs          ← Pre-match player slots + READY button
    │   └── GameOverScreen.cs       ← Final scores, winner banner, Play Again
    ├── Audio/
    │   └── NeonBlitzAudio.cs       ← 100 % procedural sound synthesis
    ├── NeonBlitzManager.cs         ← Central orchestrator (server + client)
    └── NeonBlitzBootstrap.cs       ← Server-build entry point
```

**Data flow (client):**

```
TouchSwipeInput ──► NeonBlitzManager ──► NeonBlitzNetworkClient ──► [TCP] ──► Server
                           ▲                                                      │
                           │                       NeonBlitzSimulation            │
                    STATE snapshot ◄──────────── NeonBlitzNetworkServer ◄─────────┘
                           │
               NeonBlitzRenderer + NeonBlitzHUD
```

---

## Setup

### Client scene
1. Create a new scene (`NeonBlitzGame`).
2. Add an **Empty GameObject** named `NeonBlitzRoot` and attach:
   - `NeonBlitzManager`
   - `NeonBlitzRenderer`
   - `NeonBlitzAudio`
   - `TouchSwipeInput`
   - `NeonBlitzGameLiftClient`
3. Create a **Canvas** and add:
   - `NeonBlitzHUD`
   - `LobbyScreen`
   - `GameOverScreen`
4. Wire all Inspector references on `NeonBlitzManager`.
5. Set **Player Settings → Scripting Backend** to IL2CPP for mobile.
6. Add `NEON_BLITZ` to Scripting Define Symbols if you want to isolate the
   assembly from other samples.

### Server build
1. Enable **Dedicated Server** build target.
2. The `UNITY_SERVER` scripting define is set automatically.
3. `NeonBlitzBootstrap` handles GameLift initialisation; no extra steps needed.

### Local testing (no AWS account required)
- Leave `NeonBlitzGameLiftClient._gameLiftApi` unassigned.
- The client automatically connects to `localhost:7778`.
- Start the server build first, then connect two client windows.

---

## Tuning

Open `NeonBlitzConfig.cs` to adjust:

- **Grid size** — `GridWidth` / `GridHeight` (portrait: keep Height > Width)
- **Base speed** — `BaseMoveInterval` (lower = faster)
- **Match duration** — `GameDuration`
- **Power-up frequency** — `PowerUpSpawnEveryNTicks`
- **Scoring** — all `Score*` constants

---

## Extending

| What to add | Where |
|-------------|-------|
| New power-up | Add value to `NeonPowerUpType`, handle in `NeonBlitzSimulation.ApplyPowerUp` |
| Shrinking grid | Add border-collapse logic in `NeonBlitzSimulation.TickPlaying` |
| AI bots | Implement a server-side bot controller calling `QueueDirection` |
| Leaderboard | Post final scores to Amazon DynamoDB via Lambda after `GameOver` |
| Spectator mode | Add a "spectator" client type that receives STATE but never sends INPUT |
