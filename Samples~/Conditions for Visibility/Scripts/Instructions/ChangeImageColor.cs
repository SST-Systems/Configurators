using System;
using SST.StableRef;
using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Inline, sync instruction that sets a UI <see cref="Image"/>'s color in one immediate <see cref="Apply"/> —
    /// e.g. tint the panel differently for each state.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI")]
    public class ChangeImageColor : Instruction
    {
        [SerializeField] private Image target;
        [SerializeField] private Color color = Color.white;

        public override void Apply()
        {
            if (target)
                target.color = color;
        }
    }
}