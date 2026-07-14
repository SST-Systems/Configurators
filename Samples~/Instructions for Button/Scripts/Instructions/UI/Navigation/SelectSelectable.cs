using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SST.StableRef;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Inline, sync instruction: makes the given Selectable the currently selected UI element.
    /// Handy to move keyboard/gamepad focus as part of a button's instruction list.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/Navigation")]
    public class SelectSelectable : Instruction
    {
        [SerializeField] private Selectable selectable;
        
        public override void Apply()
        {
            if (selectable && EventSystem.current)
            {
                selectable.Select();
            }
        }
    }
}