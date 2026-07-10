using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// An extension whose value is produced asynchronously. Such extensions must be resolved through
    /// <see cref="IExtensionManager"/> before <see cref="GetValueAsync"/> is called.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    public interface IAsyncExtension<TValue> : IExtension
    {
        /// <summary>Produces the extension's value asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task yielding the value.</returns>
        Task<TValue> GetValueAsync(CancellationToken cancellationToken = default);
    }
}