using Game.Shared;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class HitboxSystem : MonoBehaviour
    {
        private HitboxCollider[] hitBoxs;
        private IActionRunner actionRunner;
        private ActionRuntime _currentRuntime;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            actionRunner = GetComponent<IActionRunner>();
            hitBoxs = GetComponentsInChildren<HitboxCollider>();
            foreach (var hitBox in hitBoxs)
            {
                hitBox.Collider.enabled = false;
                hitBox.OnHit += (other) =>
                {
                    var actionData = _currentRuntime.Data;
                    float knockDirX = Mathf.Sign(other.transform.position.x - hitBox.transform.position.x);
                    var hitInfo = new HitInfo
                    {
                        DamageCauser = gameObject,
                        DamageType = actionData.damageType,
                        DamageAmount = actionData.damageAmount,
                        HitLocation = other.ClosestPoint(hitBox.transform.position),
                        KnockbackForce = new Vector2(Mathf.Abs(actionData.knockback.x) * knockDirX, actionData.knockback.y)
                    };
                    other.GetComponentInParent<IDamageable>()?.TakeDamage(hitInfo);
                };
            }

            actionRunner.OnActionStarted += (runtime) =>
            {
                // Disable tất cả hitbox trước
                foreach (var hitBox in hitBoxs)
                    hitBox.Collider.enabled = false;

                // Unsubscribe cũ, subscribe mới
                if (_currentRuntime != null)
                    _currentRuntime.OnPhaseChanged -= OnActionPhaseChanged;
                _currentRuntime = runtime;

                actionRunner.CurrentRuntime.OnPhaseChanged += OnActionPhaseChanged;
            };
        }

        private void OnActionPhaseChanged(ActionPhase phase)
        {
            switch (phase)
            {
                case ActionPhase.Startup:
                case ActionPhase.Recovery:
                case ActionPhase.Done:
                    actionRunner.CurrentRuntime.OnPhaseChanged -= OnActionPhaseChanged;
                    foreach (var hitBox in hitBoxs)
                    {
                        hitBox.Collider.enabled = false;
                    }
                    break;
                case ActionPhase.Active:
                    int index = actionRunner.CurrentRuntime.Data.hitboxIndex;
                    if (index >= 0 && index < hitBoxs.Length)
                    {
                        hitBoxs[index].Collider.enabled = true;
                    }
                    break;
            }
        }
    }
}
