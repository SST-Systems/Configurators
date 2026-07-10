using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// An asynchronous modification handler that receives injected data and applies awaitable logic to a context.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    public interface IAsyncModificationHandler<in TContext> : IModificationHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The modification data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Applies the modification to the given context asynchronously.</summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the modification finishes.</returns>
        Task Apply(TContext context, CancellationToken cancellationToken = default);
    }
}