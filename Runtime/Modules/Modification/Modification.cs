using System;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for a synchronous, inline modification. Subclass this and implement <see cref="Apply"/> to run
    /// logic directly; for handler/DI-based modifications use <see cref="ModificationData{TContext, THandler}"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    [Serializable]
    public abstract class Modification<TContext> : IModification<TContext>
    {
        /// <summary>Applies the modification to the given context.</summary>
        /// <param name="context">The context to modify.</param>
        public abstract void Apply(TContext context);
    }
}