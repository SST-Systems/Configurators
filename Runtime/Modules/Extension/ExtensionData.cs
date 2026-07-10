using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized extension that delegates to a pooled, synchronous handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected extensions; for inline logic subclass
    /// <see cref="Extension{T}"/> directly.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class ExtensionData<TValue, THandler> : Extension<TValue>, IHandlerBinder
        where THandler : class, ISyncExtensionHandler<TValue>
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

        /// <summary>
        /// Returns the bound handler's value. Logs an error and returns <c>default</c> if the extension has not
        /// been resolved via <see cref="IExtensionManager.ResolveExtensions"/>.
        /// </summary>
        /// <returns>The handler's value, or <c>default</c> if no handler is bound.</returns>
        public override TValue GetValue()
        {
            if (_handler == null)
            {
                Debug.LogError($"[Configurator] {GetType().Name}: no handler bound. " +
                               "Call IExtensionManager.ResolveExtensions before requesting the value.");
                return default;
            }

            return _handler.GetValue();
        }
    }
}