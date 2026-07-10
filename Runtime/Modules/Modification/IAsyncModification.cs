using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// A modification whose application is asynchronous. Processors await this variant before proceeding to the
    /// next entry.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    public interface IAsyncModification<in TContext> : IModification<TContext>
    {
        /// <summary>Applies the modification to the given context asynchronously.</summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the modification finishes.</returns>
        Task Apply(TContext context, CancellationToken cancellationToken = default);
    }
}