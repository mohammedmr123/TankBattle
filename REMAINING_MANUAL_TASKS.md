# Remaining Manual Tasks — Tank Battleground

**Priority order: highest-value items first.**  
**All tasks require the Unity 6 LTS (6000.6.5f1) Editor to be open with this project.**

---

## TASK 1 — Install Cinemachine and Trigger Recompile ⚠️ BLOCKS CAMERA

**Why:** `com.unity.cinemachine: 2.10.3` was added to `Packages/manifest.json`. Unity must download and resolve it before any Cinemachine API is available for the editor setup tools.

**Where:** Unity Editor (automatic on next open)

**Steps:**
1. Open the project in Unity Hub → Unity 6 LTS.
2. Wait for the "Importing packages..." dialog to complete.
3. Check **Console** → no errors related to Cinemachine.
4. If you see `"Cinemachine package not found"`: go to **Window → Package Manager → + → Add package by name** → type `com.unity.cinemachine` → version `2.10.3` → **Add**.

**Success:** Console shows "Cinemachine 2.10.3 imported successfully."

---

## TASK 2 — Run the Cinemachine Editor Setup Tool ⚠️ BLOCKS CAMERA

**Why:** The camera shake, smooth follow, and confiner require a Cinemachine Virtual Camera to be placed in each gameplay scene.

**Where:** Unity Editor top menu → **TankBattleground → Setup Cinemachine Camera (Open Scene Required)**

**Steps (repeat for each gameplay scene: Test.unity, Level1, Level2, Level3):**
1. Open the scene in the Editor.
2. Click **TankBattleground → Setup Cinemachine Camera**.
3. Select the newly created `CM_PlayerFollowCamera` GameObject.
4. In the **Inspector**:
   - Body: set to **Framing Transposer** (Cinemachine 2.x) or **Position Delta** (Cinemachine 3.x)
   - Screen X/Y: 0.5 / 0.5 (centered)
   - Camera Distance: **20**
   - Pitch: **60–70°** (top-down angle)
   - Aim: **Do Nothing**
5. Disable the old `TopDownCameraFollow` component (uncheck it, do NOT delete it yet).
6. Add **Cinemachine Impulse Source** to the `ThrowProjectile` GameObject.
7. Press **Play** → confirm camera follows the player tank and shakes when firing.

**Success:** Camera smoothly follows tank, shakes on fire, stays within arena.

---

## TASK 3 — Configure the ProjectilePoolManager in Scene ⚠️ BLOCKS POOLING

**Why:** `ProjectilePoolManager` must be in the scene for pools to work. It needs the prefab references set.

**Where:** Unity Editor → Hierarchy

**Steps (do this in the Test.unity scene, then in each Level scene):**
1. In the Hierarchy, create: **Right-click → Create Empty** → name it `[ProjectilePoolManager]`.
2. Add component: **ProjectilePoolManager**.
3. Assign Inspector fields:
   - Player Projectile Prefab: `Assets/Prefabs/Projectile.prefab`
   - Enemy Projectile Prefab: `Assets/Prefabs/Projectile.prefab` (same for now)
   - Impact FX Prefab: choose a CFXR explosion particle from `Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Explosions/`
4. Press **Play** → fire the player tank → confirm bullets appear/reappear without Instantiate log spam.

**Verify in Profiler:** No `Instantiate` calls during steady-state shooting.

---

## TASK 4 — Configure the LevelManager in Each Level Scene ⚠️ BLOCKS WIN/LOSS

**Why:** LevelManager must exist in each scene for victory and defeat conditions to work.

**Steps:**
1. In the Hierarchy, create an empty GameObject named `[LevelManager]`.
2. Add component: **LevelManager**.
3. Assign:
   - Victory Panel: the UI GameObject that shows "You Win!"
   - Game Over Panel: the UI GameObject that shows "Game Over"
4. Confirm `EnemyTankAI.Start()` calls `LevelManager.RegisterEnemy(this)`.
5. Test: kill all enemies → Victory Panel appears.
6. Test: let player die → Game Over Panel appears.
7. Test: press Retry button → scene reloads cleanly.

---

## TASK 5 — Assign the Ground LayerMask to Tank_Inputs ⚠️ BLOCKS AIMING

**Why:** The aiming raycast must be restricted to the ground plane to avoid hitting the tank's own hull.

**Steps:**
1. Open **Edit → Project Settings → Tags and Layers**.
2. Confirm Layer 8 = `Ground` (already set in TagManager.asset).
3. Select the floor mesh/plane in the scene.
4. Set its **Layer** dropdown to **Ground**.
5. Select the **PlayerTank** GameObject → find **Tank_Inputs** component.
6. Set the **Ground Layer Mask** field to **Ground** (uncheck everything else).
7. Press **Play** → move mouse → reticle stays on the ground, not on the tank hull.

---

## TASK 6 — Build Level Scenes (Level1, Level2, Level3) ⚠️ ASSESSMENT REQUIREMENT

**Why:** The assessment requires 2–3 playable levels. The build script creates blank scenes; you must lay out gameplay content.

**Level 1 — Warmup Arena:**
1. Open `Assets/Scenes/Level1_Warmup.unity`.
2. ProBuilder: create a flat 60×60 ground plane + 4 surrounding walls.
3. Assign the **Ground** layer to the floor plane.
4. Add 2–3 **Tank_Enemy** prefabs from `Assets/Prefabs/Tank_Enemy.prefab`.
5. Add 4–6 **Destructible Block** prefabs (plain Cube + DestructibleBlock script, health=30).
6. Add Player tank prefab (copy from Test.unity hierarchy).
7. Add LevelManager, ProjectilePoolManager, FPSDebugPanel, Camera rigs.
8. Set enemy `maxHealth = 40` for easy difficulty.

**Level 2 — Tactical:**
1. Duplicate Level1 scene → save as `Level2_Tactical.unity`.
2. Add corridors using ProBuilder blocks.
3. Add 3–5 enemies with `maxHealth = 60`, tighter followRange.
4. Remove destructible blocks; add indestructible barricades.

**Level 3 — Showdown (Optional, +5 pts):**
1. Duplicate Level2 → `Level3_Showdown.unity`.
2. 5+ enemies, `maxHealth = 80`, faster fire rate.

---

## TASK 7 — Build Main Menu Scene ⚠️ ASSESSMENT REQUIREMENT

**Steps:**
1. Open `Assets/Scenes/MainMenu.unity`.
2. Add a **UI Canvas** → add two Buttons: "PLAY" and "QUIT".
3. Add empty GameObject `[MainMenu]` → attach **MainMenuController** component.
4. Wire: PLAY Button → `MainMenuController.OnPlayButton()` | QUIT Button → `MainMenuController.OnQuitButton()`.
5. Add a background (solid color Camera or sprite).
6. Press **Play** → click Play → Level1 loads.

---

## TASK 8 — Register All Scenes in Build Settings ⚠️ BLOCKS BUILD

**Steps:**
1. **File → Build Settings**.
2. Click **Add Open Scenes** for each scene in order:
   - `Assets/Scenes/MainMenu.unity` (index 0)
   - `Assets/Scenes/Level1_Warmup.unity` (index 1)
   - `Assets/Scenes/Level2_Tactical.unity` (index 2)
   - `Assets/Scenes/Level3_Showdown.unity` (index 3)
3. Set Platform to **PC, Mac & Linux Standalone** → Architecture: **x86_64**.

---

## TASK 9 — Execute Windows Standalone Build ⚠️ ASSESSMENT REQUIREMENT

**Steps:**
1. After completing Tasks 1–8: **TankBattleground → Build Windows Standalone**.
2. Wait for build (~2–5 minutes).
3. Navigate to `Builds/TankBattleground/`.
4. Launch `TankBattleground.exe` → confirm game starts, main menu shows, level loads.
5. Test WASD movement, mouse aiming, left-click firing.
6. **TankBattleground → Package Build as ZIP** → produces `TankBattleground_Windows.zip`.

---

## TASK 10 — Test Physical Gamepad

**Why:** Gamepad dual-stick input cannot be verified without a physical controller.

**Steps:**
1. Connect an Xbox or DualShock controller before launching the build.
2. Launch `TankBattleground.exe`.
3. Confirm:
   - Left stick moves and steers tank hull.
   - Right stick rotates turret 360°.
   - Right Bumper OR Right Trigger fires.
   - Select/Back button toggles the debug panel.
4. If right-stick axis is wrong: open **Edit → Project Settings → Input Manager** and adjust `AimHorizontal` axis index (try 2, 3, or 5) until correct.

---

## TASK 11 — Assign ThrowProjectile Fields in Test.unity

The existing Test.unity scene has ThrowProjectile but may not have the `throwPoint` Transform assigned.

**Steps:**
1. Open `Assets/Scenes/Test.unity`.
2. Select the Player Tank → find **ThrowProjectile** component.
3. Assign: **Throw Point** = the turret barrel tip Transform.
4. Assign: **Projectile Prefab** = `Assets/Prefabs/Projectile.prefab`.
5. Press **Play** → left-click fires a bullet.

---

## TASK 12 — Commit to Git (Optional But Recommended)

The workspace is not a git repository.

**Steps:**
1. Open PowerShell in the project root.
2. `git init`
3. `git add .`
4. `git commit -m "feat: complete assessment implementation"`
5. Push to your GitHub fork.

**Important:** The `.gitignore` already excludes `Library/`, `Temp/`, `Logs/`, and build artifacts. Do not add these.

