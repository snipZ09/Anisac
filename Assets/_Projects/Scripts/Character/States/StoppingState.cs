using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class StoppingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _stoppingData;
        private float _timer;
        private float _duration;

        public bool IsDone { get; private set; }

        public StoppingState(AnimationDriver animationDriver, ActionData stoppingData)
        {
            _animationDriver = animationDriver;
            _stoppingData = stoppingData;
        }

        public override void EnterState()
        {
            _timer = 0f;
            IsDone = false;
            _duration = _stoppingData.spriteAnimation.Length / _stoppingData.animationFrameRate;
            _animationDriver.PlayLocomotionAnimation(_stoppingData);
        }

        public override void ExecuteState()
        {
            _timer += Time.deltaTime;

            if (_timer >= _duration)
                IsDone = true;
        }
    }
}