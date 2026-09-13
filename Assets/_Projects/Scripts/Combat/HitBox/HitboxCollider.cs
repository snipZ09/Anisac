using System;
using UnityEngine;

namespace Game.Combat
{
    public class HitboxCollider : MonoBehaviour
    {
        [SerializeField] private bool drawDebugGizmo = true;
        [SerializeField] private Color activeColor = new(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color inactiveColor = new(1f, 0f, 0f, 0.15f);

        private BoxCollider2D _boxCollider;
        private Vector2 _baseOffset;

        public event Action<Collider2D> OnHit;
        public Collider2D Collider => _boxCollider;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            _baseOffset = _boxCollider.offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnHit?.Invoke(other);
        }

        public void SetFacing(float directionX)
        {
            if (_boxCollider == null || Mathf.Abs(directionX) <= 0.01f) return;

            Vector2 offset = _baseOffset;
            offset.x = Mathf.Abs(_baseOffset.x) * Mathf.Sign(directionX);
            _boxCollider.offset = offset;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmo) return;

            var boxCollider = _boxCollider != null ? _boxCollider : GetComponent<BoxCollider2D>();
            if (boxCollider == null) return;

            Gizmos.color = boxCollider.enabled ? activeColor : inactiveColor;

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
            Gizmos.matrix = previousMatrix;
        }
    }
}
