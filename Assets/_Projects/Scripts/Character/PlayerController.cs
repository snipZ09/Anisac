using Game.Combat;
using Game.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private ActionSystem actionSystem;
        [SerializeField] private CharacterMovement characterMovement;
        [SerializeField] private CharacterStateController characterStateController;
        [SerializeField] private Health health;
        [SerializeField] private AnimationDriver animationDriver;
        [SerializeField] private ActionData deathAnimation;
        private InputSystem_Actions _inputSystemActions;
        private bool _isDead;

        private void Awake()
        {
            actionSystem ??= GetComponent<ActionSystem>();
            characterMovement ??= GetComponent<CharacterMovement>();
            characterStateController ??= GetComponent<CharacterStateController>();
            health ??= GetComponent<Health>();
            animationDriver ??= GetComponent<AnimationDriver>();
            _inputSystemActions = new InputSystem_Actions();

            Subscribe();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void Update()
        {
            if (_isDead || characterStateController != null && characterStateController.IsHurtPendingOrActive) return;

            Vector2 direction = _inputSystemActions.Player.Move.ReadValue<Vector2>();
            characterMovement.OnMove(direction);
        }

        private void Subscribe()
        {
            _inputSystemActions.Enable();
            _inputSystemActions.Player.Attack.performed += OnAttack;
            _inputSystemActions.Player.Dash.performed += OnDash;
            _inputSystemActions.Player.Jump.performed += OnJump;
        }

        private void OnAttack(InputAction.CallbackContext obj)
        {
            if (_isDead || characterStateController != null && characterStateController.IsHurtPendingOrActive) return;
            actionSystem.OnInput(ActionType.Attack);
        }

        private void OnDash(InputAction.CallbackContext obj)
        {
            if (_isDead || characterStateController != null && characterStateController.IsHurtPendingOrActive) return;
            characterMovement.OnDash(characterMovement.FacingDirection);
            actionSystem.OnInput(ActionType.Dash);
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
            if (_isDead || characterStateController != null && characterStateController.IsHurtPendingOrActive) return;
            characterMovement.OnJump();
        }

        private void Unsubscribe()
        {
            _inputSystemActions.Disable();
            _inputSystemActions.Player.Attack.performed -= OnAttack;
            _inputSystemActions.Player.Dash.performed -= OnDash;
            _inputSystemActions.Player.Jump.performed -= OnJump;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandleDied()
        {
            if (_isDead) return;

            _isDead = true;

            characterMovement.SetMovementLock(true);

            if (actionSystem != null)
            {
                actionSystem.enabled = false;
            }

            animationDriver?.PlayPriorityAnimation(deathAnimation, -1f);
        }
    }
}
