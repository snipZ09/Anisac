using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class FallingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _fallingData;
        private readonly CharacterMovement _movement;

        public bool IsGrounded => _movement.IsGrounded;

        public FallingState(AnimationDriver animationDriver, ActionData fallingData, CharacterMovement movement)
        {
            _animationDriver = animationDriver;
            _fallingData = fallingData;
            _movement = movement;
        }

        public override void EnterState()
        {
            _animationDriver.PlayAnimation(_fallingData);
        }
    }
}