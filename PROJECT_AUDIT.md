# Project Audit & Baseline Report: TopDown Tank Battleground

**Audit Date:** October 9, 2026  
**Auditor:** Senior Unity Engineer  
**Workspace:** `d:\projects unity\Unity_Dev_test-main\Unity_Dev_test-main`  

---

## 1. Executive Summary

An exhaustive audit of the TopDown Tank Battleground project was performed. The original project was created in Unity 2021.3.33f1 utilizing the Built-in Render Pipeline. It has been opened and imported into **Unity 6 LTS (6000.6.5f1)** on the host machine. The core gameplay consists of a player tank driving and shooting at an enemy tank in a flat sandbox scene. 

Several critical issues, obsolete APIs, and performance bottlenecks identified in the assessment brief were validated and cataloged:
* Extreme console spam from `Debug.Log(distance)` in `EnemyTankAi.cs:160` inside `FixedUpdate()`.
* Missing object pooling: every bullet and impact particle effect is instantiated and destroyed at runtime.
* Hardcoded KBM inputs: `Input.mousePosition` and `Input.GetMouseButtonDown(0)` preclude gamepad gameplay.
* Physics timing inaccuracies: `Time.deltaTime` used in `FixedUpdate()` rather than `Time.fixedDeltaTime`.
* String comparisons `collision.gameObject.tag == "PlayerTank"` instead of `CompareTag()`.
* Raycast aiming hits the tank's own colliders due to absence of a LayerMask.
* Rigid camera follow script (`TopDownCameraFollow` in `TopDownCamera.cs`) lacking Cinemachine, impulse shake, and arena confiners.
* Single flat test scene without menu, victory/loss progression, or multiple levels.

---

## 2. Environment & Engine Configuration

* **Original Target Version:** Unity 2021.3.33f1
* **Available Installed Editors on Machine:**
  * `C:\Program Files\Unity\Hub\Editor\2021.3.33f1`
  * `C:\Program Files\Unity\Hub\Editor\2022.3.51f1` (Unity 2022.3 LTS)
  * `C:\Program Files\Unity\Hub\Editor\6000.0.44f1` (Unity 6 LTS)
  * `C:\Program Files\Unity\Hub\Editor\6000.6.5f1` (Unity 6 Active Process PID: 13196)
* **Render Pipeline:** Built-in Render Pipeline (BiRP) preserved, standard forward rendering.
* **Git Status:** Workspace was extracted from an archive (`Unity_Dev_test-main`). An untouched original backup copy is preserved at `..\Unity_Dev_test-main (2)\Unity_Dev_test-main`.

---

## 3. Investigation of Mesh Warning: 'Lorry_12.fbx'

* **Reported Warning:** *"A polygon of Mesh 'Lorry_12' in Assets/Free Low Poly Vehicle/Models/Lorry_12.fbx is self-intersecting and has been discarded."*
* **Investigation Findings:**
  1. The mesh `Lorry_12.fbx` has GUID `3a004bfd6d1a0fe428412f6df8a3f41f`.
  2. The only prefab referencing this mesh is `Assets/Free Low Poly Vehicle/Pefabs/Lorry_12.prefab` (GUID `0748e0aebe4087e418fd7be8c4391d19`).
  3. The prefab is **only** referenced in `Assets/Free Low Poly Vehicle/Demo Scene/Demo Scene.unity`.
  4. Neither the gameplay scenes (`Test.unity`) nor any gameplay prefabs (`Tank_Enemy.prefab`, `Projectile.prefab`, or player tank) reference `Lorry_12`.
* **Recommendation:** Per the assessment rules, leaving the third-party asset intact without modifying or deleting it is completely safe and avoids unnecessary risk or metadata invalidation.

---

## 4. Codebase Audit & Deprecation Catalog

| File & Line | Issue Type | Description | Remediation |
|---|---|---|---|
| `EnemyTankAi.cs:160` | Performance / GC | `Debug.Log(distance);` executed every physics frame (50Hz) | Remove line immediately. |
| `EnemyTankAi.cs:57` | Deprecated API | `FindFirstObjectByType<TankController>()` deprecated in Unity 6 | Replace with `FindAnyObjectByType<TankController>()`. |
| `EnemyTankAi.cs:104,110,122,145` | Physics Bug | `Time.deltaTime` used in `FixedUpdate()` | Replace with `Time.fixedDeltaTime`. |
| `EnemyTankAi.cs:185` | Memory / GC | `Instantiate(projectilePrefab, ...)` on every shot | Route through `ProjectilePool`. |
| `TankController.cs:41,45,58` | Physics Bug | `Time.deltaTime` used in `FixedUpdate()` | Replace with `Time.fixedDeltaTime`. |
| `TankController.cs:59` | Runtime Risk | `Quaternion.LookRotation(finalTurretLookDir)` can throw zero-vector warnings | Guard with check `if (finalTurretLookDir.sqrMagnitude > 0.001f)`. |
| `Tank_Inputs.cs:50,53` | Input / Physics | `ScreenPointToRay(Input.mousePosition)` with no LayerMask; hits player's own collider | Add `LayerMask` for Ground, ignore tank colliders. Support dual-stick gamepad direction aiming. |
| `ThrowProjectile.cs:25` | Input Limitation | Hardcoded to `Input.GetMouseButtonDown(0)` | Add Gamepad Right Trigger (RT) and Bumper (RB) support. |
| `ThrowProjectile.cs:41` | Memory / GC | `Instantiate(projectilePrefab, ...)` on every shot | Route through `ProjectilePool`. |
| `Projectile.cs:11,22` | Memory / GC | `Instantiate(impactFX)` and `Destroy(this.gameObject)` on impact | Route through projectile and impact particle object pools. |
| `Projectile.cs:13,18` | Performance | `collision.gameObject.tag == "PlayerTank"` / `"EnemyTank"` string comparisons | Replace with `CompareTag("PlayerTank")` and `CompareTag("EnemyTank")`. |
| `TopDownCamera.cs` | Feature Gap | Class is `TopDownCameraFollow` (name mismatch). Basic lerp follow with no framing or shake. | Implement Cinemachine Virtual Camera, Cinemachine Impulse, and Confiner. |
| `EnemyTankHealth.cs:20` | Logic Bug | `if (currentHealth < 0)` instead of `<= 0` causes enemy to survive at 0 health | Fix to `<= 0`, integrate with LevelManager enemy count. |

---

## 5. Requirement-to-File Mapping

* **Unity Upgrade:** `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`.
* **Cinemachine:** `Packages/manifest.json` (`com.unity.cinemachine@2.10.3`), `Assets/Scripts/Camera/...`, Scene cameras.
* **Input System:** `ProjectSettings/InputManager.asset`, `Assets/Scripts/Tank_Inputs.cs`, `Assets/Scripts/ThrowProjectile.cs`.
* **Object Pooling:** `Assets/Scripts/Pool/ObjectPool.cs`, `Projectile.cs`, `ThrowProjectile.cs`, `EnemyTankAi.cs`.
* **Game Progression:** `Assets/Scripts/GameManager.cs`, `Assets/Scripts/LevelManager.cs`, `Assets/Scenes/MainMenu.unity`, `Level1_Warmup.unity`, `Level2_Tactical.unity`, `Level3_Showdown.unity`.
* **In-Game Debug Panel:** `Assets/Scripts/Debug/FPSDebugPanel.cs`.
* **Build Automation:** `Assets/Editor/BuildPipelineScript.cs`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`.

---

## 6. Blockers & Priority Order

1. **[P0 - Core Stability]** Install Cinemachine package cleanly in `Packages/manifest.json` and resolve all compiler warnings.
2. **[P0 - Performance]** Strip `Debug.Log(distance)`, fix `Time.fixedDeltaTime`, fix `CompareTag()`, implement high-performance bullet and FX object pooling.
3. **[P0 - Controls]** Overhaul `InputManager.asset` and `Tank_Inputs.cs` for responsive dual-stick gamepad and seamless KBM fallback.
4. **[P1 - Camera & Game Feel]** Setup Cinemachine Virtual Camera, camera shake impulses, and arena confiner.
5. **[P1 - Levels & Progression]** Build Main Menu, Level 1 (Warmup), Level 2 (Tactical), Level 3 (Showdown), with Win/Loss/Retry/Next game flow.
6. **[P1 - QA Tools]** Implement toggleable FPS Debug Panel with zero-allocation performance counters and all 4 functional QA cheats.
7. **[P0 - Build & Packaging]** Set product name to "Tank Battleground", configure build settings, execute standalone Windows x86_64 build, package into ZIP.

