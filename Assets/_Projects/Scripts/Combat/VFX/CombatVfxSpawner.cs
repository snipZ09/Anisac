using UnityEngine;

namespace Game.Combat
{
    public class CombatVfxSpawner : MonoBehaviour
    {
        [SerializeField] private Vector2 spawnOffset;

        private SpriteRenderer _spriteRenderer;
        private IActionRunner _actionRunner;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _actionRunner = GetComponent<IActionRunner>();
        }

        private void OnEnable()
        {
            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted += HandleActionStarted;
            }
        }

        private void OnDisable()
        {
            if (_actionRunner != null)
            {
                _actionRunner.OnActionStarted -= HandleActionStarted;
            }
        }

        private void HandleActionStarted(ActionRuntime runtime)
        {
            GameObject prefab = runtime.Data.vfxPrefab;
            if (prefab == null) return;

            bool facingLeft = _spriteRenderer != null && _spriteRenderer.flipX;
            float offsetX = facingLeft ? -spawnOffset.x : spawnOffset.x;
            Vector3 spawnPosition = transform.position + new Vector3(offsetX, spawnOffset.y, 0f);

            GameObject vfx = Instantiate(prefab, spawnPosition, Quaternion.identity);

            Vector3 scale = vfx.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingLeft ? -1f : 1f);
            vfx.transform.localScale = scale;
        }
    }
}
