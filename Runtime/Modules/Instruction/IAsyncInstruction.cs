using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// An instruction whose execution is asynchronous. Processors await this variant before proceeding to the
    /// next entry in the sequence.
    /// </summary>
    public interface IAsyncInstruction : IInstruction
    {
        /// <summary>Executes the instruction asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the instruction finishes.</returns>
        Task Apply(CancellationToken cancellationToken = default);
    }
}