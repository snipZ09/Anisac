using Game.Combat;
using Game.Shared;

namespace Game.Character
{
    public class AttackingState : BaseState
    {
        private readonly ActionSystem _actionSystem;
        private readonly CharacterMovement _characterMovement;

        public bool IsAttacking => _actionSystem.CurrentRuntime != null;

        public AttackingState(ActionSystem actionSystem,  CharacterMovement characterMovement)
        {
            _actionSystem = actionSystem;
            _characterMovement = characterMovement;
        }

        public override void EnterState()
        {
            _characterMovement.SetMovementLock(true);
        }

        public override void ExitState()
        {
            _characterMovement.SetMovementLock(false);
        }
    }
}
