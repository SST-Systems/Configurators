using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when the button loses selection (OnDeselect).</summary>
    [RequireComponent(typeof(Button))]
    public class DeselectInstructionForButton : InstructionForButtonBase, IDeselectHandler
    {
        public void OnDeselect(BaseEventData eventData)
        {
            TryApply();
        }
    }
}