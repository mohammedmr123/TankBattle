# Optimization Tips — Tank Battleground

## Status Key
- ✅ **Implemented** — Code written and in project files
- 🔬 **Expected** — Benefit expected but not profiler-measured in this environment
- 📋 **Recommended** — Not yet implemented, worth doing before shipping

---

## 1. Remove Debug.Log Spam ✅

**File:** `Assets/Scripts/EnemyTankAi.cs:160` (original)

**Problem:** `Debug.Log(distance)` was called inside `FixedUpdate()` (50 times/second per enemy). With 3 enemies, this is 150 log messages per second. Each call:
- Formats a string (heap allocation)
- Writes to the internal log queue
- Triggers stack-trace capture in Development builds

**Fix:** The line has been **removed entirely**.

**Expected benefit:** Elimination of ~150–450 managed heap allocations per second during combat. In a profiler session, this would appear as a massive reduction in `GC.Alloc` calls in the FixedUpdate budget.

---

## 2. Object Pooling for Projectiles and FX ✅

**Files:** `Assets/Scripts/Pool/ObjectPool.cs`, `ProjectilePoolManager.cs`, `AutoReturnFX.cs`

**Problem:** Every bullet fired called `Instantiate()` and every impact called both `Instantiate(impactFX)` and `Destroy()`. These are expensive:
- `Instantiate` triggers asset loading, component initialization, and physics registration
- `Destroy` triggers GC eventually
- Rapid fire (0.5s cadence) produces 2–4 objects per second per tank → constant GC pressure

**Fix:** A generic `ObjectPool<T>` class pre-allocates objects on scene load. `Get()` re-enables and repositions an existing object; `Return()` disables it for reuse.

Pool sizes (configurable in Inspector on `ProjectilePoolManager`):
| Pool | Initial | Max |
|---|---|---|
| Player projectiles | 10 | 30 |
| Enemy projectiles | 15 | 50 |
| Impact FX | 10 | 40 |

**Expected benefit:** Zero per-shot heap allocations after warmup. Eliminates GC stutter spikes during rapid combat.

---

## 3. CompareTag Instead of String Equality ✅

**File:** `Assets/Scripts/Projectile.cs` (original: `collision.gameObject.tag == "PlayerTank"`)

**Problem:** The `==` operator on `GameObject.tag` performs a standard string comparison. In hot collision callbacks this wastes CPU and creates ephemeral string objects.

**Fix:** All tag checks replaced with `CompareTag("EnemyTank")` and `CompareTag("PlayerTank")`. `CompareTag` is a native call that does not allocate and is faster than managed string comparison.

---

## 4. Time.fixedDeltaTime in FixedUpdate ✅

**Files:** `TankController.cs`, `EnemyTankAi.cs`

**Problem:** Both scripts used `Time.deltaTime` inside `FixedUpdate()`. `Time.deltaTime` returns the *last render frame time* when called from `FixedUpdate`, which is non-deterministic. During frame drops the physics step gets a larger-than-expected delta, causing tanks to stutter or teleport.

**Fix:** All `FixedUpdate` physics calls now use `Time.fixedDeltaTime`, which is the configured fixed timestep (default 0.02s = 50Hz). This makes movement framerate-independent at the physics level.

---

## 5. Ground LayerMask on Aiming Raycast ✅

**File:** `Assets/Scripts/Tank_Inputs.cs`

**Problem:** The mouse-to-world raycast hit all colliders including the player tank's hull, turret, and wheels. This caused the reticle to snap to incorrect positions when the cursor was over the tank body, making the turret spin erratically.

**Fix:** A `groundLayerMask` field (type `LayerMask`) is now passed to `Physics.Raycast`. Assign the `Ground` layer (layer 8, added to TagManager) to the arena floor and walls in the Inspector. The tank colliders should be on a different layer (e.g. `Default`).

---

## 6. FindAnyObjectByType ✅

**File:** `Assets/Scripts/EnemyTankAi.cs` (original: `FindFirstObjectByType`)

**Problem:** `FindFirstObjectByType<T>()` is deprecated in Unity 6 because it relies on internal instance-ID ordering, which is not deterministic.

**Fix:** Replaced with `FindAnyObjectByType<TankController>()`, which finds any matching instance without ordering guarantees — appropriate here since there is only one player tank.

---

## Additional Recommended Optimizations 📋

### A. Static Batching for Environment
In **Edit → Project Settings → Player**, enable **Static Batching**. Mark all level geometry (floor, walls, barricades) as Static in the Inspector. Unity will merge their draw calls at build time. Expected: 40–60% draw call reduction on geometry-heavy scenes.

### B. Collision Layer Matrix
Open **Edit → Project Settings → Physics → Layer Collision Matrix**. Disable collisions between:
- Projectile vs Projectile (bullets should not collide with each other)
- Player vs Player Projectile (prevents self-damage on spawn)
- Enemy vs Enemy (no friendly-fire physics needed)

This reduces the physics broadphase workload significantly.

### C. Bake Lighting
For static level geometry:
1. Mark all environment objects as **Contribute GI** in the Mesh Renderer.
2. Open **Window → Rendering → Lighting**.
3. Set **Lighting Mode = Baked Indirect**.
4. Click **Generate Lighting**.

Baked lighting replaces per-frame shadow/light calculations with pre-computed textures. Expected: 30–50% GPU time reduction on the lighting pass.

### D. Texture Compression
- Diffuse/albedo textures: **DXT1** (no alpha) or **BC7** (with alpha)
- Normal maps: **BC5** for best quality
- Particle textures: **ETC2** for mobile, **DXT5** for PC

Set in each texture's Import Settings. Expected: 2–4× VRAM reduction and faster texture sampling.

### E. Avoid Per-Frame UI Reconstruction
Use `Text.SetText()` (TextMeshPro) rather than `text = string.Format(...)` to avoid per-frame string allocation in HUD updates. The FPSDebugPanel uses `string.Format` at a controlled 10Hz update rate (`updateInterval = 0.1f`) rather than every frame.

### F. LOD Groups for Complex Meshes
The tank models contain multiple sub-meshes. Adding `LODGroup` components with simplified meshes at distance thresholds (e.g., LOD0 = full detail < 20m, LOD1 = simplified > 20m) would reduce triangle count during follow-camera pulls.

---

## Measured vs Expected

| Optimization | Status | Measurement |
|---|---|---|
| Debug.Log removed | ✅ Implemented | Expected: ~150–450 GC alloc/sec eliminated |
| Object pooling | ✅ Implemented | Expected: 0 alloc per shot after warmup |
| CompareTag | ✅ Implemented | Expected: minor CPU saving in collision callbacks |
| fixedDeltaTime | ✅ Implemented | Expected: deterministic movement |
| Ground LayerMask | ✅ Implemented | Expected: correct aiming behavior |
| Static batching | 📋 Recommended | ~40–60% draw call reduction |
| Baked lighting | 📋 Recommended | ~30–50% GPU lighting cost reduction |
| Collision layer matrix | 📋 Recommended | Reduced physics broadphase work |
| Texture compression | 📋 Recommended | 2–4× VRAM reduction |

> **Note:** No Unity Profiler sessions were run in this environment. All "expected" figures are based on standard Unity optimization literature and industry baselines for comparable game types. A profiler session in Unity Editor with deep profiling enabled would give exact millisecond measurements per category.

---

## Recent Additions (Options / Settings / Debug session)

### G. Accurate projectile counter ✅
`LevelManager.ActiveProjectileCount` was declared but never updated, so the Debug
Panel always showed 0. `Projectile.OnEnable/OnDisable` now increments/decrements it,
so the F3 panel reports the true live projectile count.

### H. No per-frame string allocation in Options / Debug ✅
- `OptionsMenuUI` and `FPSDebugPanel` both build cached `GUIStyle`s once and only
  touch UI when visible. The Debug metrics still refresh at a fixed 10 Hz interval.
- Collision diagnostics (Debug Panel) cache the player `Rigidbody`/`Collider`
  references once instead of searching every frame.

### I. Auto-created persistent managers ✅
`GameBootstrap` creates `SettingsManager`, `AudioManager`, `OptionsMenuUI`, and
`FPSDebugPanel` once at startup with `DontDestroyOnLoad` and singleton guards — no
duplicate managers after scene reloads, and no manual per-scene wiring.


