using System;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>
    /// Serialized instruction that delegates execution to a pooled, synchronous handler resolved via
    /// <see cref="IHandlerFactory"/>. Use this for dependency-injected instructions; for inline logic subclass
    /// <see cref="Instruction"/> directly.
    /// </summary>
    /// <typeparam name="THandler">The handler type this data binds to and injects itself into.</typeparam>
    [Serializable]
    public abstract class InstructionData<THandler> : Instruction, IHandlerBinder
        where THandler : class, ISyncInstructionHandler
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

        /// <summary>Forwards execution to the bound handler; does nothing if no handler is bound.</summary>
        public override void Apply()
        {
            _handler?.Apply();
        }
    }
}