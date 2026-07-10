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
}