# 🕹️ Neon Flux Arkanoid

**Neon Flux Arkanoid** is a modern, high-quality, 2D arcade breakout game built in **Unity** utilizing the **Universal Render Pipeline (URP)** and the modern **Input System**. It features a striking retro-synthwave neon aesthetic, dynamic squash-and-stretch animations, screen shake, floating popup scores, procedural sound synthesis, and a modular layout system.

---

## 🚀 Quick Start Setup

To play the game:
1. Open the project folder in the **Unity Editor** (compatible with Unity 2022.3 LTS and newer).
2. Allow the C# scripts to compile.
3. In the top Unity Editor toolbar, select:
   **`Tools > Arkanoid > Setup Game Scene`**
   *This utility automatically creates `Assets/Scenes/Arkanoid.unity`, populates the scene with all boundaries, camera setups, URP Bloom volume configurations, UI overlays, and establishes the runtime script linkages.*
4. Open the created scene: **`Assets/Scenes/Arkanoid.unity`**.
5. Press **Play**!

---

## 🕹️ Controls
* **Paddle Movement**: Move the **Mouse** (smooth horizontal position tracking) or press **A/D** / **Left/Right Arrow Keys** to move.
* **Launch / Shoot**: Press **Left Mouse Click** or the **Spacebar** to:
  * Launch the ball from the paddle at start-of-life or when caught.
  * Fire laser beams when the **Laser (L)** power-up is active.
* **Pause / Unpause**: Press **Escape** or **P** at any time to toggle the pause menu.
* **Menu Navigation**: Press the **Up/Down Arrow Keys** to highlight buttons, and press **Enter** or **Spacebar** to confirm selection.

---

## 🤫 Cheat Codes (Dev Mode)
During gameplay, hold the **`C`** key and press one of the following keys to trigger developer cheats:

### ⏭️ Level Skipping
* **`C` + `1`**: Skip to Level 1
* **`C` + `2`**: Skip to Level 2
* **`C` + `3`**: Skip to Level 3
* **`C` + `4`**: Skip to Level 4
*(If additional levels are added, they can be skipped to using `C` + `5` through `C` + `9`)*

### ⚡ Instant Power-ups
* **`C` + `Q`**: **Expand** (paddle width expands)
* **`C` + `W`**: **Laser** (paddle can shoot laser beams)
* **`C` + `E`**: **Catch** (balls stick to paddle)
* **`C` + `R`**: **Slow** (slows down balls)
* **`C` + `T`**: **Pierce** (balls pierce through bricks)
* **`C` + `Y`**: **Triple** (splits balls into three)
* **`C` + `U`**: **Life** (grants an extra life)

---

## 🌟 Key Features & Tech Stack

* **Procedural Synth Audio**: Sound effects (bounces, hits, powerups, lasers, deaths, wins, ball launch, catching balls, power-up spawns, UI navigation, and UI selection) are mathematically synthesized on-the-fly in C# using wave formulas (sine, square, frequency sweeps). **No external audio files required!**
* **Neon Grid Background**: Draws a scrolling futuristic vector highway using direct GL rendering, adding depth without loading bulky image assets.
* **Vector Aesthetic & Juice**: 
  * Glow trails behind balls (`TrailRenderer`).
  * Dynamic squash-and-stretch scaling animations on ball impact.
  * Physics-based programmatic debris particle bursts matching brick colors.
  * Camera shake on deaths and hard impacts.
  * Floating "+100" score indicators that drift upward and fade out.
* **Custom Physics Bouncing**: Bypasses classic Unity physics limitations using manual reflection vectors (`Vector2.Reflect`) and dot product filtering. The ball will never clip, bounce horizontally forever, or get stuck in colliders.

---

## 📦 Core Architecture & Scripts

The code is organized cleanly within `Assets/Scripts/`:

| Script | Location | Purpose |
| :--- | :--- | :--- |
| **`GameManager`** | `Core/` | Controls the main game state loop (Menu, Play, Pause, GameOver, Victory), tracking scores, high scores, and active powerups. |
| **`LevelManager`** | `Core/` | Generates grid levels dynamically from ASCII layout templates, spawning bricks and managing stage completion. |
| **`SoundManager`** | `Core/` | Procedurally synthesizes and plays audio clips at runtime using mathematical wave calculations. |
| **`PaddleController`**| `Gameplay/` | Handles keyboard/mouse movement constraints, squash/stretch reactions, and powerup states (Laser, Catch, Expand). |
| **`BallController`** | `Gameplay/` | Directs ball movement, speed clamping, custom paddle-bounce angle offsets, manual wall-reflections, and Pierce mode. |
| **`Brick`** | `Gameplay/` | Tracks health/durability, handles visual flashing on hit, spawns score rewards, drops powerups, and shatters into debris. |
| **`PowerUp`** | `Gameplay/` | Drops neon capsules with alphanumeric labels, falling towards the paddle to trigger active powerup modifiers. |
| **`LaserBeam`** | `Gameplay/` | Upward-travelling trigger projectiles fired from the paddle when in Laser mode. |
| **`EffectsManager`** | `Effects/` | Manages camera screen shake routines and instantiates floating points text popups. |
| **`GridBackground`** | `Effects/` | Renders the scrolling vector grid using GL line calculations. |
| **`HUDController`** | `UI/` | Controls standard canvas UI text displays (score, high-score, lives, level, active power-up warnings). |
| **`MenuController`** | `UI/` | Coordinates the active display of main menus, pause, game over, stage clear, and victory panels. |

---

## ⚡ Power-ups

Bricks have a **15% chance** to drop a glowing capsule containing a power-up:
* **`C` (Catch)**: Active balls stick to the paddle upon contact. Press Space or Click to aim and launch.
* **`E` (Expand)**: Lerps the paddle width to $1.6\times$ its default size.
* **`L` (Laser)**: Mounts neon laser blasters to the paddle; shoot using Space/Click.
* **`T` (Triple)**: Instantly splits all active balls into three independent balls.
* **`S` (Slow)**: Temporarily slows all balls down to their baseline speed.
* **`P` (Pierce)**: Turns the ball into a flaming orange fireball that ignores colliders and slices straight through breakable bricks.
* **`1UP`**: Grants an extra life.

---

## ⚙️ How to Configure Settings

You can customize the gameplay balancing in the Unity Inspector by selecting the corresponding GameObjects:

### Paddle Settings
* Select the **Paddle** object:
  * **Keyboard Speed**: Speed of keyboard translation.
  * **Normal, Expand, Laser, Catch Colors**: Modify the glowing neon materials.
  * **Laser Cooldown**: Firing rate of the laser beams.

### Ball Settings
* Open the **Prototypes** folder in the hierarchy, select **BallPrototype**:
  * **Base Speed**: Initial launch speed.
  * **Max Speed**: Maximum velocity cap.
  * **Speed Increment**: Acceleration added to the ball after each paddle hit to ramp up difficulty.

### Game Manager Settings
* Select the **Managers** object:
  * **Starting Lives**: Number of starting attempts.
  * **Power Up Duration**: Lifespan of Expand, Laser, Catch, and Pierce power-ups in seconds.

---

## 🛠️ How to Add New Levels

Levels are built using simple ASCII string grids. You can add as many levels as you like by modifying the array inside **[LevelManager.cs](file:///Users/johnrevell/Git/arkanoid/Assets/Scripts/Core/LevelManager.cs)**.

### Level Syntax
The level layout is a grid of **12 columns**. Use the following character mappings:
* **`.`**: Empty space (no brick)
* **`Y`**: Yellow brick (1 hit, 50 points)
* **`G`**: Green brick (1 hit, 60 points)
* **`B`**: Blue brick (1 hit, 70 points)
* **`O`**: Orange brick (2 hits, 100 points)
* **`I`**: Indigo brick (2 hits, 120 points)
* **`R`**: Red brick (3 hits, 150 points)
* **`S`**: Steel brick (Unbreakable block; ignores standard damage and bounces the ball)

### Step-by-Step: Adding Level 5
1. Open **[LevelManager.cs](file:///Users/johnrevell/Git/arkanoid/Assets/Scripts/Core/LevelManager.cs)**.
2. Scroll to the `levels` array declaration (around line 25).
3. Add a new grid array at the end of the lists. For example:

```csharp
            // Level 5: Neon Cross (Add this to the end of the levels array)
            new string[]
            {
                "S..........S",
                "....RRRR....",
                "...RIIII..R.",
                "..RIIOOIIR..",
                "...RIIII..R.",
                "....RRRR....",
                "S..........S"
            }
```
4. Save the script. The game will automatically load the new level after completing Level 4. The UI level indicator and victory logic will scale dynamically!

---

## 🏷️ Release History

### 🟡 `v1.1.0` — Audio Listener Safeguard & Tactile Audio Expansion (June 3, 2026)
This release resolves the missing `AudioListener` issue and introduces five new retro-synth sound effects to make gameplay and UI navigation feel significantly more tactile.

#### ⚙️ Feature Summary
* **AudioListener Safeguard**: Integrated automatic `AudioListener` assignment to the Main Camera during scene setup (`ArkanoidSceneSetup.cs`) and added a runtime fallback check in `SoundManager.cs` to guarantee that exactly one listener is always present.
* **Launch Sound Effect**: Added a synthetic upward-sweeping pitch envelope played whenever the ball is launched from the paddle.
* **Catch Sound Effect**: Added a synthetic descending pitch drop played when balls stick to the paddle under the Catch power-up.
* **Power-up Drop Sound Effect**: Added a retro note-cascade arpeggio played when a brick drops a power-up capsule.
* **Tactile UI Sound Effects**: Added a clicky focus sound for button selection highlights and a double-chirp tone when confirming menu selections, dynamically bound to all UI buttons.

### 🟢 `v1.0.0` — Initial Base Release (June 3, 2026)
This release establishes the core game loop, developer cheats, procedural audio synth engine, neon graphics pipeline, and keyboard menu navigation.

#### 🖥️ Screen & Display Configuration
* **Reference Resolution**: `1920 × 1080` (16:9 Landscape Aspect Ratio).
* **Standalone Build Settings**: Default width: `1920`, Default height: `1080` (Native Fullscreen enabled).
* **WebGL Build Settings**: Width: `960`, Height: `600`.
* **UI Scaling Model**: `Scale With Screen Size` (Reference Resolution: `1920x1080`, Match: `0.5` width/height balance).
* **Playfield Boundaries (Unity World Space)**:
  * **Left Border**: `x = -6.1`
  * **Right Border**: `x = 6.1`
  * **Top Border**: `y = 8.4`
  * **Bottom Death Zone**: `y = -8.6`
  * **Camera Setup**: Main Camera is positioned at `(0, 0, -10)` with **Orthographic Projection** and **Orthographic Size = `8.5`**.
  * *Note: The playable grid is centered at `(0,0)`, giving a world viewport of `12.2` units wide by `17.0` units high (aspect ratio ≈ `0.718` vertical format).*

#### ⚙️ Feature Summary
* **Classic Game Loop**: Main Menu, Gameplay Scene, Pause Menu, Stage Transitions, Game Over screen, and Victory screen.
* **Level Progression**: Four pre-configured brick matrix levels, generated dynamically via ASCII strings.
* **Keyboard UI Navigation**: Fully operational keyboard-focused navigation for UI buttons using **Up/Down Arrow Keys** and **Space/Enter** to select, with neon hover highlights.
* **Developer Cheat System**: Dev tools enabled during gameplay. Holding `C` allows skipped levels (`1`–`4`) and instant power-up injections (`Q`/`W`/`E`/`R`/`T`/`Y`/`U`).
* **Procedural Sound Engine**: High-fidelity sound effects generated programmatically in real-time. Synthesizes wave sound envelopes for bounces, brick hits, laser firing, powerups, wins, and deaths.
* **Neon Synthwave Graphics**: Retro scrolling GL vector line background grid, dynamic particle burst on breakable bricks, trailing ball effects, and URP Bloom/Vignette post-processing volume configuration.

