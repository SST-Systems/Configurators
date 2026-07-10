using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Stateful, single-run processor for a sequence of instructions. Runs entries in order, awaiting async ones,
    /// and exposes the current run via <see cref="ExecutionTask"/>. A repeat <see cref="Apply"/> cancels the
    /// previous run. Handler-based entries must first be resolved through <see cref="IInstructionManager"/>.
    /// </summary>
    [Serializable]
    public class InstructionProcessor : IConfiguratorProcessor
    {
        /// <summary>The configured instruction sequence, executed in order.</summary>
        public StableRefList<IInstruction> Instructions;

        /// <summary>The task of the current or most recent run; <see cref="Task.CompletedTask"/> before the first run.</summary>
        public Task ExecutionTask { get; private set; } = Task.CompletedTask;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _resolveCancellationToken;

        /// <summary>Receives the resolve-scoped cancellation token linked into each run. Called by the manager.</summary>
        /// <param name="token">The resolve-scoped token that is cancelled when the binding is disposed.</param>
        internal void SetResolveCancellation(CancellationToken token)
        {
            _resolveCancellationToken = token;
        }

        /// <summary>Cancels the current run, if any.</summary>
        public void Cancel() => _cancellationTokenSource?.Cancel();

        /// <summary>
        /// Runs the instruction sequence, cancelling any previous run first. The run is linked to both the
        /// supplied token and the resolve-scoped token.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels this run.</param>
        /// <returns>The task for this run, also exposed via <see cref="ExecutionTask"/>.</returns>
        public Task Apply(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = (cancellationToken.CanBeCanceled || _resolveCancellationToken.CanBeCanceled)
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _resolveCancellationToken)
                : new CancellationTokenSource();

            return ExecutionTask = RunAsync(_cancellationTokenSource.Token);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Instructions == null || Instructions.Count == 0)
                return;

            foreach (var stableRef in Instructions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var instruction = stableRef?.Value;

                if (instruction == null)
                    continue;

                try
                {
                    if (instruction is IAsyncInstruction asyncInstruction)
                        await asyncInstruction.Apply(cancellationToken);
                    else if (instruction is Instruction syncInstruction)
                        syncInstruction.Apply();
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }
}