using UnityEngine;

namespace Game.Shared
{
    public struct HitInfo
    {
        public GameObject DamageCauser;
        public DamageType DamageType;
        public float DamageAmount;
        public Vector2 HitLocation;
        public Vector2 KnockbackForce;
    }
}
