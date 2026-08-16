# Anisac — Architecture Reference

## Assembly Layers

```
Game.Shared          ← no dependencies
Game.Infrastructure  ← no dependencies
Game.Combat          ← depends on Game.Shared
Game.Character       ← depends on Game.Shared, Game.Combat, Game.Infrastructure
Game.Enemy           ← depends on Game.Shared, Game.Combat (inferred)
```

---

## File Map

### Game.Shared

| File | Type | Role |
|------|------|------|
| `ActionPhase.cs` | Enum | Startup / Active / Recovery / Done |
| `ActionType.cs` | Enum | None / Attack / Dash / Jump / Move |
| `CharacterStateType.cs` | Enum | 9 state types (unused at runtime, for reference) |
| `DamageType.cs` | Enum | Physical / Magic / True |
| `HitInfo.cs` | Struct | Damage payload: causer, type, amount, location, knockback |
| `IDamageable.cs` | Interface | `TakeDamage(HitInfo)` — any damageable object implements this |
| `IMovable.cs` | Interface | `IsGrounded`, `OnMove()`, `OnJump()` |
| `MovementStats.cs` | ScriptableObject | Tunable movement values: speed, jumpForce, dashForce, cooldowns, counts |
| `InputSystem_Actions.cs` | Auto-generated | Input action wrappers for Player and UI maps |
| `StateMachine/BaseState.cs` | Abstract class | `EnterState()`, `ExecuteState()`, `ExitState()` |
| `StateMachine/StateMachine.cs` | Plain C# class | Runs current state, evaluates transitions and any-transitions |
| `StateMachine/StateNode.cs` | Plain C# class | Holds a state + its outgoing transitions |
| `StateMachine/Transition.cs` | Plain C# class | Pair of (target state, condition predicate) |
| `StateMachine/FuncPredicate.cs` | Plain C# class | Wraps a `Func<bool>` as an `IPredicate` |

### Game.Combat

| File | Type | Role |
|------|------|------|
| `ActionData.cs` | ScriptableObject | Full action definition: sprites, timing phases, hitbox index, damage |
| `ActionRuntime.cs` | Plain C# class | Executes one action: ticks phases, fires events, tracks cancel window |
| `ActionSystem.cs` | MonoBehaviour | Receives input, resolves combo, executes action, owns current runtime |
| `ComboData.cs` | ScriptableObject | Combo rule: input sequence, required previous action, result action, priority |
| `ComboDatabase.cs` | ScriptableObject | Collection of all ComboData assets |
| `ComboResolver.cs` | Plain C# class | Matches buffered input against combos, returns best matching ActionData |
| `BufferedInput.cs` | Struct | Single buffered input: ActionType + timestamp |
| `InputBuffer.cs` | MonoBehaviour | Queue of BufferedInputs; Push / GetValid / ConsumeAll |
| `Animation/AnimationDriver.cs` | MonoBehaviour | Drives SpriteRenderer frame-by-frame; three modes: locomotion / action / priority |
| `Health/Health.cs` | MonoBehaviour | Holds HP, implements IDamageable, fires OnDamaged / OnDied events |
| `HitBox/HitboxCollider.cs` | MonoBehaviour | Single BoxCollider2D hitbox; fires OnHit; adjusts offset by facing direction |
| `HitBox/HitboxSystem.cs` | MonoBehaviour | Manages all HitboxColliders; enables correct hitbox during Active phase only |
| `Debug/CombatDebugHUD.cs` | MonoBehaviour | On-screen action timeline (Startup=Red, Active=Green, Recovery=Gray, cancel=Yellow) |
| `Debug/DummyEnemy.cs` | MonoBehaviour | Empty stub — placeholder for future test target |

### Game.Character

| File | Type | Role |
|------|------|------|
| `CharacterMovement.cs` | MonoBehaviour | Physics movement: velocity, jump (air jumps), dash (cooldown + count), facing, movement lock |
| `CharacterStateController.cs` | MonoBehaviour | Builds and runs the 9-state FSM; wires all transitions; handles hurt request from Health |
| `PlayerController.cs` | MonoBehaviour | Input subscriber; routes Attack/Dash/Jump/Move events to movement and ActionSystem; blocks input when dead/hurt |
| `States/IdleState.cs` | BaseState | Plays idle animation |
| `States/WalkingState.cs` | BaseState | Plays walk animation |
| `States/StoppingState.cs` | BaseState | Plays stop animation with timer |
| `States/JumpingState.cs` | BaseState | Plays jump animation; tracks IsFalling |
| `States/FallingState.cs` | BaseState | Plays fall animation |
| `States/LandingState.cs` | BaseState | Plays land animation with timer |
| `States/DashingState.cs` | BaseState | Plays dash animation with timer |
| `States/AttackingState.cs` | BaseState | Locks movement; exits when ActionSystem.CurrentRuntime == null |
| `States/HurtState.cs` | BaseState | Locks movement; plays hurt animation; exits via IsDone |

### Game.Infrastructure

| File | Type | Role |
|------|------|------|
| `CameraFollow2D.cs` | MonoBehaviour | Smooth damp follow with configurable offset and smoothTime (default 0.12s) |

### Game.Enemy (no asmdef — Default Assembly)

| File | Type | Role |
|------|------|------|
| `EnemyController.cs` | MonoBehaviour | Self-contained AI: detect → chase → attack → hurt → death; uses internal enum state + timers; drives AnimationDriver |

---

## Combat Pipeline

```
PlayerController.OnAttackInput()
    └─► ActionSystem.OnInput(ActionType)
            ├─► InputBuffer.Push(BufferedInput)
            └─► TryResolveAndExecute()
                    ├─► InputBuffer.GetValid()          ← filters by windowTime
                    ├─► ComboResolver.Resolve(buffer, previousAction)
                    │       └─► returns ActionData (best matching combo) or null
                    ├─► check: ActionRuntime.IsInCancelWindow (or no current action)
                    └─► ExecuteAction(ActionData)
                            ├─► new ActionRuntime(actionData)   ← plain C# object
                            ├─► ActionRuntime.OnCancelWindowOpened → TryResolveAndExecute (combo chaining)
                            ├─► InputBuffer.ConsumeAll()
                            ├─► AnimationDriver.PlayActionAnimation(actionData)
                            └─► OnActionStarted event → HitboxSystem

ActionRuntime.Tick(dt)  [called by ActionSystem.Update]
    └─► phases: Startup → Active → Recovery → Done
            ├─► Startup→Active: fires OnPhaseChanged(Active)
            ├─► Active→Recovery: fires OnPhaseChanged(Recovery)
            ├─► Recovery: fires OnCancelWindowOpened (once, at cancelWindowStart)
            └─► Recovery end: fires OnActionComplete, phase = Done

HitboxSystem  [subscribes to ActionSystem.OnActionStarted + ActionRuntime.OnPhaseChanged]
    ├─► Active phase: enable hitBoxs[hitboxIndex].Collider
    ├─► Other phases: disable all colliders
    └─► OnHit trigger:
            └─► build HitInfo (from ActionData)
            └─► IDamageable.TakeDamage(hitInfo)  ← on hit target

Health.TakeDamage(HitInfo)
    ├─► reduce CurrentHealth
    ├─► fire OnDamaged(HitInfo)
    └─► if dead: fire OnDied()

CharacterStateController  [subscribes to Health.OnDamaged]
    └─► HandleDamaged() → sets _hurtRequested = true → FSM any-transition to HurtState
```

---

## FSM — 9 States

```
States: Idle | Walking | Stopping | Jumping | Falling | Landing | Dashing | Attacking | Hurt

Priority order (any state can transition to higher priority):
  1. Hurt          (AnyTransition — fires when _hurtRequested consumed)
  2. Attacking     (ActionSystem.CurrentRuntime != null)
  3. Dashing       (CharacterMovement.IsDashing)
  4. Jumping/Fall  (IsGrounded false + velocity)
  5. Ground movement (Walk / Stop / Land)

Key exit conditions:
  Attacking → Idle     : CurrentRuntime == null
  Hurt      → Idle/Walk/Jump/Fall : HurtState.IsDone + ground/air check
  Dashing   → Idle/Fall : DashingState.IsDone + ground check
  Stopping  → Idle     : StoppingState.IsDone + no input
  Landing   → Idle/Walk : LandingState.IsDone + input check
```

---

## Key Class Relationships

```
PlayerController
    │  input events
    ├──► CharacterMovement   (jump, dash, move)
    └──► ActionSystem        (attack input)

CharacterStateController
    │  reads from:
    ├──► CharacterMovement   (IsGrounded, IsDashing, HasMoveInput)
    ├──► ActionSystem        (CurrentRuntime)
    ├──► Health              (OnDamaged event)
    └──► AnimationDriver     (via each state)

ActionSystem
    ├──► InputBuffer
    ├──► ComboResolver       (owns ComboDatabase)
    ├──► AnimationDriver
    └──► ActionRuntime       (creates, ticks, disposes)

HitboxSystem
    ├──► ActionSystem        (OnActionStarted)
    ├──► ActionRuntime       (OnPhaseChanged)
    └──► HitboxCollider[]    (enables by index)

EnemyController  (self-contained)
    ├──► Health              (OnDamaged, OnDied)
    └──► AnimationDriver     (locomotion / action / priority modes)
```

---

## ScriptableObject Data Flow

```
ActionData      → ActionSystem / AnimationDriver / ActionRuntime / HitboxSystem / EnemyController
ComboData       → ComboResolver (via ComboDatabase)
ComboDatabase   → ComboResolver
MovementStats   → CharacterMovement
```

---

## Design Patterns

| Pattern | Class / File | Cách dùng |
|---------|-------------|-----------|
| **Data-Driven** (ScriptableObject) | `ActionData`, `ComboData`, `ComboDatabase`, `MovementStats` | Dữ liệu game tách khỏi code — thay đổi balance, timing, hitbox không cần sửa .cs |
| **Facade** | `ActionSystem` | Single entry point cho toàn bộ combat pipeline. Outsider chỉ cần gọi `OnInput()` — không biết InputBuffer, ComboResolver, ActionRuntime bên trong |
| **Strategy** | `ComboResolver` | Thuật toán match combo được đóng gói riêng — có thể swap sang resolver khác mà không đổi ActionSystem |
| **Observer** | `ActionRuntime` events | `OnPhaseChanged`, `OnCancelWindowOpened`, `OnActionComplete` — HitboxSystem và ActionSystem subscribe; ActionRuntime không biết subscriber là ai |
| **Command** | `BufferedInput` struct | Input được lưu lại (ActionType + timestamp) và execute trễ — cho phép combo chaining qua cancel window |
| **State** (FSM) | `CharacterStateMachine`, `BaseState`, `StateNode`, `Transition` | Mỗi state là class riêng; StateMachine quản lý transition; AnyTransition cho Hurt/Attack priority |
| **Template Method** | `ActionRuntime.Tick()` | Khung cố định: Startup → Active → Recovery → Done. Timing thay đổi theo ActionData; event fire points cố định ở từng pha |

---

## Architecture Notes

### Dependency Direction (Assembly Rule)
```
Game.Shared → Game.Combat → Game.Character → Game.Roguelite → Game.UI
```
Một chiều duy nhất. `Game.Combat` không được biết đến `Game.Character`. `Game.Shared` không phụ thuộc ai.

### Planned Assemblies (chưa tạo)
- `Game.Roguelite` — run data, meta progression, room/level management
- `Game.UI` — health bar, HUD, menus

### MovementStats — Runtime Copy Pattern
`CharacterMovement` gọi `Instantiate(movementStats)` trong `Awake` để tạo bản copy runtime. Roguelite upgrade sửa bản copy — **không bao giờ sửa asset gốc**. Asset gốc giữ nguyên làm baseline.

### Engine
Unity 6.3 **2D Core** (không phải URP). Rendering pipeline mặc định, không dùng URP post-processing.
