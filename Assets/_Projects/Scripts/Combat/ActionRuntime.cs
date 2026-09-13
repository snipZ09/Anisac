using System;
using Game.Shared;

namespace Game.Combat
{
    public class ActionRuntime
    {
        public readonly ActionData Data;
        private float _timer;
        private bool _cancelWindowFired;
        
        public event Action<ActionPhase> OnPhaseChanged;
        public event Action OnActionComplete;
        public event Action OnCancelWindowOpened;
        
        public ActionPhase CurrentPhase { get; private set; }
        
        public float ElapsedTime  => _timer;

        public bool IsInCancelWindow =>
            Data.canCancel &&
            (Data.cancelWindowEnd + Data.timeActive + Data.timeStartUp) >= _timer &&
            _timer >= (Data.cancelWindowStart + Data.timeActive + Data.timeStartUp);
        
        public ActionRuntime(ActionData data)
        {
            this.Data = data;
            _timer = 0f;
            CurrentPhase = ActionPhase.Startup;
        }

        // Update is called once per frame
        public void Tick(float dt)
        {
            if (CurrentPhase == ActionPhase.Done) return;

            _timer += dt;
            if (CurrentPhase == ActionPhase.Startup && _timer >= Data.timeStartUp)
            {
                CurrentPhase = ActionPhase.Active;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else if (CurrentPhase == ActionPhase.Active && _timer >= Data.timeStartUp + Data.timeActive)
            {
                CurrentPhase = ActionPhase.Recovery;
                OnPhaseChanged?.Invoke(CurrentPhase);
            }
            else if (CurrentPhase == ActionPhase.Recovery && _timer >= Data.timeStartUp + Data.timeActive + Data.cancelWindowStart && !_cancelWindowFired)
            {
                _cancelWindowFired  = true;
                OnCancelWindowOpened?.Invoke();
            }
            else if (CurrentPhase == ActionPhase.Recovery && _timer >= Data.TotalDuration)
            {
                CurrentPhase = ActionPhase.Done;
                OnActionComplete?.Invoke();
            }
        }
        
        public void ForceEnd()
        {
            CurrentPhase = ActionPhase.Done;
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnActionComplete?.Invoke();
        }
    }
}