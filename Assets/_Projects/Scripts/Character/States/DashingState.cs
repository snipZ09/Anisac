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

        public bool IsDashing => _movement.IsDashing;

        public DashingState(AnimationDriver animationDriver, ActionData dashingData, CharacterMovement movement)
        {
            _animationDriver = animationDriver;
            _dashingData = dashingData;
            _movement = movement;
        }

        public override void EnterState()
        {
            _animationDriver.PlayAnimation(_dashingData);
        }
    }
}