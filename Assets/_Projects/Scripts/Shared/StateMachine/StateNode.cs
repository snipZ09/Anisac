using Game.Shared;
using System.Collections.Generic;

namespace Game.Shared
{
    public class StateNode
    {
        public BaseState State { get; }
        public HashSet<Transition> Transitions { get; }

        public StateNode(BaseState state)
        {
            State = state;
            Transitions = new HashSet<Transition>();
        }

        public void AddTransition(BaseState nextState, FuncPredicate condition)
        {
            Transitions.Add(new Transition(nextState, condition));

        }
    }
}
