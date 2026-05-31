using Game.Combat;
using Game.Shared;
using UnityEngine;

namespace Game.Character
{
    public class JumpingState : BaseState
    {
        private readonly AnimationDriver _animationDriver;
        private readonly ActionData _jumpingData;
        private readonly Rigidbody2D _rb;

        public bool IsFalling => _rb.linearVelocity.y < -0.01f;

        public JumpingState(AnimationDriver animationDriver, ActionData jumpingData, Rigidbody2D rb)
        {
            _animationDriver = animationDriver;
            _jumpingData = jumpingData;
            _rb = rb;
        }

        public override void EnterState()
        {
            _animationDriver.PlayAnimation(_jumpingData);
        }
    }
}

