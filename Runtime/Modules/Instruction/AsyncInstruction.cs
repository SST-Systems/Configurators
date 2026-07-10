using System;
using System.Threading;
using System.Threading.Tasks;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for an asynchronous, inline instruction. Subclass this and implement <see cref="Apply"/> to run
    /// awaitable logic directly; for handler/DI-based instructions use <see cref="AsyncInstructionData{THandler}"/>.
    /// </summary>
    [Serializable]
    public abstract class AsyncInstruction : IAsyncInstruction
    {
        /// <summary>Executes the instruction asynchronously.</summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task that completes when the instruction finishes.</returns>
        public abstract Task Apply(CancellationToken cancellationToken = default);
    }
}