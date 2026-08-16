# Anisac — Project & Learning Progress

_Last updated: 2026-06-30_

---

## Project Status

### Complete / Working

| System | Status | Notes |
|--------|--------|-------|
| Player movement | ✅ Done | Idle, Walk, Stop, Jump (air jumps), Fall, Land, Dash with cooldown/count |
| FSM 9-state | ✅ Done | Priority order: Hurt > Attack > Dash > Jump/Fall > Ground movement |
| InputBuffer | ✅ Done | Timestamp-based, configurable window (0.5s default) |
| ComboResolver | ✅ Done | Priority + length matching, gap timing check |
| ActionRuntime | ✅ Done | Phase machine: Startup → Active → Recovery → Done; cancel window |
| ActionSystem | ✅ Done | Wires buffer + resolver + runtime; cancel-into-combo chaining |
| AnimationDriver | ✅ Done | Three modes: locomotion / action / priority; frame timing, looping |
| HitboxSystem | ✅ Done | Enables correct hitbox on Active phase; builds HitInfo; calls IDamageable |
| Health | ✅ Done | MaxHealth, TakeDamage, OnDamaged / OnDied events |
| HurtState (player) | ✅ Done | AnyTransition from FSM; locks movement; exits by IsDone |
| EnemyController | ✅ Done | Detect / Chase / Attack / Hurt / Death; hit flash; destroyOnDeath option |
| CameraFollow2D | ✅ Done | Smooth damp follow |
| IDamageable interface | ✅ Done | Implemented by Health; used by HitboxSystem and EnemyController |

### Incomplete / Placeholder

| System | Status | Notes |
|--------|--------|-------|
| `DummyEnemy.cs` | ⚠️ Stub | Empty — no behavior implemented |
| Enemy FSM | ⚠️ Flat | EnemyController uses internal enum + timers, not the shared StateMachine — acceptable for now |
| Player death state | ⚠️ Partial | `PlayerController` blocks input on death, no Death state in FSM |
| Knockback on player hit | ⚠️ Partial | HitInfo carries KnockbackForce but nothing applies it to player Rigidbody2D |
| Roguelite loop | ❌ Not started | No run data, no meta progression, no room/level system |
| UI / HUD | ❌ Not started | No health bar, no combo indicator |
| Audio | ❌ Not started | |
| Room / scene management | ❌ Not started | |

---

## Known Issues / Debt

- `EnemyController` uses `GameObject.FindGameObjectWithTag("Player")` in `TryResolvePlayer()` — runs in `Update` when `player == null`. Should be resolved once at start.
- `HitboxSystem` subscribes to `actionSystem.CurrentRuntime.OnPhaseChanged` directly instead of through the `_currentRuntime` it already stores — minor inconsistency.
- No enemy assembly definition — `EnemyController` is in the default assembly.

---

## Learning Progress

### Book: *Learning C# by Developing Games with Unity 3D Beginner's Guide*

**PDF:** `E:\Learning Csharp chapter_1-6.pdf` (127 pages, fully read)

| Chapter | Topic | Status |
|---------|-------|--------|
| Ch1 | Discovering Your Hidden Scripting Skills | 🟡 Content taught — **checking question pending** |
| Ch2 | Introducing the Building Blocks for Unity Scripts | ❌ Not started |
| Ch3 | Getting into the Details of Variables | ❌ Not started |
| Ch4 | Getting into the Details of Methods | ❌ Not started |
| Ch5 | Making Decisions in Code | ❌ Not started |
| Ch6 | Using Dot Syntax for Object Communication | ❌ Not started |

### Ch1 Checking Question (must answer before Ch2)

> In `EnemyController.cs`, the enemy is taught multiple behaviors in one script: detect player, chase, attack, hurt, death.
> According to the spirit of "each script teaches one behavior" — is this a problem or is it fine? **Why?**

---

## Next Steps

### Immediate (in current prototype)
1. Answer Ch1 checking question → unlock Ch2 teaching
2. Apply player knockback from `HitInfo.KnockbackForce` in `CharacterMovement` or `HurtState`
3. Add player Death state to FSM (currently only blocked via `PlayerController`)
4. Replace `FindGameObjectWithTag` in `EnemyController` with serialized reference or single Awake call

### Near-term (still prototype)
5. Build a minimal health bar UI to visualize Health system
6. Add a second enemy type to test the system beyond the current single enemy
7. Consider giving `EnemyController` a proper FSM using the shared `StateMachine` — when complexity grows

### Future (post-prototype)
- Room/scene management
- Roguelite run loop (run data vs permanent data separation)
- Audio integration
- Addressables (only when content volume justifies it)
