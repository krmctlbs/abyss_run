# Abyss Run

A fast-paced 2D vertical platformer built in Unity. The core objective is to climb to the top of a deep well shaft within a strict 60-second time limit using precision jumping and a magnetic grapple mechanic.

All gameplay systems, custom sound effects, and 2D sprite assets in this project were made entirely from scratch by me.

---

## Gameplay Demo

![Gameplay](gameplay.gif)

---

## Key Mechanics

- **Magnetic Grapple:** Pressing `Left Shift` or clicking automatically latches onto the nearest grapple point within range—no manual mouse aiming required. Releasing it launches the character upward while damping horizontal momentum to prevent slamming directly into walls.
- **Wall Bouncing:** Bouncing off vertical shaft walls consumes wall-jump charges, represented by simple HUD indicator dots. Successfully connecting to a grapple orb mid-air refreshes all charges.
- **Speedrun Timer:** The 60-second countdown only begins on your very first input (movement or grapple). When time runs out, the run resets automatically after a brief delay.
- **Score Tracking:** Keeps track of your best completion time, max altitude reached, and total win count using local save data.

---

## Controls

- **A / D or Arrow Keys:** Move horizontally
- **Space / Up Arrow:** Jump / Wall bounce
- **Left Shift / Mouse Click:** Shoot and hold magnetic grapple (release to launch)
- **R:** Instant quick restart

---

## Architecture Overview

The codebase focuses on simple, readable scripts with zero extraneous logging during normal gameplay:
- `AbyssPlayerController.cs` - Handles horizontal physics, floor checks, and wall-jump charges.
- `MagneticGrapple.cs` - Manages proximity anchor detection and upward boost physics.
- `SpeedrunGameManager.cs` - Controls the first-input start clock, endgame triggers, and high scores.
- `AbyssUIManager.cs` - Updates altimeter HUD, timer text, and the typewriter quote display.
- `JuicyAudioManager.cs` - Plays sound effects and scales audio pitch upward with consecutive air bounces.
- `JuicyCameraFollow.cs` - Smooth camera tracking that locks within the well boundaries and stops shaking when the game is paused.

---

## Getting Started

1. Clone or download this repository.
2. Open the project folder using Unity (tested on 2022.3 LTS and newer).
3. Open `Assets/Scenes/SampleScene.unity` and hit Play.
