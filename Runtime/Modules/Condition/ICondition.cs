using System;

namespace SST.Configurators
{
    /// <summary>
    /// A synchronous boolean predicate that can notify listeners when its result may have changed.
    /// </summary>
    public interface ICondition
    {
        /// <summary>Evaluates the condition.</summary>
        /// <returns><c>true</c> if the condition is currently satisfied.</returns>
        bool IsMet();

        /// <summary>Subscribes a callback invoked when the condition's result may have changed.</summary>
        /// <param name="onChanged">The callback to add.</param>
        void AddListener(Action onChanged);

        /// <summary>Unsubscribes a previously added change callback.</summary>
        /// <param name="onChanged">The callback to remove.</param>
        void RemoveListener(Action onChanged);
    }

    /// <summary>
    /// A synchronous boolean predicate evaluated against a <typeparamref name="TContext"/> that can notify
    /// listeners when its result may have changed. The context flows in per evaluation, mirroring
    /// <see cref="IModification{TContext}"/>; for the context-free variant see <see cref="ICondition"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    public interface ICondition<in TContext>
    {
        /// <summary>Evaluates the condition against the given context.</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> if the condition is currently satisfied for <paramref name="context"/>.</returns>
        bool IsMet(TContext context);

        /// <summary>Subscribes a callback invoked when the condition's result may have changed.</summary>
        /// <param name="onChanged">The callback to add.</param>
        void AddListener(Action onChanged);

        /// <summary>Unsubscribes a previously added change callback.</summary>
        /// <param name="onChanged">The callback to remove.</param>
        void RemoveListener(Action onChanged);
    }
}