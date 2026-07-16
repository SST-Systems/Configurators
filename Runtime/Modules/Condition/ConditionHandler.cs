using System;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for a condition handler, exposing the injected data to subclasses and managing listener
    /// bookkeeping. Override <see cref="OnFirstListenerAdded"/> and <see cref="OnLastListenerRemoved"/> to
    /// subscribe to and unsubscribe from a data source, and call <see cref="NotifyChanged"/> to signal a change.
    /// Because handlers are pooled and reused, keep no per-call state outside <see cref="Data"/>.
    /// </summary>
    /// <typeparam name="TData">The serialized data type this handler operates on.</typeparam>
    public abstract class ConditionHandler<TData> : IConditionHandler where TData : class
    {
        /// <summary>The data injected via <see cref="SetData"/>, available to subclasses.</summary>
        protected TData Data { get; private set; }

        private Action _onChanged;

        /// <summary>Injects the serialized data, cast to <typeparamref name="TData"/>.</summary>
        /// <param name="data">The condition data, or <c>null</c> when unbound.</param>
        public void SetData(object data) => Data = data as TData;

        /// <summary>Evaluates the condition.</summary>
        /// <returns><c>true</c> if the condition is currently satisfied.</returns>
        public abstract bool IsMet();

        /// <summary>
        /// Adds a change listener, invoking <see cref="OnFirstListenerAdded"/> when the first one is added.
        /// </summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged)
        {
            var wasEmpty = _onChanged == null;
            _onChanged += onChanged;

            if (wasEmpty)
                OnFirstListenerAdded();
        }

        /// <summary>
        /// Removes a change listener, invoking <see cref="OnLastListenerRemoved"/> when the last one is removed.
        /// </summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged)
        {
            _onChanged -= onChanged;

            if (_onChanged == null)
                OnLastListenerRemoved();
        }

        /// <summary>Notifies all listeners that the condition's result may have changed.</summary>
        protected void NotifyChanged() => _onChanged?.Invoke();

        /// <summary>Called when the first listener is added; override to subscribe to a data source.</summary>
        protected virtual void OnFirstListenerAdded() { }

        /// <summary>Called when the last listener is removed; override to unsubscribe from a data source.</summary>
        protected virtual void OnLastListenerRemoved() { }
    }

    /// <summary>
    /// Base class for a context-aware condition handler, exposing the injected data to subclasses and managing
    /// listener bookkeeping. Override <see cref="OnFirstListenerAdded"/> and <see cref="OnLastListenerRemoved"/> to
    /// subscribe to and unsubscribe from a data source, and call <see cref="NotifyChanged"/> to signal a change.
    /// Because handlers are pooled and reused, keep no per-call state outside <see cref="Data"/>; the context flows
    /// in as a parameter. For the context-free variant see <see cref="ConditionHandler{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The serialized data type this handler operates on.</typeparam>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    public abstract class ConditionHandler<TData, TContext> : IConditionHandler<TContext> where TData : class
    {
        /// <summary>The data injected via <see cref="SetData"/>, available to subclasses.</summary>
        protected TData Data { get; private set; }

        private Action _onChanged;

        /// <summary>Injects the serialized data, cast to <typeparamref name="TData"/>.</summary>
        /// <param name="data">The condition data, or <c>null</c> when unbound.</param>
        public void SetData(object data) => Data = data as TData;

        /// <summary>Evaluates the condition against the given context.</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> if the condition is currently satisfied for <paramref name="context"/>.</returns>
        public abstract bool IsMet(TContext context);

        /// <summary>
        /// Adds a change listener, invoking <see cref="OnFirstListenerAdded"/> when the first one is added.
        /// </summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged)
        {
            var wasEmpty = _onChanged == null;
            _onChanged += onChanged;

            if (wasEmpty)
                OnFirstListenerAdded();
        }

        /// <summary>
        /// Removes a change listener, invoking <see cref="OnLastListenerRemoved"/> when the last one is removed.
        /// </summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged)
        {
            _onChanged -= onChanged;

            if (_onChanged == null)
                OnLastListenerRemoved();
        }

        /// <summary>Notifies all listeners that the condition's result may have changed.</summary>
        protected void NotifyChanged() => _onChanged?.Invoke();

        /// <summary>Called when the first listener is added; override to subscribe to a data source.</summary>
        protected virtual void OnFirstListenerAdded() { }

        /// <summary>Called when the last listener is removed; override to unsubscribe from a data source.</summary>
        protected virtual void OnLastListenerRemoved() { }
    }
}