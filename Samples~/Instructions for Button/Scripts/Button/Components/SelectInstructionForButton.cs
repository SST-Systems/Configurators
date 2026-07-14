using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when the button becomes selected (OnSelect).</summary>
    [RequireComponent(typeof(Button))]
    public class SelectInstructionForButton : InstructionForButtonBase, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            TryApply();
        }
    }
}