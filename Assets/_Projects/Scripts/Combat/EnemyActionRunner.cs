using System;
using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(AnimationDriver))]
    public class EnemyActionRunner : MonoBehaviour, IActionRunner
    {
        private AnimationDriver _animationDriver;
        private ActionRuntime _runtimeAction;

        public ActionRuntime CurrentRuntime => _runtimeAction;
        public bool IsBusy => _runtimeAction != null && _runtimeAction.CurrentPhase != ActionPhase.Done;

        public event Action<ActionRuntime> OnActionStarted;

        private void Awake()
        {
            _animationDriver = GetComponent<AnimationDriver>();
        }

        private void Update()
        {
            if (_runtimeAction == null) return;

            _runtimeAction.Tick(Time.deltaTime);
            if (_runtimeAction.CurrentPhase == ActionPhase.Done)
            {
                _runtimeAction = null;
            }
        }

        public void PlayAction(ActionData data)
        {
            if (data == null) return;

            _runtimeAction?.ForceEnd();
            _runtimeAction = new ActionRuntime(data);
            _animationDriver.PlayActionAnimation(data);
            OnActionStarted?.Invoke(_runtimeAction);
        }

        public void CancelCurrentAction()
        {
            _runtimeAction?.ForceEnd();
            _runtimeAction = null;
        }
    }
}
