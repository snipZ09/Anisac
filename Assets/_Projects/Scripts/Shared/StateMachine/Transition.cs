namespace Game.Shared
{
    public sealed class Transition
    {
        public BaseState To { get; }
        public FuncPredicate Condition { get; }

        public Transition(BaseState to, FuncPredicate condition)
        {
            To = to;
            Condition = condition;
        }
    }
}
