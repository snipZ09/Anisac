using Game.Infrastructure;
using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    public class CombatSfxPlayer : MonoBehaviour
    {
        [Header("Health SFX")]
        [SerializeField] private AudioClip hurtClip;
        [SerializeField] private AudioClip deathClip;

        private Health _health;
        private IActionRunner _actionRunner;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _actionRunner = GetComponent<IActionRunner>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
                _health.OnDied += HandleDied;
            }

            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted += HandleActionStarted;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
                _health.OnDied -= HandleDied;
            }

            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted -= HandleActionStarted;
            }
        }

        private void HandleDamaged(HitInfo hitInfo) => AudioManager.Instance?.PlaySfx(hurtClip);

        private void HandleDied() => AudioManager.Instance?.PlaySfx(deathClip);

        private void HandleActionStarted(ActionRuntime runtime) => AudioManager.Instance?.PlaySfx(runtime.Data.sfxClip);
    }
}
