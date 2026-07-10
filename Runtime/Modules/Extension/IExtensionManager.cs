using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Resolves the handler-based extensions of an <see cref="ExtensionProcessor"/>, binding pooled handlers to
    /// their serialized data. See <see cref="ExtensionManager"/> for the default implementation.
    /// </summary>
    public interface IExtensionManager
    {
        /// <summary>
        /// Binds pooled handlers to the processor's extensions and returns a binding that unbinds them, returns
        /// them to the pool, and cancels in-flight work when disposed. Async extensions must be resolved before
        /// their value is requested.
        /// </summary>
        /// <param name="processor">The processor whose extensions to resolve.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy; pass
        /// <c>null</c> to manage the returned <see cref="IDisposable"/> yourself.</param>
        /// <returns>The binding, or <c>null</c> if <paramref name="processor"/> is null.</returns>
        IDisposable ResolveExtensions(ExtensionProcessor processor, Component lifetimeOwner);
    }
}