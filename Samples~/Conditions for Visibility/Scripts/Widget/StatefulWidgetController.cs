using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>A rule: switch the widget to <see cref="State"/> while this condition set is met.</summary>
    [Serializable]
    public class StateRule<TState>
    {
        public TState State;
        public ConditionProcessor When;
    }

    /// <summary>
    /// Optional add-on that drives a <see cref="StatefulWidget{TState}"/>'s state from conditions.
    /// The first rule whose conditions are met wins; re-evaluated reactively whenever any condition changes
    /// (e.g. a toggle flips). The widget itself stays condition-agnostic — this component is the only place
    /// conditions live.
    /// </summary>
    public abstract class StatefulWidgetController<TState> : MonoBehaviour where TState : Enum
    {
        [SerializeField] private StatefulWidget<TState> target;
        [SerializeField] private List<StateRule<TState>> rules = new();

        private bool _ready;

        private void Start()
        {
            ConditionManager conditions = ServiceLocator.Get<ConditionManager>();

            foreach (StateRule<TState> rule in rules)
            {
                conditions.ResolveConditions(rule.When, this);
                rule.When.Subscribe(OnConditionsChanged);
            }

            _ready = true;
            Evaluate();
        }

        private void OnConditionsChanged(bool _)
        {
            if (_ready)
                Evaluate();
        }

        private void Evaluate()
        {
            if (!target)
                return;

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].When.IsMet())
                {
                    target.SetState(rules[i].State);
                    return;
                }
            }
        }
    }
}