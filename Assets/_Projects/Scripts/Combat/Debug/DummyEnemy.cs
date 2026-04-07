using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    public class DummyEnemy : MonoBehaviour, IDamageable
    {
        public void TakeDamage(HitInfo hitInfo)
        {
            Debug.Log($"DummyEnemy took {hitInfo.DamageAmount} damage from {hitInfo.DamageCauser.name} with {hitInfo.DamageType} damage.");
        }
    }
}