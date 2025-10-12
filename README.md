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



