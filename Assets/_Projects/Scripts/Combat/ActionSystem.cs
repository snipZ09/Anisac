using Game.Shared;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class ActionSystem : MonoBehaviour, IActionRunner
    {
        private ComboResolver _resolver;
        private ActionRuntime _runtimeAction;
        private InputBuffer _inputBuffer;
        private AnimationDriver _animationDriver;
        [SerializeField] private ComboDatabase comboDatabase;

        public ActionRuntime CurrentRuntime => _runtimeAction;
        public event Action<ActionRuntime> OnActionStarted;

        private void Awake()
        {
            _resolver = new ComboResolver(comboDatabase);
            _inputBuffer = GetComponent<InputBuffer>();
            _animationDriver = GetComponent<AnimationDriver>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_runtimeAction != null)
            {
                _runtimeAction.Tick(Time.deltaTime);
                if (_runtimeAction.CurrentPhase == ActionPhase.Done)
                {
                    _runtimeAction.OnCancelWindowOpened -= TryResolveAndExecute;
                    _runtimeAction = null;
                }
            }
        }

        public void OnInput(ActionType actionType)
        {
            _inputBuffer.Push(new BufferedInput(actionType, Time.time));
            TryResolveAndExecute();
        }

        private void TryResolveAndExecute()
        {
            var buffer = _inputBuffer.GetValid();
            var actionData = _resolver.Resolve(buffer, _runtimeAction?.Data);

            if (actionData == null)
                return;

            if (_runtimeAction is { IsInCancelWindow: false }) return;

            ExecuteAction(actionData);
        }

        private void ExecuteAction(ActionData actionData)
        {
            if (_runtimeAction != null)
            {
                _runtimeAction.OnCancelWindowOpened -= TryResolveAndExecute;
                _runtimeAction.ForceEnd();
            }
            _runtimeAction = new ActionRuntime(actionData);
            _runtimeAction.OnCancelWindowOpened += TryResolveAndExecute;
            _inputBuffer.ConsumeAll();
            _animationDriver.PlayActionAnimation(actionData);
            OnActionStarted?.Invoke(_runtimeAction);
        }
    }
}