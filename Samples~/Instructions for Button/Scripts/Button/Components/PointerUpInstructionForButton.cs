using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when a press ends (pointer/Submit up).</summary>
    [RequireComponent(typeof(Button))]
    public class PointerUpInstructionForButton : PressInstructionForButtonBase
    {
        protected override void OnPressBegan() { }

        protected override void OnPressEnded() => TryApply();
    }
}