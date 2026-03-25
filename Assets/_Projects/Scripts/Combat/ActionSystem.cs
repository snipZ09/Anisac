using Game.Shared;
using UnityEngine;

namespace Game.Combat
{
    public class ActionSystem : MonoBehaviour
    {
        private ComboResolver _resolver;
        private ActionRuntime _runtimeAction;
        private InputBuffer _inputBuffer;
        [SerializeField] private ComboDatabase comboDatabase;

        private void Awake()
        {
            _resolver = new ComboResolver(comboDatabase);
            _inputBuffer = GetComponent<InputBuffer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_runtimeAction != null)
            {
                _runtimeAction.Tick(Time.deltaTime);
                if (_runtimeAction.CurrentPhase == ActionPhase.Done)
                {
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
            var actionData = _resolver.Resolve(buffer);
            if(actionData == null)
                return;
            if (_runtimeAction is { IsInCancelWindow: false }) return;
            Debug.Log($"Executing {actionData}");
            _runtimeAction = new ActionRuntime(actionData);
            _inputBuffer.ConsumeAll();
        }
    }
}