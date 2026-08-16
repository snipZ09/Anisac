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

        public bool IsDone { get; private set; }

        public HurtState(AnimationDriver animationDriver, ActionData hurtData, CharacterMovement movement)
        {
            _animationDriver = animationDriver;
            _hurtData = hurtData;
            _movement = movement;
        }

        public override void EnterState()
        {
            _timer = 0f;
            IsDone = false;
            _duration = _hurtData.spriteAnimation.Length / _hurtData.animationFrameRate;
            _movement.SetMovementLock(true);
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
