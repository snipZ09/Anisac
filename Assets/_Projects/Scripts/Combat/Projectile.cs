using Game.Infrastructure;
using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private Sprite[] flightFrames;
        [SerializeField] private float animationFrameRate = 15f;
        [SerializeField] private AudioClip impactClip;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private GameObject _owner;
        private float _damageAmount;
        private DamageType _damageType;
        private Vector2 _knockback;
        private int _currentFrame;
        private float _frameTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (flightFrames == null || flightFrames.Length == 0 || _spriteRenderer == null) return;

            _frameTimer += Time.deltaTime;
            if (_frameTimer < 1f / animationFrameRate) return;

            _frameTimer = 0f;
            _currentFrame = (_currentFrame + 1) % flightFrames.Length;
            _spriteRenderer.sprite = flightFrames[_currentFrame];
        }

        public void Launch(Vector2 direction, float speed, GameObject owner,
            float damageAmount, DamageType damageType, Vector2 knockback)
        {
            _owner = owner;
            _damageAmount = damageAmount;
            _damageType = damageType;
            _knockback = knockback;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = direction.x < 0f;
            }

            _rb.linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null) return;

            damageable.TakeDamage(new HitInfo
            {
                DamageCauser = _owner,
                DamageType = _damageType,
                DamageAmount = _damageAmount,
                HitLocation = other.ClosestPoint(transform.position),
                KnockbackForce = _knockback
            });

            AudioManager.Instance?.PlaySfx(impactClip);
            Destroy(gameObject);
        }
    }
}
