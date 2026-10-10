# Tank Battleground

## Game Overview
A top-down 3D tank shooter built in Unity 6 LTS. You command a player tank against waves of AI enemies across three progressively difficult arenas.

## Unity Version
**Unity 6 LTS — 6000.6.5f1** (Built-in Render Pipeline)

> Originally created in Unity 2021.3.33f1. Upgraded to Unity 6 LTS for this submission.

---

## How to Open the Project
1. Install **Unity 6 LTS (6000.6.5f1)** via Unity Hub.
2. Click **Add → Add project from disk** in Unity Hub.
3. Select the folder: `Unity_Dev_test-main\Unity_Dev_test-main`.
4. Wait for Unity to import all assets (~2–5 minutes on first open).
5. Open **File → Build Settings** to confirm the scene list is correct.

---

## Controls

### Keyboard & Mouse
| Action | Control |
|---|---|
| Move Forward / Back | W / S or ↑ / ↓ |
| Steer Left / Right | A / D or ← / → |
| Aim Turret | Mouse position (world raycast) |
| Fire | Left Mouse Button |
| Open Options | ESC (or "Cancel") |
| Debug Panel | F3 or ` (Backquote) |

### Gamepad (Xbox / DualShock layout)
| Action | Control |
|---|---|
| Drive / Steer | Left Stick |
| Aim Turret | Right Stick (360°) |
| Fire | Right Bumper (RB) or Right Trigger (RT) |
| Debug Panel | Select / Back (View button) |

**Device switching is automatic.** Move the right stick to switch to gamepad aiming; move the mouse to return to mouse aiming.

---

## Package Changes
| Package | Version | Reason |
|---|---|---|
| `com.unity.cinemachine` | 2.10.3 | Top-down camera follow, camera shake, arena confiner |
| `com.unity.ai.navigation` | 2.0.15 | NavMesh AI (existing) |
| `com.unity.probuilder` | 6.1.2 | Level geometry (existing) |

---

## Build Instructions (Manual)

### From Unity Editor
1. Open the project in Unity 6 LTS.
2. Ensure all scenes compiled cleanly (Console → no red errors).
3. Go to **TankBattleground → Build Windows Standalone** (Editor menu, top bar).
4. The build appears at `Builds/TankBattleground/TankBattleground.exe`.
5. To ZIP: **TankBattleground → Package Build as ZIP** → `TankBattleground_Windows.zip`.

### Manual Build Settings Setup
1. **File → Build Settings**.
2. Click **Add Open Scenes** for each of: `MainMenu`, `Level1_Warmup`, `Level2_Tactical`, `Level3_Showdown`.
3. Confirm MainMenu is at index 0.
4. Set Platform: **PC, Mac & Linux Standalone** → Architecture: **x86_64**.
5. Click **Build**.

---

## Scene List (Build Order)
| Index | Scene | Description |
|---|---|---|
| 0 | MainMenu | Play / Quit buttons |
| 1 | Level1_Warmup | Open arena, easy enemies, destructible blocks |
| 2 | Level2_Tactical | Barricades, narrow corridors, tougher enemies |
| 3 | Level3_Showdown | Final fortified arena, most challenging |

---

## Known Limitations
- Cinemachine Virtual Camera requires manual Inspector configuration after opening in Unity Editor (see `REMAINING_MANUAL_TASKS.md`).
- Level scenes (Level1–Level3) are placeholder blank scenes pending manual layout in the Unity Editor.
- The `Lorry_12.fbx` asset produces a self-intersection warning on import; this is a cosmetic mesh bug in the third-party asset and does not affect gameplay. The asset is only used in the vendor demo scene, not in any gameplay scenes.
- Gamepad right-stick axis mapping uses Unity Legacy Input Manager axis indices 3 (X) and 4 (Y). If your controller maps differently, adjust the `AimHorizontal` and `AimVertical` entries in **Edit → Project Settings → Input Manager**.
