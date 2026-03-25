using System;
using Game.Shared;

namespace Game.Combat
{
    public class ActionRuntime
    {
        private ActionData _actionData;
        private float _timer;
        public event Action<ActionPhase> OnPhaseChanged;
        public event Action OnActionComplete;
        public ActionPhase CurrentPhase { get; private set; }

        public bool IsInCancelWindow =>
            _actionData.canCancel &&
            (_actionData.cancelWindowEnd + _actionData.timeActive + _actionData.timeStartUp) >= _timer &&
            _timer >= (_actionData.cancelWindowStart + _actionData.timeActive + _actionData.timeStartUp);
        
        public ActionRuntime(ActionData actionData)
        {
            _actionData = actionData;
            _timer = 0f;
            CurrentPhase = ActionPhase.Startup;
        }

        // Update is called once per frame
        public void Tick(float dt)
        {
            if (CurrentPhase == ActionPhase.Done) return;

            _timer += dt;
            if (CurrentPhase == ActionPhase.Startup && _timer >= _actionData.timeStartUp)
            {
                CurrentPhase = ActionPhase.Active;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else if (CurrentPhase == ActionPhase.Active && _timer >= _actionData.timeStartUp + _actionData.timeActive)
            {
                CurrentPhase = ActionPhase.Recovery;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else if (CurrentPhase == ActionPhase.Recovery && _timer >= _actionData.TotalDuration)
            {
                CurrentPhase = ActionPhase.Done;
                OnActionComplete?.Invoke();
            }
        }



        public void ForceEnd()
        {
            CurrentPhase = ActionPhase.Done;
            OnActionComplete?.Invoke();
        }
    }
}