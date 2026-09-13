using Game.Shared;
using Game.Combat;
using UnityEngine;

namespace Game.Character
{
    public class CharacterMovement : MonoBehaviour, IMovable
    {
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private HitboxCollider[] _hitboxColliders;
        private int _airJumpUsed;
        private float _airJumpCooldown;
        private int _dashCount;
        private float _dashCountDown;
        private bool _isDashing;
        private float _dashTimer;
        private float _defaultGravityScale;
        private Vector2 _moveInput;
        private Vector2 _dashDirection;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius;

        public MovementStats movementStats;

        public bool IsGrounded { get; private set; }

        public bool IsDashing => _isDashing;

        public bool HasMoveInput => Mathf.Abs(_moveInput.x) > 0.01f;

        public Vector2 FacingDirection { get; private set; } = Vector2.right;

        public bool IsMovementLocked { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _hitboxColliders = GetComponentsInChildren<HitboxCollider>();
            _defaultGravityScale = _rigidbody.gravityScale;
            movementStats = Instantiate(movementStats);
        }

        private void Update()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius,
                groundLayer);

            if (IsGrounded)
            {
                _airJumpUsed = 0;
                _airJumpCooldown = 0;
                _dashCount = 0;
            }

            if (_airJumpCooldown > 0)
            {
                _airJumpCooldown -= Time.deltaTime;
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0)
                {
                    _isDashing = false;
                    _rigidbody.gravityScale = _defaultGravityScale;
                    _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);
                    _dashCountDown = movementStats.dashCooldown;
                }
            }
            
            if (_dashCountDown > 0)
            {
                _dashCountDown -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (!_isDashing) return;

            _rigidbody.linearVelocity = new Vector2(_dashDirection.x * movementStats.dashForce, 0f);
        }

        public void OnMove(Vector2 direction)
        {
            if (_isDashing || IsMovementLocked) return;

            _moveInput = direction;

            if (Mathf.Abs(direction.x) > 0.01f)
            {
                FacingDirection = direction.x > 0 ? Vector2.right : Vector2.left;
                UpdateFacingVisuals(direction.x);
            }


            _rigidbody.linearVelocity =
                new Vector2(direction.x * movementStats.acceleration, _rigidbody.linearVelocity.y);
        }

        public void OnJump()
        {
            if (IsGrounded)
            {
                DoJump();
                return;
            }

            if (_airJumpCooldown > 0)
                return;

            if (_airJumpUsed >= movementStats.maxAirJumps)
                return;

            DoJump();
            _airJumpUsed++;
            _airJumpCooldown = movementStats.jumpCooldown;
        }

        private void DoJump()
        {
            _rigidbody.linearVelocity = new Vector2(
                _rigidbody.linearVelocity.x,
                movementStats.jumpForce
            );
        }

        private void UpdateFacingVisuals(float directionX)
        {
            bool isFacingLeft = directionX < 0;
            _spriteRenderer.flipX = isFacingLeft;

            foreach (HitboxCollider hitboxCollider in _hitboxColliders)
            {
                hitboxCollider.SetFacing(directionX);
            }
        }

        public void OnDash(Vector2 direction)
        {
            if (_isDashing || _dashCountDown > 0 || IsMovementLocked) return;

            if (_dashCount >= movementStats.maxDashCount) return;

            _dashDirection = direction.sqrMagnitude > 0.01f
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : FacingDirection;

            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = new Vector2(_dashDirection.x * movementStats.dashForce, 0f);
            _dashCount++;
            _isDashing = true;
            _dashTimer = movementStats.dashMoveDuration;
        }
        
        public void ApplyKnockback(Vector2 force)
        {
            _rigidbody.linearVelocity = force;
        }

        public void SetMovementLock(bool isLocked)
        {
            IsMovementLocked = isLocked;

            if (isLocked && IsGrounded)
            {
                _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);
            }
        }
    }
}
