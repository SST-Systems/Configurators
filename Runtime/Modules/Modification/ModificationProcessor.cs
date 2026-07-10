using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Stateless processor for a sequence of modifications applied to a <typeparamref name="TContext"/>. Applies
    /// entries in order, awaiting async ones, and keeps no per-run state, so a single processor can be applied to
    /// many contexts concurrently. Handler-based entries must first be resolved through
    /// <see cref="IModificationManager"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type the modifications are applied to.</typeparam>
    [Serializable]
    public class ModificationProcessor<TContext> : IConfiguratorProcessor
    {
        /// <summary>The configured modification sequence, applied in order.</summary>
        public StableRefList<IModification<TContext>> Modifications;

        private CancellationToken _resolveCancellationToken;

        /// <summary>Receives the resolve-scoped cancellation token linked into each run. Called by the manager.</summary>
        /// <param name="token">The resolve-scoped token that is cancelled when the binding is disposed.</param>
        internal void SetResolveCancellation(CancellationToken token)
        {
            _resolveCancellationToken = token;
        }

        /// <summary>
        /// Applies all modifications to the context in order, awaiting async ones. The run is linked to both the
        /// supplied token and the resolve-scoped token.
        /// </summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels this run.</param>
        /// <returns>A task that completes when all modifications finish.</returns>
        public Task Apply(TContext context, CancellationToken cancellationToken = default)
        {
            if (Modifications == null || Modifications.Count == 0)
                return Task.CompletedTask;

            if (!_resolveCancellationToken.CanBeCanceled)
                return RunAsync(context, cancellationToken);

            if (!cancellationToken.CanBeCanceled)
                return RunAsync(context, _resolveCancellationToken);

            return RunLinkedAsync(context, cancellationToken);
        }

        private async Task RunLinkedAsync(TContext context, CancellationToken cancellationToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _resolveCancellationToken);
            await RunAsync(context, linked.Token);
        }

        private async Task RunAsync(TContext context, CancellationToken cancellationToken)
        {
            foreach (var stableRef in Modifications)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var modification = stableRef?.Value;

                if (modification == null)
                    continue;

                try
                {
                    if (modification is IAsyncModification<TContext> asyncModification)
                        await asyncModification.Apply(context, cancellationToken);
                    else if (modification is Modification<TContext> syncModification)
                        syncModification.Apply(context);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }
}