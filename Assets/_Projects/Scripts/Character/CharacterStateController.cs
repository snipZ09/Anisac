using Game.Combat;
using UnityEngine;
using Game.Shared;

namespace Game.Character
{
    public class CharacterStateController : MonoBehaviour
    {
        [SerializeField] private ActionData idleData;
        [SerializeField] private ActionData walkingData;
        [SerializeField] private ActionData jumpingData;
        [SerializeField] private ActionData fallingData;
        [SerializeField] private ActionData dashingData;

        private AnimationDriver _animationDriver;
        private CharacterMovement _movement;
        private ActionSystem _actionSystem;
        private Rigidbody2D _rb;
        private StateMachine _stateMachine;

        private IdleState _idle;
        private WalkingState _walking;
        private JumpingState _jumping;
        private FallingState _falling;
        private DashingState _dashing;
        private AttackingState _attacking;


        private void Awake()
        {
            _animationDriver = GetComponent<AnimationDriver>();
            _movement = GetComponent<CharacterMovement>();
            _actionSystem = GetComponent<ActionSystem>();
            _rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            // Initialize states
            _idle = new IdleState(_animationDriver, idleData, _movement, _rb);
            _walking = new WalkingState(_animationDriver, walkingData, _movement, _rb);
            _jumping = new JumpingState(_animationDriver, jumpingData, _rb);
            _falling = new FallingState(_animationDriver, fallingData, _movement);
            _dashing = new DashingState(_animationDriver, dashingData, _movement);
            _attacking = new AttackingState(_actionSystem);

            // Initialize state machine
            _stateMachine = new StateMachine(_idle);

            // Add transitions
            _stateMachine.AddTransition(_idle, _walking, new FuncPredicate(() => _idle.HasInput));

            _stateMachine.AddTransition(_walking, _idle,
                new FuncPredicate(() => !_walking.HasInput));

            _stateMachine.AddTransition(_idle, _jumping, new FuncPredicate(() => !_idle.IsGrounded && _rb.linearVelocity.y > 0.01f));

            _stateMachine.AddTransition(_walking, _jumping,
                new FuncPredicate(() => !_walking.IsGrounded && _rb.linearVelocity.y > 0.01f));

            _stateMachine.AddTransition(_idle, _falling,
                new FuncPredicate(() => !_idle.IsGrounded));

            _stateMachine.AddTransition(_walking, _falling,
                new FuncPredicate(() => !_walking.IsGrounded));

            _stateMachine.AddTransition(_jumping, _falling,
                new FuncPredicate(() => _jumping.IsFalling));

            _stateMachine.AddTransition(_falling, _idle,
                new FuncPredicate(() => _falling.IsGrounded));

            _stateMachine.AddTransition(_dashing, _idle,
                new FuncPredicate(() => !_dashing.IsDashing));

            _stateMachine.AddTransition(_idle, _dashing, new FuncPredicate(() => _movement.IsDashing));

            _stateMachine.AddTransition(_walking, _dashing,
                new FuncPredicate(() => _movement.IsDashing));

            _stateMachine.AddTransition(_falling, _dashing,
                new FuncPredicate(() => _movement.IsDashing));

            _stateMachine.AddTransition(_idle, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_walking, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_falling, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_attacking, _idle,
                new FuncPredicate(() => !_attacking.IsAttacking));

            _stateMachine.AddTransition(_idle, _attacking, new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_walking, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_jumping, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_falling, _attacking,
                new FuncPredicate(() => _actionSystem.CurrentRuntime != null));

            _stateMachine.AddTransition(_attacking, _idle,
                new FuncPredicate(() => !_attacking.IsAttacking));
        }

        void Update()
        {
            _stateMachine.Execute();
        }
    }
}
