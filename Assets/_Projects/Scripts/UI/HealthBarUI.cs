using Game.Combat;
using Game.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Image fillImage;

        private void Awake()
        {
            health ??= GetComponentInParent<Health>();
        }

        private void OnEnable()
        {
            if (health == null) return;

            health.OnDamaged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            if (health == null) return;

            health.OnDamaged -= HandleHealthChanged;
        }

        private void Start()
        {
            UpdateFill();
        }

        private void HandleHealthChanged(HitInfo hitInfo)
        {
            UpdateFill();
        }

        private void UpdateFill()
        {
            if (fillImage == null || health == null || health.MaxHealth <= 0f) return;

            fillImage.fillAmount = health.CurrentHealth / health.MaxHealth;
        }
    }
}
