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
        private InputSystem_Actions _inputSystemActions;

        private void Awake()
        {
            actionSystem ??= GetComponent<ActionSystem>();
            characterMovement ??= GetComponent<CharacterMovement>();
            _inputSystemActions = new InputSystem_Actions();

            Subscribe();
        }

        private void Update()
        {
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
            actionSystem.OnInput(ActionType.Attack);
        }

        private void OnDash(InputAction.CallbackContext obj)
        {
            characterMovement.OnDash(obj.ReadValue<Vector2>());
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
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
    }
}