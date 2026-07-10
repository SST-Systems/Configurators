using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for an asynchronous modification handler, exposing the injected data to subclasses.
    /// Because handlers are pooled and reused, keep no per-call state outside <see cref="Data"/>.
    /// </summary>
    /// <typeparam name="TData">The serialized data type this handler operates on.</typeparam>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    public abstract class AsyncModificationHandler<TData, TContext> : IAsyncModificationHandler<TContext> where TData : class
    {
        /// <summary>The data injected via <see cref="SetData"/>, available to subclasses.</summary>
        protected TData Data { get; private set; }

        /// <summary>Injects the serialized data, cast to <typeparamref name="TData"/>.</summary>
        /// <param name="data">The modification data, or <c>null</c> when unbound.</param>
        public void SetData(object data) => Data = data as TData;

        /// <summary>Applies the modification to the given context asynchronously.</summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the modification finishes.</returns>
        public abstract Task Apply(TContext context, CancellationToken cancellationToken = default);
    }
}