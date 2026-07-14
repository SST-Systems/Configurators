using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>One state of a <see cref="StatefulWidget{TState}"/>: the instruction chain run on entering it.</summary>
    [Serializable]
    public class StatefulState<TState>
    {
        public TState State;
        public InstructionProcessor OnEnter;
    }

    /// <summary>
    /// A plain state machine: switch states with <see cref="SetState"/>, and each state plays its own instruction
    /// chain on entry. No conditions here — drive it from your code, a UnityEvent, or by attaching a
    /// <see cref="StatefulWidgetController{TState}"/> that switches states from conditions.
    /// </summary>
    public abstract class StatefulWidget<TState> : MonoBehaviour where TState : Enum
    {
        [SerializeField] private List<StatefulState<TState>> states = new List<StatefulState<TState>>();
        [SerializeField] private TState initialState;

        private int _current = -1;
        private bool _resolved;

        public TState CurrentState => _current >= 0 ? states[_current].State : default;

        private void Start()
        {
            EnsureResolved();

            // Only apply the initial state if nothing set one yet — lets a controller win regardless of Start order.
            if (_current < 0)
                SetState(initialState);
        }

        /// <summary>Switches to a state and plays its instruction chain. Safe to call before Start.</summary>
        public void SetState(TState state)
        {
            EnsureResolved();

            for (int i = 0; i < states.Count; i++)
            {
                if (EqualityComparer<TState>.Default.Equals(states[i].State, state))
                {
                    Enter(i);
                    return;
                }
            }
        }

        private void EnsureResolved()
        {
            if (_resolved)
                return;

            InstructionManager instructions = ServiceLocator.Get<InstructionManager>();
            foreach (StatefulState<TState> state in states)
                instructions.ResolveInstructions(state.OnEnter, this);

            _resolved = true;
        }

        private void Enter(int index)
        {
            if (index == _current)
                return;

            _current = index;
            states[index].OnEnter.Apply();
        }
    }
}