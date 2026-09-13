using UnityEngine.Serialization;
using UnityEngine;

namespace Game.Shared
{
    [CreateAssetMenu(fileName = "MovementStats", menuName = "Scriptable Objects/MovementStats")]
    public class MovementStats : ScriptableObject
    {
        [Header("Movement")]
        public float acceleration;
        [Header("Jumping")]
        public float jumpForce;
        public float jumpCooldown;
        public int maxAirJumps;
        [Header("Dashing")]
        public float dashForce;
        [FormerlySerializedAs("dashDuration")]
        public float dashMoveDuration;
        public float dashCooldown;
        public float maxDashCount;
    }
}
