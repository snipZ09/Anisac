using System;
using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool logDebug;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<HitInfo> OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(HitInfo hitInfo)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Max(CurrentHealth - hitInfo.DamageAmount, 0f);
            OnDamaged?.Invoke(hitInfo);

            if (logDebug)
            {
                Debug.Log(
                    $"{name} took {hitInfo.DamageAmount} {hitInfo.DamageType} damage. HP: {CurrentHealth}/{MaxHealth}",
                    this
                );
            }

            if (IsDead)
            {
                if (logDebug)
                {
                    Debug.Log($"{name} died.", this);
                }

                OnDied?.Invoke();
            }
        }
    }
}
