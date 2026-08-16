using Game.Combat;
using Game.Shared;
using System.Collections;
using UnityEngine;

namespace Game.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        private enum EnemyAnimationState
        {
            None,
            Idle,
            Chase,
            Attack,
            Hurt,
            Death
        }

        [SerializeField] private Health health;
        [SerializeField] private Transform player;
        [SerializeField] private Rigidbody2D rigidbody2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AnimationDriver animationDriver;
        [SerializeField] private ActionData idleAnimation;
        [SerializeField] private ActionData chaseAnimation;
        [SerializeField] private ActionData attackAnimation;
        [SerializeField] private ActionData hurtAnimation;
        [SerializeField] private ActionData deathAnimation;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float detectRange = 6f;
        [SerializeField] private float loseDetectBuffer = 1f;
        [SerializeField] private float stopDistance = 1.25f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackHitDelay = 0.15f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private Vector2 attackKnockback = new(3f, 1.5f);
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private bool logDebug;
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.08f;

        private Color _originalColor;
        private Coroutine _hitFlashRoutine;
        private bool _lastIsPlayerInRange;
        private float _attackTimer;
        private float _attackAnimationTimer;
        private float _pendingAttackTimer;
        private bool _pendingAttackHit;
        private float _hurtAnimationTimer;
        private bool _isDead;
        private EnemyAnimationState _currentAnimationState;

        public bool IsPlayerInRange { get; private set; }
        public bool IsPlayerInAttackRange { get; private set; }

        private void Awake()
        {
            health ??= GetComponent<Health>();
            rigidbody2D = GetComponent<Rigidbody2D>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            animationDriver ??= GetComponent<AnimationDriver>();
            TryResolvePlayer();

            Debug.Assert(rigidbody2D != null, $"{name} is missing Rigidbody2D.", this);

            if (spriteRenderer != null)
            {
                _originalColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            TryResolvePlayer();
            UpdateAnimationState();
        }

        private void Update()
        {
            if (player == null)
            {
                TryResolvePlayer();
            }

            if (_isDead)
            {
                return;
            }

            if (player == null) return;

            if (_attackAnimationTimer > 0f)
            {
                _attackAnimationTimer -= Time.deltaTime;
            }

            if (_hurtAnimationTimer > 0f)
            {
                _hurtAnimationTimer -= Time.deltaTime;
            }

            if (_pendingAttackHit)
            {
                _pendingAttackTimer -= Time.deltaTime;
                if (_pendingAttackTimer <= 0f)
                {
                    _pendingAttackHit = false;

                    if (IsPlayerInAttackRange)
                    {
                        TryAttackPlayer();
                    }
                }
            }

            float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
            float currentDetectLimit = IsPlayerInRange ? detectRange + loseDetectBuffer : detectRange;
            IsPlayerInRange = horizontalDistanceToPlayer <= currentDetectLimit;
            IsPlayerInAttackRange = horizontalDistanceToPlayer <= attackRange;

            if (logDebug && IsPlayerInRange != _lastIsPlayerInRange)
            {
                Debug.Log(
                    $"{name} detect state changed. InRange={IsPlayerInRange}, DistanceX={horizontalDistanceToPlayer:F2}",
                    this
                );
            }

            _lastIsPlayerInRange = IsPlayerInRange;

            if (IsPlayerInRange)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = player.position.x < transform.position.x;
                }

                if (_attackTimer > 0f)
                {
                    _attackTimer -= Time.deltaTime;
                }

                if (_hurtAnimationTimer <= 0f && IsPlayerInAttackRange && _attackTimer <= 0f)
                {
                    _attackTimer = attackCooldown;
                    _pendingAttackHit = true;
                    _pendingAttackTimer = attackHitDelay;
                    _attackAnimationTimer = GetAnimationDuration(attackAnimation);
                    PlayAttackAnimation();

                    if (logDebug)
                    {
                        Debug.Log($"{name} attack triggered. Hit in {attackHitDelay:F2}s", this);
                    }
                }
            }

            UpdateAnimationState();
        }

        private void FixedUpdate()
        {
            if (rigidbody2D == null)
            {
                return;
            }

            if (_isDead)
            {
                return;
            }

            if (_hurtAnimationTimer > 0f)
            {
                StopMovement();
                return;
            }

            if (player == null || !IsPlayerInRange || IsPlayerInAttackRange)
            {
                StopMovement();
                return;
            }

            float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
            if (horizontalDistanceToPlayer <= stopDistance)
            {
                if (logDebug)
                {
                    Debug.Log($"{name} reached stop distance. DistanceX={horizontalDistanceToPlayer:F2}", this);
                }
                StopMovement();
                return;
            }

            float directionX = Mathf.Sign(player.position.x - transform.position.x);
            rigidbody2D.linearVelocity = new Vector2(directionX * moveSpeed, rigidbody2D.linearVelocity.y);

            if (logDebug)
            {
                Debug.Log(
                    $"{name} chasing. DistanceX={horizontalDistanceToPlayer:F2}, VelocityX={rigidbody2D.linearVelocity.x:F2}",
                    this
                );
            }
        }

        private void TryResolvePlayer()
        {
            if (player != null) return;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                if (logDebug)
                {
                    Debug.Log($"{name} resolved player reference: {player.name}", this);
                }
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDied -= HandleDied;
            }
        }

        private void HandleDamaged(HitInfo hitInfo)
        {
            if (_isDead) return;

            _pendingAttackHit = false;
            _pendingAttackTimer = 0f;
            _attackAnimationTimer = 0f;
            StopMovement();

            float hurtDuration = GetAnimationDuration(hurtAnimation);
            if (hurtDuration > 0f)
            {
                _hurtAnimationTimer = hurtDuration;
                PlayHurtAnimation();
            }

            if (spriteRenderer == null) return;

            if (_hitFlashRoutine != null)
            {
                StopCoroutine(_hitFlashRoutine);
            }

            _hitFlashRoutine = StartCoroutine(FlashHit());
        }

        private IEnumerator FlashHit()
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = _originalColor;
            _hitFlashRoutine = null;
        }

        private void HandleDied()
        {
            _isDead = true;
            _pendingAttackHit = false;
            _pendingAttackTimer = 0f;
            _attackAnimationTimer = 0f;
            _hurtAnimationTimer = 0f;
            StopMovement();

            float deathDuration = GetAnimationDuration(deathAnimation);
            if (deathDuration > 0f)
            {
                PlayDeathAnimation();
                StartCoroutine(FinishDeathAfter(deathDuration));
                return;
            }

            FinishDeath();
        }

        private void StopMovement()
        {
            if (rigidbody2D == null) return;
            rigidbody2D.linearVelocity = new Vector2(0f, rigidbody2D.linearVelocity.y);
        }

        private void UpdateAnimationState()
        {
            if (animationDriver == null) return;

            if (_attackAnimationTimer > 0f)
            {
                PlayAnimationState(EnemyAnimationState.Attack);
                return;
            }

            if (_hurtAnimationTimer > 0f)
            {
                PlayAnimationState(EnemyAnimationState.Hurt);
                return;
            }

            if (player != null && IsPlayerInRange && !IsPlayerInAttackRange)
            {
                float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
                if (horizontalDistanceToPlayer > stopDistance)
                {
                    PlayAnimationState(EnemyAnimationState.Chase);
                    return;
                }
            }

            PlayAnimationState(EnemyAnimationState.Idle);
        }

        private void PlayAnimationState(EnemyAnimationState state)
        {
            if (_currentAnimationState == state) return;

            ActionData data = state switch
            {
                EnemyAnimationState.Idle => idleAnimation,
                EnemyAnimationState.Chase => chaseAnimation,
                EnemyAnimationState.Attack => attackAnimation,
                EnemyAnimationState.Hurt => hurtAnimation,
                EnemyAnimationState.Death => deathAnimation,
                _ => null
            };

            if (data == null) return;

            _currentAnimationState = state;

            if (state == EnemyAnimationState.Attack)
            {
                animationDriver.PlayActionAnimation(data);
                return;
            }

            if (state == EnemyAnimationState.Hurt || state == EnemyAnimationState.Death)
            {
                animationDriver.PlayPriorityAnimation(data, -1f);
                return;
            }

            animationDriver.PlayLocomotionAnimation(data);
        }

        private void PlayAttackAnimation()
        {
            if (animationDriver == null || attackAnimation == null) return;

            _currentAnimationState = EnemyAnimationState.Attack;
            animationDriver.PlayActionAnimation(attackAnimation);
        }

        private float GetAnimationDuration(ActionData data)
        {
            if (data == null || data.spriteAnimation == null || data.spriteAnimation.Length == 0 || data.animationFrameRate <= 0f)
            {
                return 0f;
            }

            return data.spriteAnimation.Length / data.animationFrameRate;
        }

        private void TryAttackPlayer()
        {
            if (player == null) return;

            IDamageable damageable = player.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                if (logDebug)
                {
                    Debug.Log($"{name} could not find IDamageable on player.", this);
                }
                return;
            }

            Vector2 direction = player.position.x >= transform.position.x ? Vector2.right : Vector2.left;
            HitInfo hitInfo = new()
            {
                DamageCauser = gameObject,
                DamageType = DamageType.Physical,
                DamageAmount = attackDamage,
                HitLocation = player.position,
                KnockbackForce = new Vector2(direction.x * attackKnockback.x, attackKnockback.y)
            };

            damageable.TakeDamage(hitInfo);
        }

        private void PlayHurtAnimation()
        {
            if (animationDriver == null || hurtAnimation == null) return;

            _currentAnimationState = EnemyAnimationState.Hurt;
            animationDriver.PlayPriorityAnimation(hurtAnimation, -1f);
        }

        private void PlayDeathAnimation()
        {
            if (animationDriver == null || deathAnimation == null) return;

            _currentAnimationState = EnemyAnimationState.Death;
            animationDriver.PlayPriorityAnimation(deathAnimation, -1f);
        }

        private IEnumerator FinishDeathAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            FinishDeath();
        }

        private void FinishDeath()
        {
            if (destroyOnDeath)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
