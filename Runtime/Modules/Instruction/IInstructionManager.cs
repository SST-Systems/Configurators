using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Resolves the handler-based instructions of an <see cref="InstructionProcessor"/>, binding pooled handlers
    /// to their serialized data. See <see cref="InstructionManager"/> for the default implementation.
    /// </summary>
    public interface IInstructionManager
    {
        /// <summary>
        /// Binds pooled handlers to the processor's instructions and returns a binding that unbinds them,
        /// returns them to the pool, and cancels in-flight work when disposed.
        /// </summary>
        /// <param name="processor">The processor whose instructions to resolve.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy; pass
        /// <c>null</c> to manage the returned <see cref="IDisposable"/> yourself.</param>
        /// <returns>The binding, or <c>null</c> if <paramref name="processor"/> is null.</returns>
        IDisposable ResolveInstructions(InstructionProcessor processor, Component lifetimeOwner);
    }
}