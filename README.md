# 👻 Echoes of Myself (Working Title)

## 📢 Overview
Echoes of Myself is a 3D puzzle-stealth game where your past actions become NPCs    that replay exactly what you did. These "Echoes" can help or hinder. The design of centers on FSM, pathfinding, AI and other course topics.

## 🕹️ Core Gameplay
- Record short action loops by performing them once.
- Echo NPCs spawn later and replay your route & interactions.
- Solve multi-step puzzles by coordinating with your Echoes.
- Avoid guards who react to both you and your Echoes.
- Win by opening the exit of each floor using Echoes for help.
- Lose if you're cuaght or soft-lock the puzzle (quick reset will be available).
- Early rooms teach Echo timing and puzzles; later rooms add light timers and pressure.

## 🧩 Game Type
- Puzzle / Stealth Adventure

## 👥 Player Setup
- Single Player (core).
- Optional local co-op: second player records their own Echoes for combined solutions on co-op only levels.

## 🤖 AI Design
### Echo NPC FSM
- Dorment: spawned, waiting for scheduled start
- Replay: follow recorded moevement & interaction timeline
- Interact: perform recorded button/plate/lever actions
- Expire vanish at loop end or on level reset

### Guard (Enemy) FSM
- Idle: stationed with occational head sweep
- Patrol: patrolling the area using NavMesh with response to sound/vision
- Chase: pursue last seen actor (player or Echo)
- Search: spiral at last known position of actor; timer back to Patrol


## ✍🏼 Scripted Events
- Timed event: Crossing the corridor sensor starts a visible countdown for a short puzzle sequence; if the timer expires, the room hard-resets and the player is returned to the last checkpoint (no softlocks).
- Anti-Echoo Null Field: Entering the Quiet Wing triggers a 30-second anti-echo field that disables Echo recordings and cancels any pending Echo spawns

## 🖼️ Environment
- Compact, modualar floors and rooms in a museum: Entry, Puzzle Hub, Guard Corridor, Exit.
- Interactive props: switches, pressure plates, timed doors, breaker boxes

## 🗓️ Basic Planning Factors
### Assets
- Models/Textures/HDRIs: Kenny (CC0), Poly Haven (CC0), ambientCG (CC0)
- Characters/Animations: Mixamo (idle/walk/run/crouch/interact) on one humaniod rig
- SFX/Music: Kenney Audio (CC0), Freesound (CC0/CC-BY), FreePD/Pixabay for ambient loops.
- VFX/UI: Unity Particle Pack (free), emmissive materials, Google Fonts
- Notes: Maintain an Attribution Log

### Team Information
- Group 23 in Section 001
- Members: Jacob Skiba (20361187)
- Team Roles: Jacob will handle everything in this group, since this is a solo project.

## Assignment 2
### Guard NPC FSM
A transform-driven guard that navigates between waypoints (no NavMesh) and reacts to the player via FOV + distance checks. The guard demonstrates ≥3 states with distinct speeds/colors and deterministic transitions.

### States

Idle: Stand still for idleTime; color = gray; speed = 0.

Patrol: Move between waypoints in a loop; advance when planar distance ≤ arriveDist; color = cyan; speed = patrolSpeed.

Chase: Move toward player while visible (within sightDistance and inside fovDegrees with clear LOS); color = red; speed = chaseSpeed.

Search: Move to lastKnownPos after losing sight; end when planar distance ≤ arriveDist or timeout; color = yellow; speed = searchSpeed.

### Transitions

Idle → Patrol: Idle timer expires.

Patrol → Chase: CanSeePlayer() becomes true.

Patrol → Patrol (next waypoint): Reached current waypoint (planar) or segment timeout.

Chase → Search: CanSeePlayer() false for ≥ 1.5s (lost timer).

Search → Patrol: Reached lastKnownPos (planar) or search timeout.

Any → Chase: If CanSeePlayer() becomes true.

https://drive.google.com/file/d/16pgPByU6Dw9MbR-megUWptXaZk4-OC3_/view?usp=drive_link


## Assignment 3 – What Was Added / Changed

### 1. NavMesh Pathfinding (Engine Pathfinding)

- Added a **NavMeshSurface** to the scene and baked a NavMesh on the level floor so the guard can use engine pathfinding instead of manual movement.
- Added a **NavMeshAgent** component to the guard and removed the old `MoveTowards`-based movement.
- In `GuardFSM`, each state now configures the NavMeshAgent:
  - **Patrol**: `agent.speed = patrolSpeed; agent.SetDestination(waypoints[wpIndex].position);`
  - **Chase**: `agent.speed = chaseSpeed; agent.SetDestination(player.position);`
  - **Search**: `agent.speed = searchSpeed; agent.SetDestination(lastKnownPos);`
- Disabled `agent.autoBraking` and used a small `arriveDist` so the guard moves **smoothly** between waypoints without stopping at each point.

### 2. Smarter Patrol Behavior (Random Route)

- Patrol is no longer a fixed loop (0 → 1 → 2 → …).
- Implemented a `PickNextWaypoint()` function that:
  - Picks a **random next waypoint**.
  - Avoids choosing the **current** index and the **previous** index when there are 3+ waypoints, so patterns like `1 → 2 → 1` are avoided.
- On scene start, the guard chooses a **random initial waypoint**, so the patrol route is less predictable.

### 3. Improved Vision & Decision-Making

- Updated `CanSeePlayer()` so the guard only sees the player if:
  - The player is within `sightDistance`.
  - The player is inside the guard’s **field of view** (`fovDegrees / 2`).
  - A **Physics raycast** from the guard’s eye to the player hits the player **first** (walls/crates with colliders now block vision).
- Added a **close-range override**: if the player is within `catchDistance`, `CanSeePlayer()` always returns true to avoid LOS glitches at point-blank range.
  - Switches from **Chase** to **Search** and moves to that position via NavMesh.

### 4. Integration with Game Over

- In the **Chase** state, if the distance to the player ≤ `catchDistance`, the guard calls:
  ```csharp
  GameManager.Instance.GameOver();

https://drive.google.com/file/d/1U_StEd2vzK1IpmhT8BGcYnotySJLtXLQ/view?usp=drive_link


## Assignment 4 - Multiplayer

### Instructions for Setup/Run

1. Build the Game
   - Go to File -> Build Settings
   - Select SampleScene
   - Click Build
2. Run the Editor Version
   - Press Play in Unity Editor
   - In the in-game UI, clikc Host
   - Move around
3. Connect the Client
   - Open the built game executable
   - Click Client
   - The client connects to the Host and spawnas thier owne controabble character
   - Both players appear now in the same session

https://drive.google.com/file/d/173d-D6Y5z72TgEamXp71z74V4oWjG03J/view?usp=sharing
