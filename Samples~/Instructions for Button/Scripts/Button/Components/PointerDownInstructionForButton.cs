using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Per-event component: plays its instruction list when a press begins (pointer/Submit down).</summary>
    [RequireComponent(typeof(Button))]
    public class PointerDownInstructionForButton : PressInstructionForButtonBase
    {
        protected override void OnPressBegan() => TryApply();

        protected override void OnPressEnded() { }
    }
}