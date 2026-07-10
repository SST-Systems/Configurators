using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// An asynchronous extension handler that receives injected data and produces a value on demand.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    public interface IAsyncExtensionHandler<TValue> : IExtensionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The extension data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Produces the extension's value asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task yielding the value.</returns>
        Task<TValue> GetValueAsync(CancellationToken cancellationToken = default);
    }
}