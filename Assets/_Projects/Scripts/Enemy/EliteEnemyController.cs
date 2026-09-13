using Game.Combat;
using Game.Shared;
using System.Collections;
using UnityEngine;

namespace Game.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ActionSystem))]
    [RequireComponent(typeof(InputBuffer))]
    public class EliteEnemyController : MonoBehaviour
    {
        private enum EliteAnimationState
        {
            None,
            Idle,
            Run,
            Attack,
            Hurt,
            Death
        }

        [SerializeField] private Health health;
        [SerializeField] private Transform player;
        [SerializeField] private Rigidbody2D rigidbody2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AnimationDriver animationDriver;
        [SerializeField] private ActionSystem actionSystem;
        [SerializeField] private ActionData idleAnimation;
        [SerializeField] private ActionData runAnimation;
        [SerializeField] private ActionData hurtAnimation;
        [SerializeField] private ActionData deathAnimation;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private ActionData dashAtkAnimation;
        [SerializeField] private float detectRange = 8f;
        [SerializeField] private float loseDetectBuffer = 1f;
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float meleeRange = 1.8f;
        [SerializeField] private float dashRange = 4f;
        [SerializeField] private float dashForce = 10f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool logDebug;
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.08f;

        private HitboxCollider[] hitboxColliders;
        private Color _originalColor;
        private Coroutine _hitFlashRoutine;
        private bool _lastIsPlayerInRange;
        private float _attackCooldownTimer;
        private float _hurtAnimationTimer;
        private bool _isDead;
        private bool _wasBusy;
        private bool _isDashMoving;
        private float _dashDirectionX;
        private EliteAnimationState _currentAnimationState;

        public bool IsPlayerInRange { get; private set; }
        public bool IsPlayerInMeleeRange { get; private set; }
        public float FacingDirectionX { get; private set; } = 1f;

        private void Awake()
        {
            health ??= GetComponent<Health>();
            rigidbody2D = GetComponent<Rigidbody2D>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            animationDriver ??= GetComponent<AnimationDriver>();
            actionSystem ??= GetComponent<ActionSystem>();
            hitboxColliders = GetComponentsInChildren<HitboxCollider>();
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

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
                health.OnDied += HandleDied;
            }

            if (actionSystem != null)
            {
                actionSystem.OnActionStarted += HandleActionStarted;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDied -= HandleDied;
            }

            if (actionSystem != null)
            {
                actionSystem.OnActionStarted -= HandleActionStarted;
            }
        }

        private void HandleActionStarted(ActionRuntime runtime)
        {
            if (runtime.Data != dashAtkAnimation) return;

            _dashDirectionX = FacingDirectionX;
            _isDashMoving = true;
            runtime.OnPhaseChanged += OnDashPhaseChanged;

            void OnDashPhaseChanged(ActionPhase phase)
            {
                if (phase != ActionPhase.Startup)
                {
                    _isDashMoving = false;
                    runtime.OnPhaseChanged -= OnDashPhaseChanged;
                }
            }
        }

        private void Update()
        {
            if (player == null)
            {
                TryResolvePlayer();
            }

            if (_isDead) return;
            if (player == null) return;

            if (_hurtAnimationTimer > 0f)
            {
                _hurtAnimationTimer -= Time.deltaTime;
            }

            UpdateDetectionAndFacing();
            TickAttackCooldown();
            UpdateAttackDecision();
            UpdateAnimationState();
        }

        private void UpdateDetectionAndFacing()
        {
            float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
            float currentDetectLimit = IsPlayerInRange ? detectRange + loseDetectBuffer : detectRange;
            IsPlayerInRange = horizontalDistanceToPlayer <= currentDetectLimit;
            IsPlayerInMeleeRange = horizontalDistanceToPlayer <= meleeRange;

            if (logDebug && IsPlayerInRange != _lastIsPlayerInRange)
            {
                Debug.Log(
                    $"{name} detect state changed. InRange={IsPlayerInRange}, DistanceX={horizontalDistanceToPlayer:F2}",
                    this
                );
            }

            _lastIsPlayerInRange = IsPlayerInRange;

            if (!IsPlayerInRange) return;

            FacingDirectionX = player.position.x >= transform.position.x ? 1f : -1f;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = FacingDirectionX < 0f;
            }

            foreach (var hitbox in hitboxColliders)
            {
                hitbox.SetFacing(FacingDirectionX);
            }
        }

        private void TickAttackCooldown()
        {
            bool busy = actionSystem.CurrentRuntime != null;

            if (_wasBusy && !busy)
            {
                _attackCooldownTimer = attackCooldown;
            }

            _wasBusy = busy;

            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateAttackDecision()
        {
            if (_hurtAnimationTimer > 0f) return;
            if (!IsPlayerInRange) return;

            bool busy = actionSystem.CurrentRuntime != null;

            if (!busy)
            {
                if (_attackCooldownTimer > 0f) return;

                float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);

                if (IsPlayerInMeleeRange)
                {
                    if (logDebug)
                    {
                        Debug.Log($"{name} light attack triggered via ActionSystem.", this);
                    }

                    actionSystem.OnInput(ActionType.Attack);
                }
                else if (horizontalDistanceToPlayer <= dashRange)
                {
                    if (logDebug)
                    {
                        Debug.Log($"{name} dash attack triggered via ActionSystem.", this);
                    }

                    actionSystem.OnInput(ActionType.Dash);
                }
                else
                {
                    if (logDebug)
                    {
                        Debug.Log($"{name} ranged attack triggered via ActionSystem.", this);
                    }

                    actionSystem.OnInput(ActionType.Ranged);
                }

                return;
            }

            // Đang bận: chỉ feed thêm input Attack nếu action hiện tại cũng là Attack (Light Atk),
            // để đúng lúc cancel-window của hit1 mở ra thì ComboResolver tự chain sang hit2.
            // Dash Atk/Ranged Atk không cancel được nên không feed gì thêm trong lúc chúng đang chạy.
            if (actionSystem.CurrentRuntime.Data.actionType == ActionType.Attack && IsPlayerInMeleeRange)
            {
                actionSystem.OnInput(ActionType.Attack);
            }
        }

        private void FixedUpdate()
        {
            if (rigidbody2D == null || _isDead) return;

            if (_hurtAnimationTimer > 0f)
            {
                return;
            }

            if (_isDashMoving)
            {
                rigidbody2D.linearVelocity = new Vector2(_dashDirectionX * dashForce, rigidbody2D.linearVelocity.y);
                return;
            }

            if (actionSystem.CurrentRuntime != null)
            {
                StopMovement();
                return;
            }

            if (player == null || !IsPlayerInRange || IsPlayerInMeleeRange)
            {
                StopMovement();
                return;
            }

            float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
            if (horizontalDistanceToPlayer <= stopDistance)
            {
                StopMovement();
                return;
            }

            rigidbody2D.linearVelocity = new Vector2(FacingDirectionX * moveSpeed, rigidbody2D.linearVelocity.y);
        }

        private void TryResolvePlayer()
        {
            if (player != null) return;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        private void HandleDamaged(HitInfo hitInfo)
        {
            if (_isDead) return;

            actionSystem.CurrentRuntime?.ForceEnd();

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = hitInfo.KnockbackForce;
            }

            float hurtDuration = GetAnimationDuration(hurtAnimation);
            if (hurtDuration > 0f)
            {
                _hurtAnimationTimer = hurtDuration;
                PlayHurtAnimation(hurtDuration);
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
            _hurtAnimationTimer = 0f;
            actionSystem.CurrentRuntime?.ForceEnd();
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

            if (actionSystem.CurrentRuntime != null)
            {
                _currentAnimationState = EliteAnimationState.Attack;
                return;
            }

            if (_hurtAnimationTimer > 0f)
            {
                PlayAnimationState(EliteAnimationState.Hurt);
                return;
            }

            if (player != null && IsPlayerInRange && !IsPlayerInMeleeRange)
            {
                float horizontalDistanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);
                if (horizontalDistanceToPlayer > stopDistance)
                {
                    PlayAnimationState(EliteAnimationState.Run);
                    return;
                }
            }

            PlayAnimationState(EliteAnimationState.Idle);
        }

        private void PlayAnimationState(EliteAnimationState state)
        {
            if (_currentAnimationState == state) return;

            ActionData data = state switch
            {
                EliteAnimationState.Idle => idleAnimation,
                EliteAnimationState.Run => runAnimation,
                EliteAnimationState.Hurt => hurtAnimation,
                EliteAnimationState.Death => deathAnimation,
                _ => null
            };

            if (data == null) return;

            _currentAnimationState = state;

            if (state == EliteAnimationState.Hurt || state == EliteAnimationState.Death)
            {
                animationDriver.PlayPriorityAnimation(data, GetAnimationDuration(data));
                return;
            }

            animationDriver.PlayLocomotionAnimation(data);
        }

        private float GetAnimationDuration(ActionData data)
        {
            if (data == null || data.spriteAnimation == null || data.spriteAnimation.Length == 0 ||
                data.animationFrameRate <= 0f)
            {
                return 0f;
            }

            return data.spriteAnimation.Length / data.animationFrameRate;
        }

        private void PlayHurtAnimation(float duration)
        {
            if (animationDriver == null || hurtAnimation == null) return;

            _currentAnimationState = EliteAnimationState.Hurt;
            animationDriver.PlayPriorityAnimation(hurtAnimation, duration);
        }

        private void PlayDeathAnimation()
        {
            if (animationDriver == null || deathAnimation == null) return;

            _currentAnimationState = EliteAnimationState.Death;
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
