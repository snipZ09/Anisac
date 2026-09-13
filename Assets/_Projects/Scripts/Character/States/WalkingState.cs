using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class WalkingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _walkingData;
        private readonly CharacterMovement _movement;
        private readonly Rigidbody2D _rb;

        public bool HasInput => _movement.HasMoveInput;

        public bool IsGrounded => _movement.IsGrounded;

        public WalkingState(AnimationDriver animationDriver, ActionData walkingData, CharacterMovement movement, Rigidbody2D rb)
        {
            _animationDriver = animationDriver;
            _walkingData = walkingData;
            _movement = movement;
            _rb = rb;
        }

        public override void EnterState()
        {
            _animationDriver.PlayLocomotionAnimation(_walkingData);
        }
    }
}