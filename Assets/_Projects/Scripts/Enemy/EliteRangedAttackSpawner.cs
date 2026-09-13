using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Enemy
{
    public class EliteRangedAttackSpawner : MonoBehaviour
    {
        [SerializeField] private ActionData rangedActionData;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float projectileSpeed = 6f;
        [SerializeField] private EliteEnemyController elite;

        private IActionRunner _actionRunner;

        private void Start()
        {
            _actionRunner = GetComponent<IActionRunner>();

            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted += HandleActionStarted;
            }
        }

        private void OnDestroy()
        {
            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted -= HandleActionStarted;
            }
        }

        private void HandleActionStarted(ActionRuntime runtime)
        {
            if (runtime.Data != rangedActionData) return;

            runtime.OnPhaseChanged += OnPhaseChanged;

            void OnPhaseChanged(ActionPhase phase)
            {
                if (phase == ActionPhase.Active)
                {
                    SpawnProjectile(runtime.Data);
                }

                if (phase != ActionPhase.Active)
                {
                    runtime.OnPhaseChanged -= OnPhaseChanged;
                }
            }
        }

        private void SpawnProjectile(ActionData data)
        {
            if (projectilePrefab == null || spawnPoint == null) return;

            Vector2 direction = new Vector2(elite != null ? elite.FacingDirectionX : 1f, 0f);
            Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            projectile.Launch(direction, projectileSpeed, gameObject, data.damageAmount, data.damageType, data.knockback);
        }
    }
}
