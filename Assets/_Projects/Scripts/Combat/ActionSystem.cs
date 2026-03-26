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
        
        public ActionRuntime CurrentRuntime => _runtimeAction;

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
    
            Debug.Log($"Resolved: {actionData?.name}, Previous: {_runtimeAction?.Data?.name}");
    
            if(actionData == null)
                return;
    
            Debug.Log($"Phase: {_runtimeAction?.CurrentPhase}, InCancelWindow: {_runtimeAction?.IsInCancelWindow}, Elapsed: {_runtimeAction?.ElapsedTime}");
    
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
        }
    }
}