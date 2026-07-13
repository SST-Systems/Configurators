using System;
using SST.StableRef;
using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Inline, sync instruction that writes a string into a legacy UGUI <see cref="Text"/> in one immediate
    /// <see cref="Apply"/> — e.g. show a per-state status label.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI")]
    public class SetTextLegacy : Instruction
    {
        [SerializeField] private Text target;
        [SerializeField] private string text;

        public override void Apply()
        {
            if (target)
                target.text = text;
        }
    }
}