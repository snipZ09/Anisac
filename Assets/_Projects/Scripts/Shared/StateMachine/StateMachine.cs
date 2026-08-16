using System;
using System.Collections.Generic;

namespace Game.Shared
{
    public class StateMachine
    {
        private StateNode _current;
        private readonly Dictionary<Type, StateNode> _nodes = new();
        private readonly HashSet<Transition> _anyTransitions = new();

        public StateMachine(BaseState initialState)
        {
            _current = GetOrAddNode(initialState);
            _current.State?.EnterState();
        }

        public BaseState GetCurrentState() => _current?.State;

        public void Execute()
        {
            var transition = GetTransition();
            if (transition != null) ChangeState(transition.To);
            _current.State?.ExecuteState();
        }

        public void ChangeState(BaseState newState)
        {
            //UnityEngine.Debug.Log(
            //    $"[{UnityEngine.Time.time:F3}] State: {_current.State.GetType().Name} -> {newState.GetType().Name}");
            _current.State?.ExitState();
            _current = _nodes[newState.GetType()];
            _current.State?.EnterState();
        }

        public void AddTransition(BaseState from, BaseState to, FuncPredicate condition)
        {
            // add nodes
            var fromNode = GetOrAddNode(from);
            var toNode = GetOrAddNode(to);

            fromNode.AddTransition(toNode.State, condition);
        }

        public void AddAnyTransition(BaseState to, FuncPredicate condition)
        {
            var toNode = GetOrAddNode(to);

            _anyTransitions.Add(new Transition(toNode.State, condition));
        }

        private StateNode GetOrAddNode(BaseState state)
        {
            var node = _nodes.GetValueOrDefault(state.GetType());

            if (node != null) return node;

            node = new StateNode(state);
            _nodes.Add(state.GetType(), node);

            return node;
        }

        private Transition GetTransition()
        {
            foreach (var transition in _anyTransitions)
            {
                if (transition.Condition.Evaluate()) return transition;
            }

            foreach (var transition in _current.Transitions)
            {
                if (transition.Condition.Evaluate()) return transition;
            }

            return null;
        }
    }
}