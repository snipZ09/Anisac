using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class LandingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _landingData;
        private float _timer;
        private float _duration;

        public bool IsDone { get; private set; }

        public LandingState(AnimationDriver animationDriver, ActionData stoppingData)
        {
            _animationDriver = animationDriver;
            _landingData = stoppingData;
        }

        public override void EnterState()
        {
            _timer = 0f;
            IsDone = false;
            _duration = _landingData.spriteAnimation.Length / _landingData.animationFrameRate;
            _animationDriver.PlayLocomotionAnimation(_landingData);
        }

        public override void ExecuteState()
        {
            _timer += Time.deltaTime;

            if (_timer >= _duration)
                IsDone = true;
        }
    }
}