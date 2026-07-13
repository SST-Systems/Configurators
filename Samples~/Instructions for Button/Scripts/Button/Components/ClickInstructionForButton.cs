using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list on the Button's onClick event.</summary>
    [RequireComponent(typeof(Button))]
    public class ClickInstructionForButton : InstructionForButtonBase
    {
        protected void OnEnable()
        {
            button.onClick.AddListener(TryApply);
        }

        protected void OnDisable()
        {
            button.onClick.RemoveListener(TryApply);
        }
    }
}