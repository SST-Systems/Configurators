using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized modification that delegates to a pooled, synchronous handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected modifications; for inline logic subclass
    /// <see cref="Modification{TContext}"/> directly.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class ModificationData<TContext, THandler> : Modification<TContext>, IHandlerBinder
        where THandler : class, ISyncModificationHandler<TContext>
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

        /// <summary>Forwards the modification to the bound handler; does nothing if no handler is bound.</summary>
        /// <param name="context">The context to modify.</param>
        public override void Apply(TContext context)
        {
            _handler?.Apply(context);
        }
    }
}