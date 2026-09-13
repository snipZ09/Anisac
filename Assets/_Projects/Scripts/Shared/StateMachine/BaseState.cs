namespace Game.Shared
{
    public abstract class BaseState
    {

        public virtual void EnterState()
        {
            // noop
        }

        public virtual void ExecuteState()
        {
            // noop
        }

        public virtual void ExitState()
        {
            // noop
        }
    }
}