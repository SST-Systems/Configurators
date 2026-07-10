using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// An asynchronous instruction handler that receives injected data and executes awaitable logic on demand.
    /// </summary>
    public interface IAsyncInstructionHandler : IInstructionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The instruction data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Executes the instruction asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the instruction finishes.</returns>
        Task Apply(CancellationToken cancellationToken = default);
    }
}