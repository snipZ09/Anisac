using Game.Combat;
using UnityEngine;
using Game.Shared;

namespace Game.Character
{
    public class CharacterStateController : MonoBehaviour
    {
        [SerializeField] private ActionData idleData;
        [SerializeField] private ActionData walkingData;
        [SerializeField] private ActionData stoppingData;
        [SerializeField] private ActionData jumpingData;
        [SerializeField] private ActionData fallingData;
        [SerializeField] private ActionData landingData;
        [SerializeField] private ActionData dashingData;
        [SerializeField] private ActionData hurtData;

        private AnimationDriver _animationDriver;
        private CharacterMovement _movement;
        private ActionSystem _actionSystem;
        private Health _health;
        private Rigidbody2D _rb;
        private StateMachine _stateMachine;
        private bool _hurtRequested;

        private IdleState _idle;
        private WalkingState _walking;
        private StoppingState _stopping;
        private JumpingState _jumping;
        private FallingState _falling;
        private LandingState _landing;
        private DashingState _dashing;
        private AttackingState _attacking;
        private HurtState _hurt;

        public bool IsHurt => _stateMachine?.GetCurrentState() is HurtState;
        public bool IsHurtPendingOrActive => _hurtRequested || IsHurt;


        private void Awake()
        {
            _animationDriver = GetComponent<AnimationDriver>();
            _movement = GetComponent<CharacterMovement>();
            _actionSystem = GetComponent<ActionSystem>();
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
            }
        }

        void Start()
        {
            // Initialize states
            _idle = new IdleState(_animationDriver, idleData, _movement, _rb);
            _walking = new WalkingState(_animationDriver, walkingData, _movement, _rb);
            _stopping = new StoppingState(_animationDriver, stoppingData);
            _jumping = new JumpingState(_animationDriver, jumpingData, _rb);
            _falling = new FallingState(_animationDriver, fallingData, _movement);
            _landing = new LandingState(_animationDriver, landingData);
            _dashing = new DashingState(_animationDriver, dashingData, _movement);
            _attacking = new AttackingState(_actionSystem, _movement);
            _hurt = new HurtState(_animationDriver, hurtData, _movement);

            // Initialize state machine
            _stateMachine = new StateMachine(_idle);

            _stateMachine.AddAnyTransition(_hurt,
                new FuncPredicate(ConsumeHurtRequest));

            // Add transitions
            // =====================
            // 1. ATTACK - ưu tiên cao nhất
            // =====================
            _stateMachine.AddTransition(_idle, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_walking, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_stopping, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_jumping, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_falling, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_landing, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_dashing, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));


            // =====================
            // 2. DASH - ưu tiên sau attack
            // =====================
            _stateMachine.AddTransition(_idle, _dashing,
                new FuncPredicate(() => _movement.IsDashing && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_walking, _dashing,
                new FuncPredicate(() => _movement.IsDashing && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_jumping, _dashing,
                new FuncPredicate(() => _movement.IsDashing && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_falling, _dashing,
                new FuncPredicate(() => _movement.IsDashing && _actionSystem.CurrentRuntime == null));


            // =====================
            // 3. JUMP / FALL
            // =====================
            _stateMachine.AddTransition(_idle, _jumping,
                new FuncPredicate(() =>
                    !_idle.IsGrounded && _rb.linearVelocity.y > 0.01f && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_walking, _jumping,
                new FuncPredicate(() =>
                    !_walking.IsGrounded && _rb.linearVelocity.y > 0.01f && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_jumping, _falling,
                new FuncPredicate(() => _jumping.IsFalling && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_idle, _falling,
                new FuncPredicate(() => !_idle.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_walking, _falling,
                new FuncPredicate(() => !_walking.IsGrounded && _actionSystem.CurrentRuntime == null));


            // =====================
            // 4. GROUND MOVEMENT
            // =====================
            _stateMachine.AddTransition(_idle, _walking,
                new FuncPredicate(() => _idle.HasInput && _idle.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_walking, _stopping,
                new FuncPredicate(() =>
                    !_movement.HasMoveInput && _movement.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_stopping, _idle,
                new FuncPredicate(() =>
                    _stopping.IsDone && !_movement.HasMoveInput && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_stopping, _walking,
                new FuncPredicate(() =>
                    _movement.HasMoveInput && _movement.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_falling, _landing,
                new FuncPredicate(() => _movement.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_landing, _idle,
                new FuncPredicate(() =>
                    _landing.IsDone && !_movement.HasMoveInput && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_landing, _walking,
                new FuncPredicate(() =>
                    _landing.IsDone && _movement.HasMoveInput && _actionSystem.CurrentRuntime == null));


            // =====================
            // 5. EXIT SPECIAL STATES
            // =====================
            _stateMachine.AddTransition(_attacking, _idle,
                new FuncPredicate(() => !_attacking.IsAttacking));

            _stateMachine.AddTransition(_hurt, _jumping,
                new FuncPredicate(() => _hurt.IsDone && !_movement.IsGrounded && _rb.linearVelocity.y > 0.01f));

            _stateMachine.AddTransition(_hurt, _falling,
                new FuncPredicate(() => _hurt.IsDone && !_movement.IsGrounded && _rb.linearVelocity.y <= 0.01f));

            _stateMachine.AddTransition(_hurt, _walking,
                new FuncPredicate(() => _hurt.IsDone && _movement.IsGrounded && _movement.HasMoveInput));

            _stateMachine.AddTransition(_hurt, _idle,
                new FuncPredicate(() => _hurt.IsDone && _movement.IsGrounded && !_movement.HasMoveInput));

            _stateMachine.AddTransition(_dashing, _falling,
                new FuncPredicate(() =>
                    _dashing.IsDone && !_movement.IsGrounded && _actionSystem.CurrentRuntime == null));

            _stateMachine.AddTransition(_dashing, _idle,
                new FuncPredicate(() =>
                    _dashing.IsDone && _movement.IsGrounded && _actionSystem.CurrentRuntime == null));
        }

        void Update()
        {
            _stateMachine.Execute();
        }

        private void HandleDamaged(HitInfo hitInfo)
        {
            if (_health == null || _health.IsDead || hurtData == null)
            {
                return;
            }

            _actionSystem?.CurrentRuntime?.ForceEnd();
            _hurtRequested = true;
        }

        private bool ConsumeHurtRequest()
        {
            if (!_hurtRequested)
            {
                return false;
            }

            _hurtRequested = false;
            return true;
        }
    }
}
