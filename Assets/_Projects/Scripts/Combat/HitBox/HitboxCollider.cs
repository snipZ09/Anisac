using System;
using UnityEngine;

namespace Game.Combat
{
    public class HitboxCollider : MonoBehaviour
    {
        public event Action<Collider2D> OnHit;
        public Collider2D Collider => GetComponent<Collider2D>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnHit?.Invoke(other);
        }
    }
}