using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SST.StableRef;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Inline, sync instruction: clears UI selection, but only if the given Selectable is the one
    /// currently selected. Demonstrates a small guarded EventSystem action as an instruction step.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/Navigation")]
    public class DeselectSelectable : Instruction
    {
        [SerializeField] private Selectable selectable;

        public override void Apply()
        {
            // Only deselect if this exact Selectable is currently selected, so we don't clobber another focus.
            if (selectable && EventSystem.current &&
                EventSystem.current.currentSelectedGameObject == selectable.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}