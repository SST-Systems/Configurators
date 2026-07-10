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
}