using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when the pointer leaves the button (hover out).</summary>
    [RequireComponent(typeof(Button))]
    public class PointerExitInstructionForButton : InstructionForButtonBase, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            TryApply();
        }
    }
}