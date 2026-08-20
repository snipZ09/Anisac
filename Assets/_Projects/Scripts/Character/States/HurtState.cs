using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class HurtState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _hurtData;
        private readonly CharacterMovement _movement;
        private float _timer;
        private float _duration;
        private Vector2 _pendingKnockback;

        public bool IsDone { get; private set; }

        public HurtState(AnimationDriver animationDriver, ActionData hurtData, CharacterMovement movement)
        {
            _animationDriver = animationDriver;
            _hurtData = hurtData;
            _movement = movement;
        }

        public void SetPendingKnockback(Vector2 knockback)
        {
            _pendingKnockback = knockback;
        }

        public override void EnterState()
        {
            _timer = 0f;
            IsDone = false;
            _duration = _hurtData.spriteAnimation.Length / _hurtData.animationFrameRate;
            _movement.SetMovementLock(true);
            _movement.ApplyKnockback(_pendingKnockback);
            _pendingKnockback = Vector2.zero;
            _animationDriver.PlayPriorityAnimation(_hurtData, _duration);
        }

        public override void ExecuteState()
        {
            _timer += Time.deltaTime;

            if (_timer >= _duration)
            {
                IsDone = true;
            }
        }

        public override void ExitState()
        {
            _movement.SetMovementLock(false);
        }
    }
}
