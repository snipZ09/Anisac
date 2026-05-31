using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class IdleState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _idleData;
        private readonly CharacterMovement _movement;
        private readonly Rigidbody2D _rb;

        public bool HasInput => Mathf.Abs(_rb.linearVelocity.x) > 0.01f;
        public bool IsGrounded => _movement.IsGrounded;

        public IdleState(AnimationDriver animationDriver, ActionData idleData, CharacterMovement movement, Rigidbody2D rb)
        {
            _animationDriver = animationDriver;
            _idleData = idleData;
            _movement = movement;
            _rb = rb;
        }

        public override void EnterState()
        {
            _animationDriver.PlayAnimation(_idleData);
        }
    }
}

