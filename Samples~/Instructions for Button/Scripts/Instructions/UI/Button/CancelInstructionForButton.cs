using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Inline, sync instruction: cancels the instruction list running on the target component.
    /// Shows how a later step can abort a still-running async step (e.g. cancel a shake on pointer up).
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/Button")]
    public class CancelInstructionForButton : Instruction
    {
        [SerializeField] private InstructionForButtonBase target;

        public override void Apply()
        {
            if (target)
                target.Cancel();
        }
    }
}