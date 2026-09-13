using UnityEngine;

namespace Game.Shared
{
    public interface IMovable
    {
        bool IsGrounded { get; }
        void OnMove(Vector2 direction);
        void OnJump();
    }
}