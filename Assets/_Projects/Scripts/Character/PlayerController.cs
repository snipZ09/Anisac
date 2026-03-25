using Game.Combat;
using Game.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private ActionSystem actionSystem;
        private InputSystem_Actions _inputSystemActions;

        private void Awake()
        {
            actionSystem ??= GetComponent<ActionSystem>();
            _inputSystemActions = new InputSystem_Actions();
            
            Subscribe();
        }

        private void Subscribe()
        {
            _inputSystemActions.Enable();
            _inputSystemActions.Player.Attack.performed += OnAttack;
            _inputSystemActions.Player.Dash.performed += OnDash;
        }

        private void OnAttack(InputAction.CallbackContext obj)
        {
            actionSystem.OnInput(ActionType.Attack);
        }

        private void OnDash(InputAction.CallbackContext obj)
        {
            actionSystem.OnInput(ActionType.Dash);
        }
        
        private void Unsubscribe()
        {
            _inputSystemActions.Disable();
            _inputSystemActions.Player.Attack.performed -= OnAttack;
            _inputSystemActions.Player.Dash.performed -= OnDash;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}