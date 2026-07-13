using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when the pointer enters the button (hover in).</summary>
    [RequireComponent(typeof(Button))]
    public class PointerEnterInstructionForButton : InstructionForButtonBase, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.IsInteractable())
                return;

            TryApply();
        }
    }
}