# TRAXION — Multiplayer Tron Arena

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
Traxion/
└── Assets/Scripts/
    ├── Core/
    │   ├── TraxionConfig.cs      ← All tunable constants
    │   └── TraxionState.cs       ← Serialisable game-state data structures
    ├── Simulation/
    │   └── TraxionSimulation.cs  ← Authoritative game logic (SERVER only)
    ├── Network/
    │   ├── TraxionProtocol.cs    ← Length-prefixed TCP framing
    │   ├── TraxionNetworkServer.cs
    │   └── TraxionNetworkClient.cs
    ├── GameLift/
    │   ├── TraxionGameLiftServer.cs  ← GameLift Server SDK wrapper
    │   └── TraxionGameLiftClient.cs  ← GameLift Core API wrapper
    ├── Input/
    │   └── TouchSwipeInput.cs      ← Swipe + WASD/arrow fallback
    ├── Rendering/
    │   └── TraxionRenderer.cs    ← Procedural neon grid (no art assets needed)
    ├── UI/
    │   ├── TraxionHUD.cs         ← In-game timer, scores, power-up bar
    │   ├── LobbyScreen.cs          ← Pre-match player slots + READY button
    │   └── GameOverScreen.cs       ← Final scores, winner banner, Play Again
    ├── Audio/
    │   └── TraxionAudio.cs       ← 100 % procedural sound synthesis
    ├── TraxionManager.cs         ← Central orchestrator (server + client)
    └── TraxionBootstrap.cs       ← Server-build entry point
```

**Data flow (client):**

```
TouchSwipeInput ──► TraxionManager ──► TraxionNetworkClient ──► [TCP] ──► Server
                           ▲                                                      │
                           │                       TraxionSimulation            │
                    STATE snapshot ◄──────────── TraxionNetworkServer ◄─────────┘
                           │
               TraxionRenderer + TraxionHUD
```

---

## Setup

### Client scene
1. Create a new scene (`TraxionGame`).
2. Add an **Empty GameObject** named `TraxionRoot` and attach:
   - `TraxionManager`
   - `TraxionRenderer`
   - `TraxionAudio`
   - `TouchSwipeInput`
   - `TraxionGameLiftClient`
3. Create a **Canvas** and add:
   - `TraxionHUD`
   - `LobbyScreen`
   - `GameOverScreen`
4. Wire all Inspector references on `TraxionManager`.
5. Set **Player Settings → Scripting Backend** to IL2CPP for mobile.
6. Add `TRAXION` to Scripting Define Symbols if you want to isolate the
   assembly from other samples.

### Server build
1. Enable **Dedicated Server** build target.
2. The `UNITY_SERVER` scripting define is set automatically.
3. `TraxionBootstrap` handles GameLift initialisation; no extra steps needed.

### Local testing (no AWS account required)
- Leave `TraxionGameLiftClient._gameLiftApi` unassigned.
- The client automatically connects to `localhost:7778`.
- Start the server build first, then connect two client windows.

---

## Tuning

Open `TraxionConfig.cs` to adjust:

- **Grid size** — `GridWidth` / `GridHeight` (portrait: keep Height > Width)
- **Base speed** — `BaseMoveInterval` (lower = faster)
- **Match duration** — `GameDuration`
- **Power-up frequency** — `PowerUpSpawnEveryNTicks`
- **Scoring** — all `Score*` constants

---

## Extending

| What to add | Where |
|-------------|-------|
| New power-up | Add value to `NeonPowerUpType`, handle in `TraxionSimulation.ApplyPowerUp` |
| Shrinking grid | Add border-collapse logic in `TraxionSimulation.TickPlaying` |
| AI bots | Implement a server-side bot controller calling `QueueDirection` |
| Leaderboard | Post final scores to Amazon DynamoDB via Lambda after `GameOver` |
| Spectator mode | Add a "spectator" client type that receives STATE but never sends INPUT |
