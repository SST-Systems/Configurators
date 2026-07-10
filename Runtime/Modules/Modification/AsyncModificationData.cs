using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized modification that delegates to a pooled, asynchronous handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected async modifications; for inline logic
    /// subclass <see cref="AsyncModification{TContext}"/> directly.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class AsyncModificationData<TContext, THandler> : AsyncModification<TContext>, IHandlerBinder
        where THandler : class, IAsyncModificationHandler<TContext>
    {
        private THandler _handler;

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
        }

        /// <summary>Forwards the modification to the bound handler, or completes immediately if none is bound.</summary>
        /// <param name="context">The context to modify.</param>
        /// <param name="cancellationToken">Token that cancels the operation.</param>
        /// <returns>The handler's task, or <see cref="Task.CompletedTask"/> when unbound.</returns>
        public override Task Apply(TContext context, CancellationToken cancellationToken = default)
        {
            return _handler != null ? _handler.Apply(context, cancellationToken) : Task.CompletedTask;
        }
    }
}