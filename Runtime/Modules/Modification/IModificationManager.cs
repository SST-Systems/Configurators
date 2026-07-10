using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Resolves the handler-based modifications of a <see cref="ModificationProcessor{TContext}"/>, binding pooled
    /// handlers to their serialized data. See <see cref="ModificationManager"/> for the default implementation.
    /// </summary>
    public interface IModificationManager
    {
        /// <summary>
        /// Binds pooled handlers to the processor's modifications and returns a binding that unbinds them,
        /// returns them to the pool, and cancels in-flight work when disposed.
        /// </summary>
        /// <typeparam name="TContext">The context type the modifications are applied to.</typeparam>
        /// <param name="processor">The processor whose modifications to resolve.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy; pass
        /// <c>null</c> to manage the returned <see cref="IDisposable"/> yourself.</param>
        /// <returns>The binding, or <c>null</c> if <paramref name="processor"/> is null.</returns>
        IDisposable ResolveModifications<TContext>(ModificationProcessor<TContext> processor, Component lifetimeOwner);
    }
}