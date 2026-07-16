using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Resolves the handler-based conditions of a <see cref="ConditionProcessor"/> or a context-aware
    /// <see cref="ConditionProcessor{TContext}"/>, recursively binding pooled handlers to their serialized data
    /// including nested composites. See <see cref="ConditionManager"/> for the default implementation.
    /// </summary>
    public interface IConditionManager
    {
        /// <summary>
        /// Recursively binds pooled handlers to the processor's conditions and returns a binding that unbinds them,
        /// returns them to the pool, and unsubscribes reactive callbacks when disposed.
        /// </summary>
        /// <param name="processor">The processor whose conditions to resolve.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy; pass
        /// <c>null</c> to manage the returned <see cref="IDisposable"/> yourself.</param>
        /// <returns>The binding, or <c>null</c> if <paramref name="processor"/> is null.</returns>
        IDisposable ResolveConditions(ConditionProcessor processor, Component lifetimeOwner);

        /// <summary>
        /// Recursively binds pooled handlers to the context-aware processor's conditions and returns a binding that
        /// unbinds them, returns them to the pool, and unsubscribes reactive callbacks when disposed.
        /// </summary>
        /// <typeparam name="TContext">The context the conditions are evaluated against.</typeparam>
        /// <param name="processor">The processor whose conditions to resolve.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy; pass
        /// <c>null</c> to manage the returned <see cref="IDisposable"/> yourself.</param>
        /// <returns>The binding, or <c>null</c> if <paramref name="processor"/> is null.</returns>
        IDisposable ResolveConditions<TContext>(ConditionProcessor<TContext> processor, Component lifetimeOwner);
    }
}