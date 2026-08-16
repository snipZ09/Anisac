using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class DashingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _dashingData;
        private readonly CharacterMovement _movement;
        private float _timer;
        private float _duration;

        public bool IsDashing => _movement.IsDashing;
        public bool IsDone { get; private set; }

        public DashingState(AnimationDriver animationDriver, ActionData dashingData, CharacterMovement movement)
        {
            _animationDriver = animationDriver;
            _dashingData = dashingData;
            _movement = movement;
        }

        public override void EnterState()
        {
            _timer = 0f;
            IsDone = false;
            _duration = _dashingData.spriteAnimation.Length / _dashingData.animationFrameRate;
            _animationDriver.PlayLocomotionAnimation(_dashingData);
        }

        public override void ExecuteState()
        {
            _timer += Time.deltaTime;

            if (_timer >= _duration)
            {
                IsDone = true;
            }
        }
    }
}
