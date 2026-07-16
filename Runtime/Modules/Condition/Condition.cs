using System;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for an inline condition. Subclass this and implement <see cref="IsMet"/>; call
    /// <see cref="NotifyChanged"/> when the result may have changed. For handler/DI-based conditions use
    /// <see cref="ConditionData{THandler}"/>.
    /// </summary>
    [Serializable]
    public abstract class Condition : ICondition
    {
        private Action _onChanged;

        /// <summary>Evaluates the condition.</summary>
        /// <returns><c>true</c> if the condition is currently satisfied.</returns>
        public abstract bool IsMet();

        /// <summary>Subscribes a callback invoked when the condition's result may have changed.</summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged) => _onChanged += onChanged;

        /// <summary>Unsubscribes a previously added change callback.</summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged) => _onChanged -= onChanged;

        /// <summary>Notifies all listeners that the condition's result may have changed.</summary>
        protected void NotifyChanged() => _onChanged?.Invoke();
    }

    /// <summary>
    /// Base class for an inline, context-aware condition. Subclass this and implement <see cref="IsMet"/>; call
    /// <see cref="NotifyChanged"/> when the result may have changed. For handler/DI-based conditions use
    /// <see cref="ConditionData{TContext, THandler}"/>. For the context-free variant see <see cref="Condition"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    [Serializable]
    public abstract class Condition<TContext> : ICondition<TContext>
    {
        private Action _onChanged;

        /// <summary>Evaluates the condition against the given context.</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> if the condition is currently satisfied for <paramref name="context"/>.</returns>
        public abstract bool IsMet(TContext context);

        /// <summary>Subscribes a callback invoked when the condition's result may have changed.</summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged) => _onChanged += onChanged;

        /// <summary>Unsubscribes a previously added change callback.</summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged) => _onChanged -= onChanged;

        /// <summary>Notifies all listeners that the condition's result may have changed.</summary>
        protected void NotifyChanged() => _onChanged?.Invoke();
    }
}