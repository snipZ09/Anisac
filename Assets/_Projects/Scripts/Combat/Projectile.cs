using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;

        private Rigidbody2D _rb;
        private GameObject _owner;
        private float _damageAmount;
        private DamageType _damageType;
        private Vector2 _knockback;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, float speed, GameObject owner,
            float damageAmount, DamageType damageType, Vector2 knockback)
        {
            _owner = owner;
            _damageAmount = damageAmount;
            _damageType = damageType;
            _knockback = knockback;
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

            Destroy(gameObject);
        }
    }
}
