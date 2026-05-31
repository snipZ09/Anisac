using Game.Combat;
using Game.Shared;

namespace Game.Character
{
    public class AttackingState : BaseState
    {
        private readonly ActionSystem _actionSystem;

        public bool IsAttacking => _actionSystem.CurrentRuntime != null;

        public AttackingState(ActionSystem actionSystem)
        {
            _actionSystem = actionSystem;
        }
    }
}
