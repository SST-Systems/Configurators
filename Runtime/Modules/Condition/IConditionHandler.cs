using System;

namespace SST.Configurators
{
    /// <summary>
    /// A condition handler that receives injected data, evaluates the predicate, and manages change subscriptions.
    /// Serves as the pool key for condition handlers.
    /// </summary>
    public interface IConditionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The condition data, or <c>null</c> when unbound.</param>
        void SetData(object data);

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
    /// Marker interface used as the pool key for context-aware condition handlers, mirroring
    /// <see cref="IModificationHandler"/>. Handlers are pooled and reused, so they must keep no per-call state;
    /// the context flows in as a parameter.
    /// </summary>
    public interface IContextConditionHandler { }

    /// <summary>
    /// A context-aware condition handler that receives injected data, evaluates the predicate against a
    /// <typeparamref name="TContext"/>, and manages change subscriptions. For the context-free variant see
    /// <see cref="IConditionHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    public interface IConditionHandler<in TContext> : IContextConditionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The condition data, or <c>null</c> when unbound.</param>
        void SetData(object data);

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