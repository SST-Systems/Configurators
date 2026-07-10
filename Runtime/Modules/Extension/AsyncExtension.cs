using System;
using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for an asynchronous, inline extension that produces a value of type <typeparamref name="TValue"/>.
    /// Subclass this and implement <see cref="GetValueAsync"/>; for handler/DI-based extensions use
    /// <see cref="AsyncExtensionData{TValue, THandler}"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    [Serializable]
    public abstract class AsyncExtension<TValue> : IAsyncExtension<TValue>
    {
        /// <summary>Produces the extension's value asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task yielding the value.</returns>
        public abstract Task<TValue> GetValueAsync(CancellationToken cancellationToken = default);
    }
}