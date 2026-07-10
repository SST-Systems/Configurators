using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized condition that delegates evaluation and change notification to a pooled handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected conditions; for inline logic subclass
    /// <see cref="Condition"/> directly.
    /// </summary>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class ConditionData<THandler> : ICondition, IHandlerBinder
        where THandler : class, IConditionHandler
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

        /// <summary>Evaluates the bound handler; returns <c>false</c> if no handler is bound.</summary>
        /// <returns><c>true</c> if the condition is currently satisfied.</returns>
        public bool IsMet() => _handler?.IsMet() ?? false;

        /// <summary>Forwards the change subscription to the bound handler.</summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged) => _handler?.AddListener(onChanged);

        /// <summary>Forwards the change unsubscription to the bound handler.</summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged) => _handler?.RemoveListener(onChanged);
    }
}