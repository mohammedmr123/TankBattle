# Session Update — Options Menu, Persistent Settings, Debug Panel, Audio

This document covers the systems added in this session and the **exact manual
Unity Editor steps** still required. New code needs **no scene wiring** (it
auto-creates at startup), but a few Inspector/settings tasks remain.

---

## What was added (code — already in the project)

| File | Purpose |
|---|---|
| `Assets/Scripts/SettingsManager.cs` | Persistent settings via PlayerPrefs. Loads + applies on start. Audio, aim deadzone, invert-Y, camera shake, quality, VSync, fullscreen, resolution, frame limit. Reset to defaults. |
| `Assets/Scripts/AudioManager.cs` | Volume routing. Works with an AudioMixer (Master/Music/SFX exposed params) **or** falls back to AudioListener + per-source volume. |
| `Assets/Scripts/OptionsMenuUI.cs` | Full options overlay (IMGUI). Open with **ESC** anywhere. Pauses game during gameplay, restores prior pause state. Resolution/fullscreen changes get a 10-second Keep/Revert timer. |
| `Assets/Scripts/GameBootstrap.cs` | Auto-creates `SettingsManager`, `AudioManager`, `OptionsMenuUI`, `FPSDebugPanel` at startup with `DontDestroyOnLoad` + singleton guards. **No per-scene setup needed, no duplicates.** |
| `Assets/Scripts/Debug/FPSDebugPanel.cs` (edited) | Added Pause/Resume, Reset Speed, Reload Level, Main Menu, Toggle Enemy AI, Collision Diagnostics toggle, and a Close button. Fixed duplicate-manager risk. |
| `Assets/Scripts/Projectile.cs` (edited) | Fixed `ActiveProjectileCount` (was always 0) — now tracked via OnEnable/OnDisable. |
| `Assets/Scripts/DestructibleBlock.cs` (edited) | Implemented the previously-unused `damageSourceTags` filter. |
| `Assets/Scripts/Tank_Inputs.cs` (edited) | Added `SetAimConfig(deadzone, invertY)` + invert-Y aim. |
| `Assets/Scripts/ThrowProjectile.cs`, `EnemyTankAi.cs` (edited) | Camera shake now respects the Options "Camera Shake" toggle; firing is blocked while Options is open. |
| `Assets/Scripts/UI/MainMenuController.cs` (edited) | Added `OnOptionsButton()` (optional main-menu button hook). |

---

## New controls
| Action | Keyboard/Mouse | Gamepad |
|---|---|---|
| Open Options | **ESC** (or "Cancel" button) | **B / Circle** (Cancel) |
| Open Debug Panel | **F3** or **`** | Select / Back (View) |

---

## MANUAL TASKS (beginner, step by step)

### M1 — Let Unity import the new scripts
1. Click into the **Unity Editor** window so it's the active window.
2. Wait ~5 seconds. Unity detects the new `.cs` files and compiles.
3. Open the **Console** tab (bottom). Confirm there are **no red errors**.
   - If you see errors mentioning a new script, tell me the exact text.

### M2 — (Optional, recommended) Create an AudioMixer so volume sliders sound right
The project currently has **no audio clips and no mixer**. The volume system is
fully functional, but you must add clips + a mixer to hear anything.

1. **Project** window → right-click in `Assets` → **Create → AudioMixer**. Name it `MainAudioMixer`.
2. Double-click it to open the **AudioMixer** window.
3. At the top you'll see a **Master** group. Click it.
4. In the **Inspector**, find the **Attenuation Unit**/**Volume** area and click the
   small arrow next to the Volume value → choose **Expose**. (The exposed name
   appears under **Exposed Parameters** on the right.)
5. Right-click the Master group → **Add child group** → name it **Music**. Repeat → name another **SFX**.
6. Select **Music**, expose its Volume the same way. Select **SFX**, expose its Volume.
7. In the **Exposed Parameters** list, rename them EXACTLY:
   - `MasterVolume`  (was Master)
   - `MusicVolume`   (was Music)
   - `SFXVolume`     (was SFX)
   These names must match `AudioManager.masterParam/musicParam/sfxParam`.
8. Press **Play**, then **Pause**. In the Hierarchy find the runtime object
   **`[AudioManager]`**, select it, and drag `MainAudioMixer` into its **Mixer** field.
9. (To actually hear sound) add an **AudioSource** to a GameObject, set its
   **Output** to the **Music** or **SFX** group, and assign an **AudioClip**.

> Without a mixer assigned, the Master slider still controls `AudioListener.volume`
> and Music/SFX sliders control their sources — so it degrades gracefully.

### M3 — Projectile collision (prevents shots passing through thin walls)
1. **Project** window → open `Assets/Prefabs/Projectile.prefab`.
2. Select its root → **Inspector → Rigidbody**.
3. Set **Collision Detection** = **Continuous Dynamic** (was Discrete).
4. **Ctrl+S** to save the prefab.
5. Repeat for any enemy projectile prefab if separate.

### M4 — Verify the Debug Panel + Options actually appear
1. Open `Assets/Scenes/Level1_Warmup.unity`.
2. Press **Play**.
3. Press **F3** → the debug overlay should show FPS, enemies, projectiles, and the
   new buttons (Pause, Reload, Main Menu, Toggle Enemy AI, Collision Diag, Close).
4. Press **ESC** → the Options menu should open and the game should pause.
5. Move a slider (e.g. Master) — it updates live.
6. Press **ESC** again → menu closes, game resumes at its previous speed.

---

## Cinemachine shake (already partially wired)
- `ThrowProjectile` and `EnemyTankAi` have a `cinemachineImpulseSource` field.
- To enable shake: select the **ThrowProjectile** GameObject, **Add Component →
  Cinemachine Impulse Source**, then drag that component into the
  **Cinemachine Impulse Source** slot. Repeat on the enemy prefab if desired.
- Toggle it on/off at runtime via **Options → Camera Shake**.

---

## Known limitation (by design, safe path)
- **Key rebinding is NOT available.** The project uses the legacy Input Manager,
  which cannot remap keys at runtime. Aim deadzone + invert-Y are provided because
  they map to the real input component. This avoids migrating (and risking) your
  working controls.

---

## Reset all settings
- **Options → Reset Defaults** clears all PlayerPrefs keys and reapplies defaults.

