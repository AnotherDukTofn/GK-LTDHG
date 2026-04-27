namespace SpellStrike.Core.StateMachine
{
    /// <summary>
    /// Finite State Machine — dùng chung cho Player, Enemy, Elite Boss.
    /// Mỗi entity tạo 1 instance StateMachine riêng.
    /// </summary>
    public class StateMachine
    {
        private IState _currentState;
        private System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<Transition>> _transitions = new();
        private System.Collections.Generic.List<Transition> _currentTransitions = new();
        private System.Collections.Generic.List<Transition> _fromAnyTransitions = new();
        private static System.Collections.Generic.List<Transition> _emptyTransitions = new(0);

        public IState CurrentState => _currentState;

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null) SetState(transition.NextState);

            _currentState?.Action();
        }

        public void FixedTick()
        {
            _currentState?.FixedAction();
        }

        public void SetState(IState state)
        {
            if (_currentState == state) return;

            _currentState?.Exit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null) _currentTransitions = _emptyTransitions;

            _currentState?.Enter();
        }

        public void AddTransition(IState from, IState to, System.Func<bool> predicate)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new System.Collections.Generic.List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState to, System.Func<bool> predicate)
        {
            _fromAnyTransitions.Add(new Transition(to, predicate));
        }

        private Transition GetTransition()
        {
            foreach (var transition in _fromAnyTransitions)
            {
                if (transition.ConditionSatisfied()) return transition;
            }

            foreach (var transition in _currentTransitions)
            {
                if (transition.ConditionSatisfied()) return transition;
            }

            return null;
        }

        private class Transition
        {
            public IState NextState { get; }
            public System.Func<bool> ConditionSatisfied { get; }

            public Transition(IState nextState, System.Func<bool> condition)
            {
                NextState = nextState;
                ConditionSatisfied = condition;
            }
        }
    }
}
