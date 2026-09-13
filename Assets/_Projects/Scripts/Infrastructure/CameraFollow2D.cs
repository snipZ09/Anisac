using UnityEngine;

namespace Game.Infrastructure
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float smoothTime = 0.12f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = new(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                smoothTime
            );
        }
    }
}
