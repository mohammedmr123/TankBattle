# Assessment Implementation Plan: TopDown Tank Battleground

**Target Role:** Senior Unity Game Developer  
**Target Unity Version:** Unity 6 LTS (6000.6.5f1 installed & active)  
**Target Platform:** Windows Standalone x86_64  
**Product Name:** Tank Battleground  
**Date:** October 9, 2026  

---

## 1. System Requirements & Matrix

| Requirement | Relevant Files / Assets | Existing Implementation | Required Changes | Dependencies | Risk Level | Verification Method | Current Status |
|---|---|---|---|---|---|---|---|
| **Req 1: Engine Upgrade & Clean Packages** | `ProjectSettings/ProjectVersion.txt`<br>`Packages/manifest.json` | Unity 2021.3.33f1 starting version, opened in Unity 6000.6.5f1 | Add Cinemachine package (`com.unity.cinemachine: 2.10.3`). Verify all packages resolve cleanly. Fix obsolete API calls (`FindFirstObjectByType`, `velocity` -> `linearVelocity`). | Unity Editor | Medium | Batchmode / Editor compile logs, zero compile errors, zero deprecation warnings. | In Progress |
| **Req 2: Cinemachine Camera System** | `Assets/Scripts/TopDownCamera.cs`<br>`Assets/Scripts/CameraManager.cs`<br>Scenes & Prefabs | `TopDownCamera.cs` (class `TopDownCameraFollow`) has rigid, basic Lerp follow. No screen shake, no confiner, no framing deadzones. | Replace with Cinemachine Virtual Camera rig. Configure smooth damping, top-down isometric angle. Add Cinemachine Impulse Listener and impulse triggers on firing and explosion. Add Confiner / arena bounding box. | `com.unity.cinemachine` | Medium | In-editor Play Mode, camera shake on firing/explosions, view bounded at arena walls. | Planned |
| **Req 3: Gamepad & Dual-Stick Input Overhaul** | `Assets/Scripts/Tank_Inputs.cs`<br>`Assets/Scripts/TankController.cs`<br>`Assets/Scripts/ThrowProjectile.cs`<br>`ProjectSettings/InputManager.asset` | Mouse-only raycast (`Input.mousePosition`), fire on `GetMouseButtonDown(0)`. No gamepad right stick or trigger support. Raycast hits player tank itself. | Configure InputManager for right stick axes (Aim X/Y), triggers, and bumpers. In `Tank_Inputs.cs`, support dual-stick gamepad + KBM fallback with auto-detection. Add Ground LayerMask to mouse raycast. Prevent player self-collision. | None (Legacy Input with complete gamepad mappings) | Medium | Automated script tests + Play mode validation for KBM and Gamepad. | Planned |
| **Req 4: Levels & Game Progression Flow** | `Assets/Scenes/Test.unity`<br>`Assets/Scenes/MainMenu.unity`<br>`Assets/Scenes/Level1_Warmup.unity`<br>`Assets/Scenes/Level2_Tactical.unity`<br>`Assets/Scenes/Level3_Showdown.unity`<br>`Assets/Scripts/GameManager.cs`<br>`Assets/Scripts/LevelManager.cs`<br>`Assets/Scripts/DestructibleBlock.cs` | Single test scene (`Test.unity`). No main menu, no level transitions, hardcoded reload on game over, no win state. | Build Level 1 (Warmup: destructible blocks, static/low-hp targets), Level 2 (Tactical: barricades, corridors, hunting enemies), Level 3 (Showdown: fortified layout, multiple tanks). Build Main Menu scene with Play/Quit. Create persistent GameManager / LevelManager with Win/Loss UI, retry, and level progression. | UI System, SceneManager | Medium | Full playthrough of menu -> L1 -> L2 -> L3 -> Victory / Defeat flow. | Planned |
| **Req 5: In-Game Debug Panel & QA Cheats** | `Assets/Scripts/Debug/FPSDebugPanel.cs`<br>In-game UI Canvas | No debug overlay exists. | Implement IMGUI/Canvas toggleable panel (Hotkey: F3 / `~` / Gamepad Back-Select). Display current FPS, smoothed FPS, 1% Low FPS, frame time (ms), heap memory, active enemies, active bullets. Implement functional cheats: God Mode, Spawn Enemy, Win/Skip Level, TimeScale (0.5x, 1.0x, 1.5x). Zero per-frame string GC allocations. | Performance API, Input | Low | Toggle overlay, verify all 4 cheats execute real logic, verify metrics update. | Planned |
| **Req 6: Performance Optimization & Best Practices** | `Assets/Scripts/EnemyTankAi.cs`<br>`Assets/Scripts/Projectile.cs`<br>`Assets/Scripts/ThrowProjectile.cs`<br>`Assets/Scripts/TankController.cs`<br>`Assets/Scripts/Pool/ObjectPool.cs`<br>`OPTIMIZATION_TIPS.md` | `Debug.Log(distance)` in FixedUpdate. Instantiate/Destroy per bullet and impact FX. String comparison `tag == "PlayerTank"`. `Time.deltaTime` in `FixedUpdate()`. No LayerMask on aiming raycast. | Remove `Debug.Log(distance)`. Create generic ObjectPool for projectiles and impact FX. Replace tag comparisons with `CompareTag()`. Replace `Time.deltaTime` with `Time.fixedDeltaTime` in all FixedUpdate physics routines. Add Ground LayerMask. Write `OPTIMIZATION_TIPS.md`. | None | Low | Zero log spam, pooled object reuse, fixedDeltaTime physics stability. | In Progress |
| **Req 7: Standalone Windows Build & Distribution** | `ProjectSettings/ProjectSettings.asset`<br>`ProjectSettings/EditorBuildSettings.asset`<br>`Assets/Editor/BuildScript.cs`<br>`Builds/TankBattleground/` | Default productName `TopDownTank`. `m_Scenes` empty. No build pipeline. | Set `productName` to "Tank Battleground", configure standalone windowed/fullscreen options. Register scenes in Build Settings. Automate Windows x86_64 standalone build via Editor script. Package into ZIP file. | Unity Build Pipeline | High | Executable exists, launches cleanly, input works, gameplay loops function outside editor. | Planned |

---

## 2. Execution Phases & Milestones

1. **Phase 1: Project & Repository Audit** — Complete audit report `PROJECT_AUDIT.md`.
2. **Phase 2: Engine Upgrade & Package Cleanliness** — Install Cinemachine, resolve deprecations, clean `Packages/manifest.json`.
3. **Phase 3: Core Optimizations & Object Pooling** — Eliminate log spam, implement `ObjectPool`, fix tag comparisons, fix `fixedDeltaTime`, add Ground LayerMask.
4. **Phase 4: Gamepad & Dual-Stick Controls** — Update `InputManager.asset`, refactor `Tank_Inputs.cs` and `TankController.cs` for responsive dual-stick aiming + KBM fallback.
5. **Phase 5: Cinemachine Camera & Game Feel** — Implement Cinemachine Virtual Camera, smooth damping, confiner, impulse listeners on cannon fire and projectile detonations.
6. **Phase 6: Multi-Level Campaign & Game Flow** — Create Main Menu, Level 1, Level 2, Level 3, GameManager, Destructible blocks, Victory/Defeat flow.
7. **Phase 7: In-Game QA & FPS Debug Panel** — Implement toggleable diagnostic HUD with FPS, 1% low, memory, God Mode, spawner, skip, and timeScale slider.
8. **Phase 8: Windows Standalone Build & Packaging** — Configure ProjectSettings, EditorBuildSettings, trigger batch build, test and zip the build.
9. **Phase 9: Comprehensive Documentation** — Update `README.md`, `OPTIMIZATION_TIPS.md`, `FINAL_TEST_REPORT.md`, and `REMAINING_MANUAL_TASKS.md`.

