using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>Inline, sync instruction: logs a serialized message — the simplest Instruction (overrides Apply(), no handler).</summary>
    [Serializable]
    [StableRefCategory("Debug")]
    public class LogMessage : Instruction
    {
        [SerializeField] private string message;

        public override void Apply()
        {
            Debug.Log(message);
        }
    }
}