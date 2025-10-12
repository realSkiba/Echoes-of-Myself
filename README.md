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
#### FSM Diagram
        (timer idleTime)
Idle  ------------------>  Patrol  --(player seen)-->  Chase
 ^                                                       |
 |                                                       |  (lost sight ≥ 1.5s) 
 |                                                       v
 +-----<--(arrive at lastKnown OR search timeout)--  Search  <--+
                      ^                                      |   |
                      +-----------(lost sight)---------------+   |
#### States (behavior & visible cues)

Idle
Behavior: Stands still for idleTime seconds.
Cues: Material gray, speed 0.

Patrol
Behavior: Moves between waypoints[wpIndex] in a loop; advances when planar distance ≤ arriveDist or a safety timeout elapses.
Cues: Material cyan, speed = patrolSpeed.

Chase
Behavior: Moves toward player while the player is within FOV (fovDegrees/2) and sightDistance, with optional raycast occlusion. If sight is lost for ≥ 1.5s, switches to Search using lastKnownPos.
Cues: Material red, speed = chaseSpeed.

Search
Behavior: Moves to lastKnownPos; if it arrives (planar) or the search timer expires, returns to Patrol. Seeing the player at any time returns to Chase.
Cues: Material yellow, speed = searchSpeed.

Transitions (conditions)

Idle → Patrol: Time.time ≥ stateEnd (idle timer).

Patrol → Chase: CanSeePlayer() == true.

Patrol → Patrol(next waypoint): PlanarDistance(guard, waypoint) ≤ arriveDist OR segment timeout.

Chase → Search: CanSeePlayer() == false for ≥ 1.5s (lost timer).

Chase → Chase (stay): CanSeePlayer() == true.

Search → Patrol: PlanarDistance(guard, lastKnownPos) ≤ arriveDist OR search timeout.

Any → Chase: If CanSeePlayer() == true (edge-case guard).

Detection rule (CanSeePlayer)

Range: distance ≤ sightDistance.

FOV: Vector3.Angle(forward, toPlayer) ≤ fovDegrees * 0.5.

Occlusion (optional): raycast from eye to player; require clear hit when occluders LayerMask is set.

Tuning (example values used)

idleTime = 2.0

arriveDist = 0.25–0.5 (planar / XZ only)

patrolSpeed = 2.0, searchSpeed = 2.2, chaseSpeed = 4.0

sightDistance = 12–20, fovDegrees = 90–120 (wider for demo)

Visible state colors: Idle=gray, Patrol=cyan, Chase=red, Search=yellow

Grader visibility checklist

Guard loops waypoints (clearly moves past WP0 to WP1… and wraps).

Approaching in front triggers Chase (red); running out of view causes Search (yellow); search completes and returns to Patrol (cyan).

Color and speed differences make states obvious on video.

(Optional: a runtime vision cone is rendered by VisionConeMesh for clarity; color shifts to red when the player is seen.)



