using UnityEngine;
using Game.Shared;

namespace Game.Character
{
    public class CharacterMovement : MonoBehaviour, IMovable
    {
        private Rigidbody2D _rigidbody;
        private int _jumpCount;
        private float _jumpCountDown;
        private int _dashCount;
        private float _dashCountDown;
        private bool _isDashing;
        private float _dashTimer;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius;

        public MovementStats movementStats;

        public bool IsGrounded { get; private set; }

        public bool IsDashing => _isDashing;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            movementStats = Instantiate(movementStats);
        }

        private void Update()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (IsGrounded)
                _dashCount = 0;


            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0)
                    _isDashing = false;
            }


            if (_dashCountDown > 0)
            {
                _dashCountDown -= Time.deltaTime;
            }
        }

        public void OnMove(Vector2 direction)
        {
            if (_isDashing) return;
            _rigidbody.linearVelocity = new Vector2(direction.x * movementStats.acceleration, _rigidbody.linearVelocity.y);
        }

        public void OnJump()
        {
            if (_jumpCountDown > 0) return;
            if (IsGrounded)
            {
                _jumpCount = 1;
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, movementStats.jumpForce);
                return;
            }
            else
            {
                if (_jumpCount > movementStats.maxAirJumps) return;
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, movementStats.jumpForce);
                _jumpCount++;
                _jumpCountDown = movementStats.jumpCooldown;
            }
        }

        public void OnDash(Vector2 direction)
        {
            if (_dashCountDown > 0) return;

            if (_dashCount > movementStats.maxDashCount) return;
            _rigidbody.linearVelocity = direction.normalized * movementStats.dashForce;
            _dashCount++;
            _dashCountDown = movementStats.dashCooldown;
            _isDashing = true;
            _dashTimer = movementStats.dashDuration;
        }
    }
}