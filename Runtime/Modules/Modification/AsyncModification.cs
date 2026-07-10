using System;
using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for an asynchronous, inline modification. Subclass this and implement <see cref="Apply"/> to run
    /// awaitable logic directly; for handler/DI-based modifications use
    /// <see cref="AsyncModificationData{TContext, THandler}"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    [Serializable]
    public abstract class AsyncModification<TContext> : IAsyncModification<TContext>
    {
        /// <summary>Applies the modification to the given context asynchronously.</summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the modification finishes.</returns>
        public abstract Task Apply(TContext context, CancellationToken cancellationToken = default);
    }
}