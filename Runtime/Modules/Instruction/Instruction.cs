using System;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for a synchronous, inline instruction. Subclass this and implement <see cref="Apply"/> to run
    /// logic directly; for handler/DI-based instructions use <see cref="InstructionData{THandler}"/>.
    /// </summary>
    [Serializable]
    public abstract class Instruction : IInstruction
    {
        /// <summary>Executes the instruction.</summary>
        public abstract void Apply();
    }
}