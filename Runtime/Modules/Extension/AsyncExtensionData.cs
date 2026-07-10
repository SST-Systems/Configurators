using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized extension that delegates to a pooled, asynchronous handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected async extensions; for inline logic subclass
    /// <see cref="AsyncExtension{TValue}"/> directly. Must be resolved via <see cref="IExtensionManager"/> before
    /// <see cref="GetValueAsync"/> is called.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class AsyncExtensionData<TValue, THandler> : IAsyncExtension<TValue>, IHandlerBinder, IResolveCancellationBinder
        where THandler : class, IAsyncExtensionHandler<TValue>
    {
        private THandler _handler;
        private CancellationToken _resolveCancellationToken;

        Type IHandlerBinder.HandlerType => typeof(THandler);
        object IHandlerBinder.GetHandler() => _handler;

        void IHandlerBinder.BindHandler(object handler)
        {
            if (_handler != null)
            {
                Debug.LogError($"[Configurator] {GetType().Name}: handler is already bound. " +
                               "Resolve was probably called twice without disposing the previous binding.");
                return;
            }

            _handler = (THandler)handler;
            _handler.SetData(this);
        }

        void IHandlerBinder.UnbindHandler()
        {
            if (_handler == null)
                return;

            _handler.SetData(null);
            _handler = null;
            _resolveCancellationToken = default;
        }

        void IResolveCancellationBinder.SetResolveCancellation(CancellationToken token) => _resolveCancellationToken = token;

        /// <summary>
        /// Returns the bound handler's value, linking the supplied token with the resolve-scoped token. Logs an
        /// error and returns <c>default</c> if the extension has not been resolved via
        /// <see cref="IExtensionManager.ResolveExtensions"/>.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>A task yielding the handler's value, or a completed task with <c>default</c> if unbound.</returns>
        public Task<TValue> GetValueAsync(CancellationToken cancellationToken = default)
        {
            if (_handler == null)
            {
                Debug.LogError($"[Configurator] {GetType().Name}: no handler bound. " +
                               "Call IExtensionManager.ResolveExtensions before requesting the value.");
                return Task.FromResult<TValue>(default);
            }

            if (!_resolveCancellationToken.CanBeCanceled)
                return _handler.GetValueAsync(cancellationToken);

            if (!cancellationToken.CanBeCanceled)
                return _handler.GetValueAsync(_resolveCancellationToken);

            return GetValueLinkedAsync(cancellationToken);
        }

        private async Task<TValue> GetValueLinkedAsync(CancellationToken cancellationToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _resolveCancellationToken);
            return await _handler.GetValueAsync(linked.Token);
        }
    }
}